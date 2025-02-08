using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces
{
    public interface IAtualizaStatusRequisitanteUseCase : IUseCaseAsync<Gerenciador, StatusVideo, CancellationToken, bool>
    {
    }
}
