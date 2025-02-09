using Amazon;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Infra.AmazonS3;
using FIAP.Hackaton.ProcessarVideo.Infra.Mensageria;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Services;

public class MensageriaProcessarVideoTests
{
    private readonly Mock<AmazonSQSClient> _mockAmazonSQSClient;
    private readonly MensageriaProcessarVideo _mensageriaProcessarVideo;

    public MensageriaProcessarVideoTests()
    {
        _mockAmazonSQSClient = new Mock<AmazonSQSClient>(RegionEndpoint.SAEast1);

        _mensageriaProcessarVideo = new MensageriaProcessarVideo(_mockAmazonSQSClient.Object);

        var field = typeof(MensageriaProcessarVideo).GetField("_amazonSQS", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_mensageriaProcessarVideo, _mockAmazonSQSClient.Object);
    }

    [Fact]
    public async Task EnviarNotificacaoAsync_DeveEnviarMensagem_ComSucesso()
    {
        var queueUrl = "https://sqs.sa-east-1.amazonaws.com/123456789012/test-queue";
        var messageBody = "Test Message";

        _mockAmazonSQSClient
            .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        await _mensageriaProcessarVideo.EnviarNotificacaoAsync(queueUrl, messageBody);

        _mockAmazonSQSClient.Verify(x => x.SendMessageAsync(It.Is<SendMessageRequest>(req => req.QueueUrl == queueUrl && req.MessageBody == messageBody), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarNotificacaoAsync_ShouldThrowException()
    {
        var queueUrl = "https://sqs.sa-east-1.amazonaws.com/123456789012/test-queue";
        var messageBody = "Test Message";

        _mockAmazonSQSClient
            .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SQS Error"));
        var exception = await Xunit.Assert.ThrowsAsync<Exception>(() => _mensageriaProcessarVideo.EnviarNotificacaoAsync(queueUrl, messageBody));
        Xunit.Assert.Contains("Erro ao enviar a mensagem", exception.Message);
    }

    [Fact]
    public async Task DeletarMensagemSQSAsync_DeveDeletarMensagem_Comsucesso()
    {
        var queueUrl = "https://sqs.sa-east-1.amazonaws.com/123456789012/test-queue";
        var receiptHandle = "test-receipt-handle";

        _mockAmazonSQSClient
            .Setup(x => x.DeleteMessageAsync(It.IsAny<DeleteMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });
        await _mensageriaProcessarVideo.DeletarMensagemSQSAsync(queueUrl, receiptHandle);

        _mockAmazonSQSClient.Verify(x => x.DeleteMessageAsync(It.Is<DeleteMessageRequest>(req => req.QueueUrl == queueUrl && req.ReceiptHandle == receiptHandle), It.IsAny<CancellationToken>()), Times.Once);
    }
}
