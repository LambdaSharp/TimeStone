using System.Collections.Generic;
using Newtonsoft.Json;

namespace My.TimeStone.GameLogic
{

    public static class Types
    {
        public class Payload {
            //--- Properties ---
            [JsonProperty("wait_time")] 
            public string WaitTime = "1";
            
            [JsonProperty("status")] 
            public string Status;

            [JsonProperty("responded")]
            public string Responded;

            [JsonProperty("dead")]
            public string Dead;
            
            [JsonProperty("round_number")]
            public string RoundNumber;
            
            [JsonProperty("thanos_health")]
            public string ThanosHealth;
            
            [JsonProperty("earth_population")]
            public string EarthPopulation;
            
        }


        public class Avenger
        {
            //--- Fields ---
            public int id;
            public string name;
            public string power;

        }

        public class AvengerTeam
        {

            //--- Fields ---
            public List<Avenger> avengerTeam;

            //--- Constructor ---
            public AvengerTeam()
            {
                avengerTeam = new List<Avenger>();
            }

            //--- Methods ---
            public void AddAvengerToTeam(Avenger avenger)
            {
                avengerTeam.Add(avenger);
            }

        }

        public class Item {

            //--- Properties ---
            [JsonProperty]
            public string Id { get; set; }

            [JsonProperty]
            public string Value { get; set; }
        }

        public class Battlefield
        {
            //--- Properties ---
            [JsonProperty] public Item[] Items { get; set; }

        }
        
        public class Graveyard
        {
            //--- Properties ---
            [JsonProperty] public Avenger[] Avengers { get; set; }

        }

        public class BattleFieldItemIdentifier
        {
            //--- Fields ---
            public string Round;
            public string Guid;
            public string Team;
            public string Type;
             
        }
    }
}