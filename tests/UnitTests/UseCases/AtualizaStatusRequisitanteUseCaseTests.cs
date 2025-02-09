using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using GeradorDeFrames.Domain.Repositories;
using Moq;
using Xunit;

namespace FIAP.Hackaton.GeradorFrame.Processador.UnitTests.UseCases
{
    public class AtualizaStatusRequisitanteUseCaseTests
    {
        private readonly Mock<IGerenciadorRepository> _repositoryMock;
        private readonly AtualizaStatusRequisitanteUseCase _atualizaStatusRequisitanteUseCase;

        public AtualizaStatusRequisitanteUseCaseTests()
        {
            _repositoryMock = new Mock<IGerenciadorRepository>();
            _atualizaStatusRequisitanteUseCase = new AtualizaStatusRequisitanteUseCase(_repositoryMock.Object);
        }

        [Theory]
        [InlineData(StatusVideo.EmProcessamento)]
        [InlineData(StatusVideo.Processado)]
        public async Task Deve_Atualizar_Status_Quando_Id_Existir(StatusVideo novoStatus)
        {
            // Arrange
            var gerenciadorMock = GetGerenciadorMock();
            var request = Guid.NewGuid();

            _repositoryMock.Setup(it => it.GetByRequisicao(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(gerenciadorMock);

            _repositoryMock.Setup(it => it.Update(It.IsAny<Gerenciador>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(gerenciadorMock);

            // Act
            await _atualizaStatusRequisitanteUseCase.Execute(gerenciadorMock, novoStatus, default);

            // Assert
            _repositoryMock.Verify(it => it.Update(It.Is<Gerenciador>(g => g.Status == novoStatus), It.IsAny<CancellationToken>()), Times.Once);
        }

        private static Gerenciador GetGerenciadorMock() => new()
        {
            Id = Guid.NewGuid(),
            ArquivoOrigem = "",
            ArquivoDestino = "",
            DataCriacao = DateTime.Now,
            DataFimProcessamento = null,
            DataInicioProcessamento = null,
            Email = "teste@teste.com",
            Status = 0,
        };
    }
}
