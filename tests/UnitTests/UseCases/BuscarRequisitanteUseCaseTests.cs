using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using GeradorDeFrames.Domain.Repositories;
using Moq;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.UseCases;

public class BuscarRequisitanteUseCaseTests
{
    private readonly Mock<IGerenciadorRepository> _repositoryMock;
    private readonly BuscarRequisitanteUseCase _buscarRequisitanteUseCase;

    public BuscarRequisitanteUseCaseTests()
    {
        _repositoryMock = new Mock<IGerenciadorRepository>();
        _buscarRequisitanteUseCase = new BuscarRequisitanteUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task Deve_Retornar_Requisitante_Quando_Id_Existir()
    {
        var gerenciadorMock = GetGerenciadorMock();
        var request = new Guid();

        _repositoryMock.Setup(it => it.GetByRequisicao(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gerenciadorMock);

        // Act
        var resultado = await _buscarRequisitanteUseCase.Execute(request, default);

        // Assert
        Xunit.Assert.NotNull(resultado);
        Xunit.Assert.Equal(gerenciadorMock.Id, resultado.Id);
        Xunit.Assert.Equal("teste@teste.com", resultado.Email);
    }

    [Fact]
    public async Task Deve_Retornar_Null_Quando_Id_Nao_Existir()
    {
        var request = new Guid();

        var resultado = await _buscarRequisitanteUseCase.Execute(request, default);
        Xunit.Assert.Null(resultado);
    }

    private static Gerenciador GetGerenciadorMock() => new()
    {
        Id = new Guid(),
        ArquivoOrigem = "",
        ArquivoDestino = "",
        DataCriacao = DateTime.Now,
        DataFimProcessamento = null,
        DataInicioProcessamento = null,
        Email = "teste@teste.com",
        Status = 0,
    };
}
