using Newtonsoft.Json;

namespace FIAP.Hackaton.ProcessarVideo.Api;

public class ProcessarVideoEvento
{
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("eTag")]
    public Guid IdRequisicao { get; set; }
}
