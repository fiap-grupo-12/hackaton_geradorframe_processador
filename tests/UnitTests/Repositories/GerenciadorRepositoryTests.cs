using Amazon.DynamoDBv2.DataModel;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Infra.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.Repositories;

public class GerenciadorRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _mockDynamoDbContext;
    private readonly Mock<ILogger<GerenciadorRepository>> _mockLogger;
    private readonly GerenciadorRepository _repository;

    public GerenciadorRepositoryTests()
    {
        _mockDynamoDbContext = new Mock<IDynamoDBContext>();
        _mockLogger = new Mock<ILogger<GerenciadorRepository>>();
        _repository = new GerenciadorRepository(_mockDynamoDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByRequisicao_DeveRetornar_Sucesso()
    {
        // Arrange
        var idRequisicao = Guid.NewGuid().ToString();
        var expectedGerenciador = new Gerenciador { Id = new Guid(idRequisicao) };

        _mockDynamoDbContext
            .Setup(x => x.LoadAsync<Gerenciador>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGerenciador);

        // Act
        var result = await _repository.GetByRequisicao(idRequisicao, CancellationToken.None);

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(expectedGerenciador.Id, result.Id);
    }

    [Fact]
    public async Task GetByRequisicao_DeveRetornarException()
    {
        // Arrange
        var idRequisicao = Guid.NewGuid().ToString();

        _mockDynamoDbContext
            .Setup(x => x.LoadAsync<Gerenciador>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DynamoDB error"));

        // Act
        var result = await _repository.GetByRequisicao(idRequisicao, CancellationToken.None);

        // Assert
        Xunit.Assert.Null(result);

        _mockLogger.Verify(x =>
            x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao consultar dados do requisitante")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Post_DeveInserirRegistro_ComSucesso()
    {
        // Arrange
        var gerenciador = new Gerenciador();

        _mockDynamoDbContext
            .Setup(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.Post(gerenciador, CancellationToken.None);

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.NotEqual(Guid.Empty, result.Id);
        _mockDynamoDbContext.Verify(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_NaoDeveInserirRegistro()
    {
        // Arrange
        var gerenciador = new Gerenciador();

        _mockDynamoDbContext
            .Setup(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro ao cadastrar metados de mídia para gerenciamento."));

        var exception = Xunit.Assert.ThrowsAsync<Exception>(async () =>
        {
            await _repository.Post(gerenciador, CancellationToken.None);
        });

        Xunit.Assert.Contains("Erro ao cadastrar metados de mídia para gerenciamento", exception.Result.Message);
        _mockDynamoDbContext.Verify(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_DeveAtualizarRegistro_ComSucesso()
    {
        // Arrange
        var gerenciador = new Gerenciador { Id = Guid.NewGuid() };

        _mockDynamoDbContext
            .Setup(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.Update(gerenciador, CancellationToken.None);

        // Assert
        Xunit.Assert.NotNull(result);
        Xunit.Assert.Equal(gerenciador.Id, result.Id);
        _mockDynamoDbContext.Verify(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NaoDeveAtualizarRegistro()
    {
        var gerenciador = new Gerenciador { Id = Guid.NewGuid() };

        _mockDynamoDbContext
            .Setup(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro ao atualizar gerenciamento."));

        var exception = Xunit.Assert.ThrowsAsync<Exception>(async () =>
        {
            await _repository.Update(gerenciador, CancellationToken.None);
        });

        Xunit.Assert.Contains("Erro ao atualizar gerenciamento", exception.Result.Message);
        _mockDynamoDbContext.Verify(x => x.SaveAsync(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
