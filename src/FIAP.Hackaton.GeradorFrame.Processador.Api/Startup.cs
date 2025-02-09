using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.SQS;
using Application.UseCases;
using FIAP.GeradorDeFrames.Application.UseCases.Interfaces;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases;
using FIAP.Hackaton.GeradorFrame.Processador.Application.UseCases.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Domain.Interfaces;
using FIAP.Hackaton.ProcessarVideo.Infra.AmazonS3;
using FIAP.Hackaton.ProcessarVideo.Infra.Mensageria;
using FIAP.Hackaton.ProcessarVideo.Infra.Repositories;
using GeradorDeFrames.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FIAP.Hackaton.GeradorFrame.Processador.Api;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddTransient<IGerenciadorRepository, GerenciadorRepository>();
        services.AddTransient<IProcessarVideoUseCase, ProcessarVideoUseCase>();
        services.AddTransient<IMensageriaProcessarVideo, MensageriaProcessarVideo>();
        services.AddTransient<IBuscarRequisitanteUseCase, BuscarRequisitanteUseCase>();
        services.AddTransient<IAtualizaStatusRequisitanteUseCase, AtualizaStatusRequisitanteUseCase>();
        services.AddTransient<IAmazonS3Service, AmazonS3Service>();
        services.AddTransient<IDynamoDBContext, DynamoDBContext>();

        services.AddDefaultAWSOptions(new Amazon.Extensions.NETCore.Setup.AWSOptions() { Region = RegionEndpoint.SAEast1 });
        services.AddAWSService<IAmazonSQS>();
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddAWSService<IAmazonS3>();
        services.AddLogging();

        services.AddCors();
        return services.BuildServiceProvider();
    }
}
