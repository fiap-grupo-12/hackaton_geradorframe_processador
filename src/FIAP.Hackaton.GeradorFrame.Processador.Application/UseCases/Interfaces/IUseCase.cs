namespace FIAP.GeradorDeFrames.Application.UseCases.Interfaces;

public interface IUseCaseAsync<T1, T2, T3>
{
    Task<T3> Execute(T1 request,T2 cancellationToken);
}

public interface IUseCaseAsync<T1, T2, T3, T4>
{
    Task<T4> Execute(T1 request, T2 fileOut, T3 cancellationToken);
}
