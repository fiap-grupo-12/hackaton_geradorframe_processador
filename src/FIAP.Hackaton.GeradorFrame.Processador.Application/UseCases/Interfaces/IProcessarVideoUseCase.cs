using Amazon.Lambda.Core;
using FIAP.GeradorDeFrames.Application.Transport;

namespace FIAP.GeradorDeFrames.Application.UseCases.Interfaces;

public interface IProcessarVideoUseCase : IUseCaseAsync<ProcessarVideoInput, ILambdaContext,  bool>
{ }

