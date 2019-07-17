using System.Collections.Generic;
using Newtonsoft.Json;

namespace My.TimeStone.Gateway
{
    public class Types
    {
        public class GetItemsResponse {

            //--- Properties ---
            [JsonRequired]
            public List<GameLogic.Types.Item> Items;
        }
        
        public class AddItemsResponse {

            //--- Properties ---
            [JsonRequired]
            public string Response;
        }
        
        public class GetGraveResponse {

            //--- Properties ---
            [JsonRequired]
            public List<GameLogic.Types.Avenger> Avengers;
        }
        
        public class PostGraveResponse {

            //--- Properties ---
            [JsonRequired]
            public List<GameLogic.Types.Avenger> Avengers;
        }

        public class ClearResponse {
            //--- Properties ---
            [JsonRequired]
            public string Response;
        }

        
    }
}