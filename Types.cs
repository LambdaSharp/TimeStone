
namespace My.TimeStone
{
    public class Types
    {
        public class Payload {
            //--- Properties ---
            [JsonProperty("wait_time")]
            public string WaitTime;

            [JsonProperty("responded")]
            public string Responded;

            [JsonProperty("dead")]
            public string Dead;
        }
    }
}