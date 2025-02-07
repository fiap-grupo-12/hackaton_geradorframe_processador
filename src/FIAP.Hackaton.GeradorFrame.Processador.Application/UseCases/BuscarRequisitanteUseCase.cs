using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using GeradorDeFrames.Domain.Repositories;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases
{
    public class BuscarRequisitanteUseCase : IBuscarRequisitanteUseCase
    {
            private readonly IGerenciadorRepository _repository;
        public BuscarRequisitanteUseCase(IGerenciadorRepository repository)
        {
            _repository = repository;
        }

        public async Task<Gerenciador> Execute(Guid request, CancellationToken cancellationToken)
        {
            return new Gerenciador() { Email =  "teste@teste.com" };//await _repository.GetByRequisicao(request.ToString(), cancellationToken);


        }
    }
}
