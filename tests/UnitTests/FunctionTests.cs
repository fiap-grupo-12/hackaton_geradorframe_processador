using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FIAP.GeradorDeFrames.Application.Transport;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Api;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using Moq;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests;

public class FunctionTests
{
    private readonly Mock<IProcessarVideoUseCase> _mockProcessarVideoUseCase;
    private readonly Mock<IBuscarRequisitanteUseCase> _mockBuscarRequisitanteUseCase;
    private readonly Mock<IAtualizaStatusRequisitanteUseCase> _mockAtualizaStatusRequisitanteUseCase;
    private readonly Mock<IMensageriaProcessarVideo> _mockMensageriaProcessarVideo;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILambdaContext> _mockLambdaContext;
    private readonly Function _function;

    public FunctionTests()
    {
        _mockProcessarVideoUseCase = new Mock<IProcessarVideoUseCase>();
        _mockBuscarRequisitanteUseCase = new Mock<IBuscarRequisitanteUseCase>();
        _mockAtualizaStatusRequisitanteUseCase = new Mock<IAtualizaStatusRequisitanteUseCase>();
        _mockMensageriaProcessarVideo = new Mock<IMensageriaProcessarVideo>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        // Mockar o retorno de GetService para cada dependência
        _mockServiceProvider.Setup(x => x.GetService(typeof(IProcessarVideoUseCase))).Returns(_mockProcessarVideoUseCase.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IBuscarRequisitanteUseCase))).Returns(_mockBuscarRequisitanteUseCase.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IAtualizaStatusRequisitanteUseCase))).Returns(_mockAtualizaStatusRequisitanteUseCase.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IMensageriaProcessarVideo))).Returns(_mockMensageriaProcessarVideo.Object);

        // Inicializar a função com o serviço mockado
        _function = new Function(_mockServiceProvider.Object);
    }

    [Fact]
    public async Task FunctionHandler_DeveProcessarMensagem_ComSucesso()
    {
        var idRequisicao = new Guid("66838c96-836b-44a8-be7f-0be3d25de06a");

        var sqsEvent = GetSQSEventMock();

        var mockContext = new Mock<ILambdaContext>();
        mockContext.Setup(x => x.Logger).Returns(new Mock<ILambdaLogger>().Object);

        var gerenciador = new Gerenciador { Id = idRequisicao };
        var videoInput = new ProcessarVideoInput
        {
            IdRequisicao = idRequisicao,
            VideoName = "video.mp4",
            BucketName = "my-bucket",
            ArchiveKey = $"{idRequisicao}/video.mp4"
        };

        _mockBuscarRequisitanteUseCase.Setup(x => x.Execute(It.IsAny<Guid>(), default)).ReturnsAsync(gerenciador);
        _mockProcessarVideoUseCase.Setup(x => x.Execute(It.IsAny<ProcessarVideoInput>(), mockContext.Object)).ReturnsAsync(true);
        _mockAtualizaStatusRequisitanteUseCase.Setup(x => x.Execute(It.IsAny<Gerenciador>(), StatusVideo.EmProcessamento, default)).ReturnsAsync(true);
        _mockAtualizaStatusRequisitanteUseCase.Setup(x => x.Execute(It.IsAny<Gerenciador>(), StatusVideo.Processado, default)).ReturnsAsync(true);

        await _function.FunctionHandler(sqsEvent, mockContext.Object);

        _mockBuscarRequisitanteUseCase.Verify(x => x.Execute(It.IsAny<Guid>(), default), Times.AtLeastOnce);
        _mockProcessarVideoUseCase.Verify(x => x.Execute(It.IsAny<ProcessarVideoInput>(), mockContext.Object), Times.Once);
        _mockAtualizaStatusRequisitanteUseCase.Verify(x => x.Execute(It.IsAny<Gerenciador>(), StatusVideo.EmProcessamento, default), Times.Once);
        _mockAtualizaStatusRequisitanteUseCase.Verify(x => x.Execute(It.IsAny<Gerenciador>(), StatusVideo.Processado, default), Times.Once);
        _mockMensageriaProcessarVideo.Verify(x => x.EnviarNotificacaoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FunctionHandler_Exception()
    {
        // Arrange
        //var sqsEvent = null;

        var mockContext = new Mock<ILambdaContext>();
        mockContext.Setup(x => x.Logger).Returns(new Mock<ILambdaLogger>().Object);

        // Act
        await _function.FunctionHandler(null, mockContext.Object);

        // Assert
        _mockProcessarVideoUseCase.Verify(x => x.Execute(It.IsAny<ProcessarVideoInput>(), mockContext.Object), Times.Never);
        mockContext.Verify(x => x.Logger.LogError(It.Is<string>(s => s.Contains("Erro ao processar a mensagem da fila SQS"))), Times.Once);
    }

    private static SQSEvent GetSQSEventMock() => new()
    {
        Records =
                [
                    new SQSEvent.SQSMessage
                    {
                        Body = "{\"Records\":[{\"s3\":{\"object\":{\"key\":\"66838c96-836b-44a8-be7f-0be3d25de06a/video.mp4\"},\"bucket\":{\"name\":\"my-bucket\"}}}]}"
                    }
                ]
    };

}
