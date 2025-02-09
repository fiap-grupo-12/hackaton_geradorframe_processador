using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Application.UseCases;
using FFMpegCore;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.UseCases;

public class ProcessarVideoUseCaseTests
{
    private readonly Mock<ILogger<ProcessarVideoUseCase>> _loggerMock;
    private readonly Mock<IAmazonS3Service> _amazonS3ServiceMock;
    private readonly ProcessarVideoUseCase _useCase;

    public ProcessarVideoUseCaseTests()
    {
        _loggerMock = new Mock<ILogger<ProcessarVideoUseCase>>();
        _amazonS3ServiceMock = new Mock<IAmazonS3Service>();
        _useCase = new ProcessarVideoUseCase(_loggerMock.Object, _amazonS3ServiceMock.Object);
    }

    [Fact]
    public async Task Execute_DeveRetornarFalse()
    {
        // Arrange
        var input = new ProcessarVideoInput
        {
            IdRequisicao = Guid.NewGuid(),
            VideoName = "video.mp4",
            BucketName = "bucketName",
            ArchiveKey = "archiveKey"
        };

        var lambdaLoggerMock = new Mock<ILambdaLogger>();
        var mockContext = new Mock<ILambdaContext>();
        mockContext.Setup(c => c.Logger).Returns(lambdaLoggerMock.Object);

        // Simulate error during file download
        _amazonS3ServiceMock.Setup(s => s.DownloadFileFromS3Async(It.IsAny<InformacoesArquivo>(), input.BucketName, input.ArchiveKey))
            .ThrowsAsync(new Exception("S3 Download error"));

        // Act
        var result = await _useCase.Execute(input, mockContext.Object);

        // Assert
        Xunit.Assert.False(result);
        lambdaLoggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Erro ao processar vídeo"))), Times.Once);
    }   
}
