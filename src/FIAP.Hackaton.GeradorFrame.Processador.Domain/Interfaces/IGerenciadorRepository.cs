using FIAP.Hackaton.ProcessarVideo.Domain.Entities;

namespace GeradorDeFrames.Domain.Repositories;

public interface IGerenciadorRepository
{
    Task<Gerenciador> GetByRequisicao(string requisicao, CancellationToken cancellationToken);
    Task<Gerenciador> Post(Gerenciador gerenciador, CancellationToken cancellationToken);
    Task<Gerenciador> Update(Gerenciador gerenciador, CancellationToken cancellationToken);
}
