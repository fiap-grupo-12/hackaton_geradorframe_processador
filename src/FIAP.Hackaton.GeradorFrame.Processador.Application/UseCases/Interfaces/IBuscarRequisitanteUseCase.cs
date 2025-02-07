using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces
{
    public interface IBuscarRequisitanteUseCase : IUseCaseAsync<Guid, CancellationToken, Gerenciador>
    {
    }
}
