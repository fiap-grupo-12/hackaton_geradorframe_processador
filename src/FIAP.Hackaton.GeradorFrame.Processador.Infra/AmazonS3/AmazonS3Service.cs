using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FIAP.Hackaton.ProcessarVideo.Infra.AmazonS3;

public class AmazonS3Service : IAmazonS3Service
{
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1; // SP
    private readonly ILogger<AmazonS3Service> _logger;
    private AmazonS3Client _amazonS3Client;

    public AmazonS3Service(ILogger<AmazonS3Service> logger)
    {
        _logger = logger;
        _amazonS3Client = new AmazonS3Client(bucketRegion);
    }

    public async Task DownloadFileFromS3Async(InformacoesArquivo informacoesArquivo, string bucketName, string keyName)
    {
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation("Iniciando Download do video");
            // Cria uma requisição para baixar o objeto
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            // Baixa o objeto do S3
            Directory.CreateDirectory(informacoesArquivo.TempPath);
            using (var response = await _amazonS3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var fileStream = new FileStream(informacoesArquivo.TempPathVideo, FileMode.Create, FileAccess.Write))
            {
                var task = responseStream.CopyToAsync(fileStream);

                while (!task.IsCompleted)
                {
                    await Task.Delay(5000);
                    Console.WriteLine("Aguardando cópia do video");
                    
                }
                await task;
            }
            stopwatch.Stop();
            _logger.LogInformation($"Video copiado com sucesso em {stopwatch.ElapsedMilliseconds} Milisegundos");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao fazer Download ou cópia do vídeo {ex.Message}");
            throw ex;
        }
    }

    public async Task UploadFileToS3Async(string bucketName, string keyName, string filePath)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            FilePath = filePath
        };

        await _amazonS3Client.PutObjectAsync(request);
    }
}
