using Amazon.DynamoDBv2.DataModel;
using FIAP.Hackaton.ProcessarVideo.Domain.Enums;

namespace FIAP.Hackaton.ProcessarVideo.Domain.Entities;

[DynamoDBTable("GerenciadorTable")]
public class Gerenciador
{
    [DynamoDBHashKey("id")]
    public Guid Id { get; set; }

    [DynamoDBProperty("idRequisicao")]
    public Guid IdRequisicao { get; set; }

    [DynamoDBProperty("nome")]
    public string Nome { get; set; }

    [DynamoDBProperty("email")]
    public string Email { get; set; }

    [DynamoDBProperty("arquivoOrigem")]
    public string ArquivoOrigem { get; set; }

    [DynamoDBProperty("arquivoDestino")]
    public string ArquivoDestino { get; set; }

    [DynamoDBProperty("dataCriacao")]
    public DateTime DataCriacao { get; set; }

    [DynamoDBProperty("dataInicioProcessamento")]
    public DateTime? DataInicioProcessamento { get; set; }

    [DynamoDBProperty("dataFimProcessamento")]
    public DateTime? DataFimProcessamento { get; set; }

    [DynamoDBProperty("status")]
    public EStatusVideo Status { get; set; }
}
