using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using My.TimeStone.GameLogic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosKill {

    public class FunctionRequest : GameLogic.Types.Payload {
    }

    public class FunctionResponse : GameLogic.Types.Payload {
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
            
            var roundNumber = request.RoundNumber;
            var thanosPower = GameLogic.Methods.GetThanosRoundPowerLevel(roundNumber);
            var population = Int32.Parse(request.EarthPopulation);
            response.EarthPopulation = (population - thanosPower).ToString();
            
            var newItem = new Types.Item
            {
                Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(), "THANOS", "KILL"),
                Value = $"YOU HAVE FAILED TO DEFEND IN TIME. I HAVE KILLED {thanosPower.ToString()} PEOPLE {GameLogic.Methods.FormatThanosMessage(request.EarthPopulation, "PEOPLE REMAINING")}"
            };

            // Post the threaten message to API Gateway
            await Methods.PostBattlefield(newItem);

            return response;
        }
    }
}
