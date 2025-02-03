namespace FIAP.GeradorDeFrames.Application.UseCases.Interfaces;

public interface IUseCaseAsync<T1, T2, T3>
{
    Task<T3> Execute(T1 request, T2 cancellationToken);
}
