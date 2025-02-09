using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.ProcessarVideo.Infra.AmazonS3;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Services;

public class AmazonS3ServiceTests
{
    private readonly Mock<ILogger<AmazonS3Service>> _loggerMock;
    private readonly Mock<AmazonS3Client> _mockAmazonS3Client;
    private readonly AmazonS3Service _amazonS3Service;


    public AmazonS3ServiceTests()
    {
        _loggerMock = new Mock<ILogger<AmazonS3Service>>();
        _mockAmazonS3Client = new Mock<AmazonS3Client>(RegionEndpoint.USEast1);

        _amazonS3Service = new AmazonS3Service(_loggerMock.Object);

        var field = typeof(AmazonS3Service).GetField("_amazonS3Client", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_amazonS3Service, _mockAmazonS3Client.Object);
    }


    [Fact]
    public async Task DownloadFileFromS3Async_DeveBaixarArquivo_ComSucesso()
    {
        var bucketName = "test-bucket";
        var keyName = "test-file.mp4";
        var tempPath = Path.GetTempPath();
        var informacoesArquivo = new InformacoesArquivo { TempPath = tempPath, TempPathVideo = Path.Combine(tempPath, keyName) };

        var response = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(new byte[] { 1, 2, 3, 4 }) // Simula um arquivo
        };

        _mockAmazonS3Client
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _amazonS3Service.DownloadFileFromS3Async(informacoesArquivo, bucketName, keyName);

        Xunit.Assert.True(File.Exists(informacoesArquivo.TempPathVideo));

        _loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Iniciando Download do video")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        _loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Video copiado com sucesso")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileToS3Async_DeveUparArquivo_ComSucesso()
    {
        var bucketName = "test-bucket";
        var keyName = "test-video.mp4";
        var filePath = "./test-file.mp4";

        _mockAmazonS3Client
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        await _amazonS3Service.UploadFileToS3Async(bucketName, keyName, filePath);

        _mockAmazonS3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
