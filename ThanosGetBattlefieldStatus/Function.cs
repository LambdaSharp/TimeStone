using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using My.TimeStone.GameLogic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosGetBattlefieldStatus {

    public class FunctionRequest : Types.Payload {

    }

    public class FunctionResponse : Types.Payload {

    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request)
        {
            
            var response = new FunctionResponse
            {
                RoundNumber = request.RoundNumber,
                EarthPopulation = request.EarthPopulation,
                ThanosHealth = request.ThanosHealth
            };

            // get data from Gateway - look for Avengers on battlefield
            var battlefield = await GameLogic.Methods.GetBattlefieldFromGateway();
            var lastDefendItem = GameLogic.Methods.GetLastItemFromBattlefield(battlefield.Items.ToList(), "DEFEND", response.RoundNumber);
            var lastDefendItemParsed = GameLogic.Methods.ParseBattleFieldId(lastDefendItem);
 
            var lastThreatItem = GameLogic.Methods.GetLastItemFromBattlefield(battlefield.Items.ToList(), "THREAT", response.RoundNumber);
            var lastThreatItemParsed = GameLogic.Methods.ParseBattleFieldId(lastThreatItem);
            
 
            // if avengers are defending - battle
            if (lastDefendItemParsed.Type.Equals("DEFEND"))
            {
                response.Status = "fight";
            }
       
            // TODO: configure this seconds value
            // Move to kill state if no avengers reponse within 10 seconds
            if (!lastDefendItemParsed.Type.Equals("DEFEND"))
            {
                var lastThreatTimestamp = "";
                try
                {
                    lastThreatTimestamp = lastThreatItemParsed.Guid;
                }
                catch{};
                
                var lastDateTime = Convert.ToDateTime(lastThreatTimestamp);
                TimeSpan timeDifference = DateTime.UtcNow - lastDateTime;
                if (timeDifference.TotalSeconds > GameLogic.Constants.TIME_LIMIT)
                {
                    response.Status = "kill";
                };
            }

            return response;
        }
    }
}
