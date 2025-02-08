using System.Text.Json.Serialization;

namespace FIAP.Hackaton.GeradorFrame.Processador.Api
{
    public class S3EventMessage
    {
        public Record[] Records { get; set; }
    }

    public class Record
    {
        public string eventVersion { get; set; }
        public string eventSource { get; set; }
        public string awsRegion { get; set; }
        public DateTime eventTime { get; set; }
        public string eventName { get; set; }
        public Useridentity userIdentity { get; set; }
        public Requestparameters requestParameters { get; set; }
        public Responseelements responseElements { get; set; }
        public S3 s3 { get; set; }
    }

    public class Useridentity
    {
        public string principalId { get; set; }
    }

    public class Requestparameters
    {
        public string sourceIPAddress { get; set; }
    }

    public class Responseelements
    {
        public string xamzrequestid { get; set; }
        public string xamzid2 { get; set; }
    }

    public class S3
    {
        public string s3SchemaVersion { get; set; }
        public string configurationId { get; set; }
        public Bucket bucket { get; set; }
        [JsonPropertyName("object")]
        public Object _object { get; set; }
    }

    public class Bucket
    {
        public string name { get; set; }
        public Owneridentity ownerIdentity { get; set; }
        public string arn { get; set; }
    }

    public class Owneridentity
    {
        public string principalId { get; set; }
    }

    public class Object
    {
        public string key { get; set; }
        public int size { get; set; }
        public string eTag { get; set; }
        public string sequencer { get; set; }
    }
}