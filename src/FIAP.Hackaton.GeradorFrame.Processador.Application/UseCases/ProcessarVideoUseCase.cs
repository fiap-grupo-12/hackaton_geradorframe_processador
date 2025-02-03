using FFMpegCore;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Application.Helpers;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using GeradorDeFrames.Domain.Repositories;
using Newtonsoft.Json;
using System.Drawing;

namespace Application.UseCases;

public class ProcessarVideoUseCase(IGerenciadorRepository repository,
    IAmazonS3Service amazonS3Service,
    IMensageriaProcessarVideo mensageriaProcessarVideo) : IProcessarVideoUseCase
{
    public async Task<bool> Execute(ProcessarVideoInput input, CancellationToken cancellationToken)
    {
        string notificacao = "";

        var informacoesArquivo = new InfomracoesArquivo
        {
            Nome = Path.GetFileName(input.KeyName),
            NomeSemExtensao = FormataNomeArquivo(Path.GetFileName(input.KeyName)),
            Pasta = Path.GetDirectoryName(input.KeyName)?.Replace("\\", "/")
        };

        var gerenciadorByRequisicao = await repository.GetByRequisicao(informacoesArquivo.NomeSemExtensao, cancellationToken);

        var tempVideoPath = await amazonS3Service.DownloadFileFromS3Async(input.BucketName, input.KeyName, Path.Combine(Path.GetTempPath(), informacoesArquivo.Nome));
        var (result, outputPath) = await ProcessarVideoAsync(tempVideoPath);

        if (result)
        {
            var bucketName = Environment.GetEnvironmentVariable("bucket_files_out").ToString() ?? "";
            await amazonS3Service.UploadFileToS3Async(bucketName, informacoesArquivo.NomeSemExtensao, outputPath);

            if (gerenciadorByRequisicao != null)
            {
                gerenciadorByRequisicao.DataFimProcessamento = DateTime.Now;
                gerenciadorByRequisicao.Status = EStatusVideo.Processado;
                gerenciadorByRequisicao.ArquivoDestino = outputPath;

                await repository.Update(gerenciadorByRequisicao, cancellationToken);

                notificacao = JsonConvert.SerializeObject(new
                {
                    gerenciadorByRequisicao.Nome,
                    gerenciadorByRequisicao.Email,
                    Assunto = Environment.GetEnvironmentVariable("subject_email_notificacao"),
                    Corpo = $"Requisição {gerenciadorByRequisicao.IdRequisicao} executada com sucesso."
                });
            }
        }
        else
        {
            notificacao = JsonConvert.SerializeObject(new
            {
                gerenciadorByRequisicao.Nome,
                gerenciadorByRequisicao.Email,
                Assunto = Environment.GetEnvironmentVariable("subject_email_notificacao"),
                Corpo = $"Erro ao processar requisição {gerenciadorByRequisicao.IdRequisicao}."
            });
        }
        
        //Deleta mensagem da fila
        await mensageriaProcessarVideo.DeletarMensagemSQSAsync(Environment.GetEnvironmentVariable("sqs_processar_video_url"), 
            input.MessageReceiptHandle);

        //Publica mensagem para notificação
        await mensageriaProcessarVideo.EnviarNotificacaoAsync(Environment.GetEnvironmentVariable("sqs_enviar_notificacao_url"), 
            notificacao);

        return true;
    }

    private async Task<(bool, string)> ProcessarVideoAsync(string tempVideoPath)
    {
        string tempOutputPath = Path.Combine(Path.GetTempPath());
        var tempZipFilePath = Path.Combine(Path.GetTempPath(), $"{tempVideoPath}.zip");

        //Directory.CreateDirectory(outputFolder);
        try
        {
            var videoInfo = FFProbe.Analyse(tempVideoPath);
            var duration = videoInfo.Duration;

            var interval = TimeSpan.FromSeconds(20);

            var snapshotStream = new MemoryStream();

            for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
            {
                Console.WriteLine($"Processando frame: {currentTime}");

                var outputPath = Path.Combine(tempOutputPath, $"frame_at_{currentTime.TotalSeconds}.jpg");
                await FFMpeg.SnapshotAsync(tempVideoPath, outputPath, new Size(1920, 1080), currentTime);
            }

            var zipFilePath = ZipFilesHelper.ZipFolder(tempVideoPath, tempZipFilePath);

            return (true, zipFilePath);

        }
        catch(Exception error)
        {
            //Log de erro
            return (false, "");
        }
        finally
        {
            if (File.Exists(tempVideoPath))
                File.Delete(tempVideoPath);

            if (File.Exists(tempOutputPath))
                File.Delete(tempOutputPath);

            if (File.Exists(tempZipFilePath))
                File.Delete(tempOutputPath);
        }
        
    }

    private static String FormataNomeArquivo(string nomeArquivo)
    {
        int lastDotIndex = nomeArquivo.LastIndexOf('.');
        return lastDotIndex >= 0
            ? nomeArquivo.Substring(0, lastDotIndex)
            : nomeArquivo;
    }
}

public record InfomracoesArquivo
{
    public string Nome { get; set; }
    public string NomeSemExtensao { get; set; }
    public string Pasta { get; set; }
}
