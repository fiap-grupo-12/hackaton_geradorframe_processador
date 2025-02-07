using FFMpegCore;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Application.Helpers;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;

namespace Application.UseCases;

public class ProcessarVideoUseCase(
    ILogger<ProcessarVideoUseCase> logger,
    IAmazonS3Service amazonS3Service,
    IMensageriaProcessarVideo mensageriaProcessarVideo) : IProcessarVideoUseCase

{
    private readonly ILogger<ProcessarVideoUseCase> _logger = logger;
    private readonly IAmazonS3Service _amazonS3Service = amazonS3Service;
    private readonly string _bucketOut = Environment.GetEnvironmentVariable("bucket_files_out");

    public async Task<bool> Execute(ProcessarVideoInput input, CancellationToken cancellationToken)
    {
        var informacoesArquivo = new InformacoesArquivo
        {
            TempPath = Path.Combine(Path.GetTempPath(), input.IdRequisicao.ToString()),
            TempPathSnapshot = Path.Combine(Path.GetTempPath(), input.IdRequisicao.ToString() + "\\Snapshot"),
            TempPathVideo = Path.Combine(Path.GetTempPath(), input.ArchiveKey.ToString())
        };

        await _amazonS3Service.DownloadFileFromS3Async(informacoesArquivo, input.BucketName, input.ArchiveKey);

        var (result, outputPath) = await ProcessarVideoAsync(informacoesArquivo);

        if (result)
            await _amazonS3Service.UploadFileToS3Async(_bucketOut, input.IdRequisicao.ToString(), outputPath);

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        return result;
    }

    private async Task<(bool, string)> ProcessarVideoAsync(InformacoesArquivo informacoesArquivo)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _logger.LogInformation("Iniciando processamento do video");
        List<string> snapShots = new List<string>();

        try
        {
            Directory.CreateDirectory(informacoesArquivo.TempPathSnapshot);
            var videoInfo = FFProbe.Analyse(informacoesArquivo.TempPathVideo);
            var duration = videoInfo.Duration;

            _logger.LogInformation($"Duração do video {duration}");

            var interval = TimeSpan.FromSeconds(20);

            for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
            {
                _logger.LogInformation($"Processando frame: {currentTime}");

                var outputPath = Path.Combine(informacoesArquivo.TempPathSnapshot, $"frame_at_{currentTime.TotalSeconds}.png");
                snapShots.Add(outputPath);
                await FFMpeg.SnapshotAsync(informacoesArquivo.TempPathVideo, outputPath, new Size(1920, 1080), currentTime);
            }
            _logger.LogInformation("Gerando ZIP");
            var zipFilePath = ZipFilesHelper.ZipFolder(informacoesArquivo.TempPathSnapshot, informacoesArquivo.TempPath + ".zip");

            stopwatch.Stop();
            _logger.LogInformation($"Processamento concluido em {stopwatch.ElapsedMilliseconds} Milisegundos");
            return (true, zipFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao processar vídeo: {ex.Message}");
            return (false, "");
        }
        finally
        {
            if (File.Exists(informacoesArquivo.TempPathVideo))
                File.Delete(informacoesArquivo.TempPathVideo);

            foreach (var item in snapShots)
            {
                if (File.Exists(item))
                    File.Delete(item);
            }
            if (Directory.Exists(informacoesArquivo.TempPathSnapshot))
                Directory.Delete(informacoesArquivo.TempPathSnapshot);

            if (Directory.Exists(informacoesArquivo.TempPath))
                Directory.Delete(informacoesArquivo.TempPath);
        }
    }
}

