using Amazon.DynamoDBv2.DataModel;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FIAP.Hackaton.ProcessarVideo.Infra.Repositories;
public class MockGerenciadorRepository : IGerenciadorRepository
{
    private readonly List<Gerenciador> _gerenciadores = new List<Gerenciador>();

    public MockGerenciadorRepository()
    {
        // Adiciona um registro mockado para testes
        _gerenciadores.Add(new Gerenciador
        {
            Id = new Guid("123e4567-e89b-12d3-a456-426614174000"),
            Email = "teste@teste.com",
            ArquivoOrigem = "video.mp4",
            ArquivoDestino = "destino.zip",
            DataCriacao = DateTime.Now,
            DataInicioProcessamento = DateTime.Now,
            DataFimProcessamento = null,
            Status = StatusVideo.EmProcessamento
        });
    }

    public Task<Gerenciador> GetByRequisicao(string requisicao, CancellationToken cancellationToken)
    {
        var gerenciador = _gerenciadores.Find(g => g.Id.ToString() == requisicao);
        return Task.FromResult(gerenciador);
    }

    public Task<Gerenciador> Post(Gerenciador gerenciador, CancellationToken cancellationToken)
    {
        _gerenciadores.Add(gerenciador);
        return Task.FromResult(gerenciador);
    }

    public Task<Gerenciador> Update(Gerenciador gerenciador, CancellationToken cancellationToken)
    {
        var index = _gerenciadores.FindIndex(g => g.Id == gerenciador.Id);
        if (index != -1)
        {
            _gerenciadores[index] = gerenciador;
        }
        return Task.FromResult(gerenciador);
    }
}
