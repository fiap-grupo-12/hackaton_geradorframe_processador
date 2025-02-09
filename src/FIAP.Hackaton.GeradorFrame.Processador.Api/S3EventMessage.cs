using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FIAP.Hackaton.GeradorFrame.Processador.Api
{
    public class S3EventMessage
    {
        public Record[] Records { get; set; }
    }

    public class Record
    {
        public S3 s3 { get; set; }
    }

    public class S3
    {
        public Bucket bucket { get; set; }
        [JsonPropertyName("object")]
        public Object _object { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Bucket
    {
        public string name { get; set; }
    }

    public class Object
    {
        public string key { get; set; }
    }
}