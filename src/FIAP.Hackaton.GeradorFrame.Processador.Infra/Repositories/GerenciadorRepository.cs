
using Amazon.DynamoDBv2.DataModel;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FIAP.Hackaton.ProcessarVideo.Infra.Repositories;

public class GerenciadorRepository : IGerenciadorRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<GerenciadorRepository> _logger;

    public GerenciadorRepository(IDynamoDBContext context, ILogger<GerenciadorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Gerenciador> GetByRequisicao(string idRequisicao, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Buscando informações do requisitante.");

            return await _context.LoadAsync<Gerenciador>(new Guid(idRequisicao));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao consultar dados do requisitante. {ex}");
            return null;
            //throw new Exception($"Erro ao consultar dados do requisitante. {ex}");
        }
    }

    public async Task<Gerenciador> Post(Gerenciador gerenciador, CancellationToken cancellationToken)
    {
        try
        {
            gerenciador.Id = Guid.NewGuid();
            await _context.SaveAsync(gerenciador, cancellationToken);
            return gerenciador;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao cadastrar metados de mídia para gerenciamento. {ex}");
        }
    }

    public async Task<Gerenciador> Update(Gerenciador gerenciador, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveAsync(gerenciador, cancellationToken);
            return gerenciador;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao atualizar gerenciamento. {ex}");
        }
    }
}
