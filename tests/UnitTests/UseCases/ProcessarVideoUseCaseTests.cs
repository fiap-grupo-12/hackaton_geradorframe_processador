using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Application.UseCases;
using FFMpegCore;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.GeradorFrame.Processador.Domain.Model;
using FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Resources;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.UseCases
{
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

        //[Fact]
        //public async Task Execute_ShouldReturnTrue_WhenProcessingVideoIsSuccessful()
        //{
        //    // Arrange
        //    var input = new ProcessarVideoInput
        //    {
        //        IdRequisicao = Guid.NewGuid(),
        //        VideoName = "video.mp4",
        //        BucketName = "bucketName",
        //        ArchiveKey = "archiveKey"
        //    };

        //    var lambdaLoggerMock = new Mock<ILambdaLogger>();
        //    var mockContext = new Mock<ILambdaContext>();
        //    mockContext.Setup(c => c.Logger).Returns(lambdaLoggerMock.Object);

        //    // Simulate successful file download and upload
        //    _amazonS3ServiceMock.Setup(s => s.DownloadFileFromS3Async(It.IsAny<InformacoesArquivo>(), input.BucketName, input.ArchiveKey))
        //        .Returns(Task.CompletedTask);
        //    _amazonS3ServiceMock.Setup(s => s.UploadFileToS3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns(Task.CompletedTask);

        //    var mediaAnalysisMock = new Mock<IMediaAnalysis>();
        //    mediaAnalysisMock.Setup(m => m.Duration).Returns(TimeSpan.FromSeconds(10));
        //    //Mock.Get(typeof(FFProbe))  // Usando Mock para a classe estática FFProbe
        //    //    .Setup(f => f.Analyse(It.IsAny<string>()))  // Substituindo o método Analyse
        //    //    .Returns(mediaAnalysisMock.Object);  // Retornando o mock de IMediaAnalysis
        //    // Mock do método estático FFProbe.Analyse


        //    // Act
        //    var result = await _useCase.Execute(input, mockContext.Object);

        //    // Assert
        //    Xunit.Assert.True(result);
        //    lambdaLoggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce); // Verify that log methods were called at least once
        //}

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

            //[Fact]
            //public async Task Execute_ShouldReturnTrue_WhenFileIsProcessedSuccessfullyAndCleanedUp()
            //{
            //    // Arrange
            //    var input = new ProcessarVideoInput
            //    {
            //        IdRequisicao = Guid.NewGuid(),
            //        VideoName = "video.mp4",
            //        BucketName = "bucketName",
            //        ArchiveKey = "archiveKey"
            //    };
            //    var mockContext = new Mock<ILambdaContext>();
            //    mockContext.Setup(c => c.Logger).Returns((ILambdaLogger)_loggerMock.Object);

            //    // Simulate successful file download and upload
            //    _amazonS3ServiceMock.Setup(s => s.DownloadFileFromS3Async(It.IsAny<InformacoesArquivo>(), input.BucketName, input.ArchiveKey))
            //        .Returns(Task.CompletedTask);
            //    _amazonS3ServiceMock.Setup(s => s.UploadFileToS3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            //        .Returns(Task.CompletedTask);

            //    // Act
            //    var result = await _useCase.Execute(input, mockContext.Object);

            //    // Assert
            //    Xunit.Assert.True(result);

            //    // Verify that temporary files were cleaned up after processing
            //    _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Limpeza concluída!"))), Times.Once);
            //}

            //[Fact]
            //public async Task Execute_ShouldReturnFalse_WhenFFmpegThrowsException()
            //{
            //    // Arrange
            //    var input = new ProcessarVideoInput
            //    {
            //        IdRequisicao = Guid.NewGuid(),
            //        VideoName = "video.mp4",
            //        BucketName = "bucketName",
            //        ArchiveKey = "archiveKey"
            //    };
            //    var mockContext = new Mock<ILambdaContext>();
            //    mockContext.Setup(c => c.Logger).Returns((ILambdaLogger)_loggerMock.Object);

            //    // Simulate error in FFmpeg processing
            //    _amazonS3ServiceMock.Setup(s => s.DownloadFileFromS3Async(It.IsAny<InformacoesArquivo>(), input.BucketName, input.ArchiveKey))
            //        .Returns(Task.CompletedTask);
            //    _amazonS3ServiceMock.Setup(s => s.UploadFileToS3Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            //        .Returns(Task.CompletedTask);

            //    var errorMessage = "FFmpeg processing error";
            //    _useCase.GetType().GetMethod("ProcessarVideoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            //        .Invoke(_useCase, new object[] { null, mockContext.Object, input.IdRequisicao.ToString() });

            //    // Act
            //    var result = await _useCase.Execute(input, mockContext.Object);

            //    // Assert
            //    Xunit.Assert.False(result);
            //    _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Erro ao processar vídeo"))), Times.Once); // Verify error logging
            //}
        }
}
