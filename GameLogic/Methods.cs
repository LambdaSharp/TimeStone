using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using My.TimeStone.GameLogic;

namespace My.TimeStone.GameLogic
{
    public static class Methods
    {

        public static async Task InitializeBattlefield() {
            // instantiate http client
            var httpClient = new HttpClient();
            
            // clear battlefield
            var response = await httpClient.GetAsync($"{GameLogic.Constants.APIGATEWAY_URL}/clear");
        }

        public static async Task<string> PostBattlefield(Types.Item input)
        {

            // instantiate http client
            var httpClient = new HttpClient();

            // deserialize current battlefield
            var serializedObj = JsonConvert.SerializeObject(input);

            // post message
            var obj = new StringContent(serializedObj, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Constants.APIGATEWAY_URL}/items", obj);

            // read response
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
        public static async Task<string> PostGrave(List<Types.Avenger> avengers)
        {

            // instantiate http client
            var httpClient = new HttpClient();

            // deserialize current battlefield
            var serializedObj = JsonConvert.SerializeObject(avengers);

            // post message
            var obj = new StringContent(serializedObj, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Constants.APIGATEWAY_URL}/grave", obj);

            // read response
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
        public static async Task<Types.Battlefield> GetBattlefieldFromGateway()
        {
            
            // instantiate http client
            var httpClient = new HttpClient();
            
            // get current battlefield
            var response = await httpClient.GetAsync($"{GameLogic.Constants.APIGATEWAY_URL}/items");
            var strContents = await response.Content.ReadAsStringAsync();

            // deserialize battlefield response
            var cleaned = JsonConvert.DeserializeObject<Types.Battlefield>(strContents);
            
            return cleaned;
        }
        public static async Task<Types.Graveyard> GetGrave()
        {
            
            // instantiate http client
            var httpClient = new HttpClient();
            
            // get current battlefield
            var response = await httpClient.GetAsync($"{GameLogic.Constants.APIGATEWAY_URL}/grave");
            var strContents = await response.Content.ReadAsStringAsync();

            // deserialize battlefield response
            var cleaned = JsonConvert.DeserializeObject<Types.Graveyard>(strContents);
            
            return cleaned;
        }
        

        public static int GetThanosRoundUnitCount(string roundString)
        {
            var roundNumber = Int32.Parse(roundString);

            // number of units = round # squared
            var unitCount = roundNumber * roundNumber;

            // max number of units;
            if(unitCount > Constants.UNIT_CAP_MAX) {
                unitCount = Constants.UNIT_CAP_MAX;
            }
            return unitCount;
        }

        public static int GetThanosRoundPowerLevel(string roundNumber)
        {
            var unitCount = GetThanosRoundUnitCount(roundNumber);
            var powerLevel = unitCount * Constants.AVERAGE_POWER;
            return powerLevel;
        }

        public static Types.BattleFieldItemIdentifier ParseBattleFieldId(Types.Item singleItem)
        {

            var split = singleItem.Id.Split("::");
            
            return new Types.BattleFieldItemIdentifier
            {
                Round = split[0],
                Guid = split[1],
                Team = split[2],
                Type = split[3]
            };
        }

        public static string  GetFormattedThanosUnitLevelIdentifier(string input)
        {
            var identifier = "%";
            // refine logic for power level
            return ($"{identifier}input{identifier}").ToString();
        }

        public static string FormatThanosMessage(string value, string units)
        {
            return $"({value} {units.ToUpper()})";

        }

        public static string FormatBattlefieldId(string round, string time, string type, string action)
        {
            return $"R{round}::{time}::{type.ToUpper()}::{action.ToUpper()}";
        }

        public static Types.Item GetLastItemFromBattlefield(List<Types.Item>battlefield, string action, string round)
        {
            var lastItem = new Types.Item
            {
                Id = "_::_::_::_",
                Value = ""
            };
            foreach (var item in battlefield)
            {
                if (item.Id.Contains(action) && item.Id.Contains($"R{round}"))
                {
                    lastItem = item;
                }
            }
            return lastItem;
        }
        
        public static Types.Item GetLastItemFromBattlefield(List<Types.Item>battlefield, string action)
        {
            var lastItem = new Types.Item
            {
                Id = "_::_::_::_",
                Value = ""
            };
            foreach (var item in battlefield)
            {
                if (item.Id.Contains(action))
                {
                    lastItem = item;
                }
            }
            return lastItem;
        }
    }
}