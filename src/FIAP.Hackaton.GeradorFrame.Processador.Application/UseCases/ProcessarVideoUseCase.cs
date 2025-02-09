using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using FFMpegCore;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Application.Helpers;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ProcessarVideoUseCase : IProcessarVideoUseCase
    {
        private readonly ILogger<ProcessarVideoUseCase> _logger;
        private readonly IAmazonS3Service _amazonS3Service;
        private readonly string _bucketOut = Environment.GetEnvironmentVariable("bucket_files_out");

        public ProcessarVideoUseCase(ILogger<ProcessarVideoUseCase> logger, IAmazonS3Service amazonS3Service)
        {
            _logger = logger;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<bool> Execute(ProcessarVideoInput input, ILambdaContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            context.Logger.LogInformation($"🛠️ Iniciando processamento do vídeo para ID: {input.IdRequisicao}");
            context.Logger.LogInformation($"🎥 Nome do vídeo: {input.VideoName} | 📂 Bucket: {input.BucketName}");

            string tempPath = Path.Combine("/tmp", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            var informacoesArquivo = new InformacoesArquivo
            {
                TempPath = tempPath,
                TempPathSnapshot = Path.Combine(tempPath, "Snapshot"),
                TempPathVideo = Path.Combine(tempPath, "video.mp4")
            };

            try
            {
                context.Logger.LogInformation("⬇️ Baixando arquivo do S3...");
                await _amazonS3Service.DownloadFileFromS3Async(informacoesArquivo, input.BucketName, input.ArchiveKey);
                context.Logger.LogInformation("✅ Download concluído!");

                context.Logger.LogInformation("🎬 Iniciando processamento do vídeo...");
                var (result, outputPath) = await ProcessarVideoAsync(informacoesArquivo, context, input.IdRequisicao.ToString());

                stopwatch.Stop();
                context.Logger.LogInformation($"🏁 Processamento finalizado em {stopwatch.ElapsedMilliseconds} ms");

                return result;
            }
            catch (Exception ex)
            {
                context.Logger.LogInformation($"❌ Erro ao processar vídeo: {ex.Message}");
                context.Logger.LogInformation($"🛑 Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                context.Logger.LogInformation("🧹 Limpando arquivos temporários...");
                CleanupFiles(informacoesArquivo, context);
                context.Logger.LogInformation("✅ Limpeza concluída!");
            }
        }

        private async Task<(bool, string)> ProcessarVideoAsync(InformacoesArquivo informacoesArquivo, ILambdaContext context, string idRequisicao)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.Logger.LogInformation("Iniciando o processamento do vídeo...");

            List<string> snapShots = new List<string>();

            try
            {
                Directory.CreateDirectory(informacoesArquivo.TempPathSnapshot);
                context.Logger.LogInformation("Criado diretorio temp " + informacoesArquivo.TempPathSnapshot);

                GlobalFFOptions.Configure(options =>
                {
                    options.BinaryFolder = "/var/task/ffmpeg-linux"; // Diretório onde o FFmpeg e o FFProbe estão
                    options.TemporaryFilesFolder = "/tmp";
                });
                context.Logger.LogInformation("GlobalFFOptions configuradas");

                // Log dos diretórios antes de executar FFProbe
                LogDirectoryContents("/var/task", context);
                LogDirectoryContents("/var/task/ffmpeg-linux", context);

                VerifyFFmpegPermissions(context);

                var videoInfo = FFProbe.Analyse(informacoesArquivo.TempPathVideo);
                context.Logger.LogInformation("Vídeo analisado");
                var duration = videoInfo.Duration;
                context.Logger.LogInformation($"🎞️ Duração do vídeo: {duration}");

                var interval = TimeSpan.FromSeconds(5);

                for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
                {
                    var outputPath = Path.Combine(informacoesArquivo.TempPathSnapshot, $"frame_at_{currentTime.TotalSeconds}.png");
                    snapShots.Add(outputPath);

                    context.Logger.LogInformation($"📸 Capturando frame em {currentTime}...");
                    await FFMpeg.SnapshotAsync(informacoesArquivo.TempPathVideo, outputPath, new Size(1920, 1080), currentTime);
                }

                context.Logger.LogInformation("📦 Compactando frames em arquivo ZIP...");
                var zipFilePath = ZipFilesHelper.ZipFolder(informacoesArquivo.TempPathSnapshot, informacoesArquivo.TempPath + ".zip");

                stopwatch.Stop();
                context.Logger.LogInformation($"✅ Processamento finalizado em {stopwatch.ElapsedMilliseconds} ms");

                context.Logger.LogInformation("⬆️ Enviando arquivo processado para o S3...");
                await _amazonS3Service.UploadFileToS3Async(_bucketOut, idRequisicao, zipFilePath);
                context.Logger.LogInformation("✅ Upload concluído!");

                return (true, zipFilePath);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"❌ Erro ao processar vídeo: {ex.ToString()}");
                context.Logger.LogError($"🛑 Stack Trace: {ex.StackTrace}");
                return (false, string.Empty);
            }
            finally
            {
                CleanupFiles(informacoesArquivo, context);
            }
        }

        private void CleanupFiles(InformacoesArquivo informacoesArquivo, ILambdaContext context)
        {
            try
            {
                context.Logger.LogInformation("🧹 Executando limpeza de arquivos temporários...");

                if (File.Exists(informacoesArquivo.TempPathVideo))
                {
                    context.Logger.LogInformation($"🗑️ Deletando arquivo de vídeo: {informacoesArquivo.TempPathVideo}");
                    File.Delete(informacoesArquivo.TempPathVideo);
                }

                if (Directory.Exists(informacoesArquivo.TempPathSnapshot))
                {
                    context.Logger.LogInformation($"🗑️ Deletando diretório de snapshots: {informacoesArquivo.TempPathSnapshot}");
                    Directory.Delete(informacoesArquivo.TempPathSnapshot, true);
                }

                if (Directory.Exists(informacoesArquivo.TempPath))
                {
                    context.Logger.LogInformation($"🗑️ Deletando diretório temporário: {informacoesArquivo.TempPath}");
                    Directory.Delete(informacoesArquivo.TempPath, true);
                }

                context.Logger.LogInformation("✅ Limpeza concluída!");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"❌ Erro ao limpar arquivos temporários: {ex.Message}");
            }
        }

        private void LogDirectoryContents(string path, ILambdaContext context)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    context.Logger.LogInformation($"📂 Listando diretórios e arquivos em: {path}");

                    var directories = Directory.GetDirectories(path);
                    if (directories.Length > 0)
                    {
                        context.Logger.LogInformation($"📁 Diretórios encontrados:");
                        foreach (var dir in directories)
                        {
                            context.Logger.LogInformation($"  📂 {dir}");
                        }
                    }
                    else
                    {
                        context.Logger.LogInformation("❌ Nenhum diretório encontrado.");
                    }

                    var files = Directory.GetFiles(path);
                    if (files.Length > 0)
                    {
                        context.Logger.LogInformation($"📄 Arquivos encontrados:");
                        foreach (var file in files)
                        {
                            context.Logger.LogInformation($"  📄 {file}");
                        }
                    }
                    else
                    {
                        context.Logger.LogInformation("❌ Nenhum arquivo encontrado.");
                    }
                }
                else
                {
                    context.Logger.LogError($"❌ Diretório {path} não encontrado!");
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"❌ Erro ao listar diretório {path}: {ex.Message}");
            }
        }

        private void VerifyFFmpegPermissions(ILambdaContext context)
        {
            string ffprobePath = "/var/task/ffmpeg-linux/ffprobe";

            if (File.Exists(ffprobePath))
            {
                context.Logger.LogInformation($"✅ FFProbe encontrado em {ffprobePath}");

                try
                {
                    // Testa se o arquivo pode ser executado
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = ffprobePath,
                        Arguments = "-version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        context.Logger.LogInformation($"🔍 FFProbe versão: {output}");
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"❌ FFProbe não pode ser executado: {ex.Message}");
                }
            }
            else
            {
                context.Logger.LogError($"❌ FFProbe não encontrado em {ffprobePath}");
            }
        }


    }
}
