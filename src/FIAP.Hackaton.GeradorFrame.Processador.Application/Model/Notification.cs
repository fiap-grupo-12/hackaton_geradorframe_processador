using FIAP.Hackaton.ProcessarVideo.Domain.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FIAP.Hackaton.GeradorFrame.Processador.Application.Model
{
    [ExcludeFromCodeCoverage]
    public class Notification
    {
        [JsonPropertyName("Email")]
        public string Destinatario { get; set; }

        [JsonPropertyName("Nome")]
        public string Nome { get; set; }

        [JsonPropertyName("Assunto")]
        public string Assunto { get; set; }

        [JsonPropertyName("Corpo")]
        public string Mensagem { get; set; }

        public Notification() { }

        public Notification(Gerenciador requisitante, bool Erro = false)
        {
            Destinatario = requisitante.Email;
            Nome = "Usuario";

            if (Erro)
            {
                Assunto = "Erro ao processar video.";
                Mensagem = $"Erro ao processar requisição {requisitante.Id}.";
            }
            else
            {
                Assunto = "Video processado com sucesso.";
                Mensagem = $"Requisição {requisitante.Id} executada com sucesso.";
            }

        }

        public string GerarEmailJsonErro(Gerenciador requisitante, bool Erro = false)
        {
            var email = new Notification(requisitante, Erro);
            string notif = JsonSerializer.Serialize(email);
            return notif;
        }
    }
}
