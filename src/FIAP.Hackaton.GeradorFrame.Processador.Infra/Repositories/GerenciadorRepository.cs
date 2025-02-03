
using Amazon.DynamoDBv2.DataModel;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using GeradorDeFrames.Domain.Repositories;

namespace FIAP.Hackaton.ProcessarVideo.Infra.Repositories;

public class GerenciadorRepository : IGerenciadorRepository
{
    private readonly IDynamoDBContext _context;

    public GerenciadorRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    public async Task<Gerenciador> GetByRequisicao(string idRequisicao, CancellationToken cancellationToken)
    {
        try
        {
            var condition = new List<ScanCondition>()
            {
                new("idRequisicao", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, idRequisicao)
            };

            var gerenciadores = await _context.ScanAsync<Gerenciador>(default)
                .GetRemainingAsync();


            return gerenciadores.FirstOrDefault(it => it.IdRequisicao.ToString().Equals(idRequisicao));
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao consultar dados de gerenciamento de mídia. {ex}");
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
