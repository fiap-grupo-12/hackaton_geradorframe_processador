using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;
using GeradorDeFrames.Domain.Repositories;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases
{
    public class AtualizaStatusRequisitanteUseCase : IAtualizaStatusRequisitanteUseCase
    {
        private readonly IGerenciadorRepository _repository;
        public AtualizaStatusRequisitanteUseCase(IGerenciadorRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Execute(Gerenciador request, StatusVideo status, CancellationToken cancellationToken)
        {
            if (StatusVideo.Processado == status)
            {
                request.DataFimProcessamento = DateTime.Now;
                request.ArquivoDestino = request.IdRequisicao.ToString();
            }

            request.Status = status;
            await _repository.Update(request, cancellationToken);

            return true;
        }
    }
}
