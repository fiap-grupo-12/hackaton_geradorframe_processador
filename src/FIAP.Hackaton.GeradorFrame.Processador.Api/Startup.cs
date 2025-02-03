using Amazon.DynamoDBv2;
using Amazon.SQS;
using Application.UseCases;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Infra.Repositories;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Hackaton.GeradorFrame.Processador.Api;

[Amazon.Lambda.Annotations.LambdaStartup]
[ExcludeFromCodeCoverage]
public class Startup
{
    public Startup()
    { }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IGerenciadorRepository, GerenciadorRepository>();
        services.AddTransient<IProcessarVideoUseCase, ProcessarVideoUseCase>();

        services.AddAWSService<IAmazonSQS>();
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddCors();
    }
}
