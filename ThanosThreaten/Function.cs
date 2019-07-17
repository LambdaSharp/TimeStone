using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using LambdaSharp.CustomResource;
using My.TimeStone.GameLogic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosThreaten {

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
           
            if (string.IsNullOrEmpty(request.RoundNumber))
            {
                var introItem = new Types.Item
                {
                    Id = $"{DateTime.UtcNow}::THANOS::WELCOME",
                    Value = $"WELCOME TO THE BATTLEFIELD, AVENGERS - ARE YOU READY TO FACE YOUR DEATH?"
                };

                // clear battlefield if there is no payload
                await Methods.InitializeBattlefield();

                // Post the intro message to the gateway
                await Methods.PostBattlefield(introItem);
                
                response.RoundNumber = "1";
                response.EarthPopulation = GameLogic.Constants.EARTH_POPULATION.ToString();
                response.ThanosHealth = GameLogic.Constants.THANOS_HEALTH.ToString();
            }

            
            // Post Round Number
            var roundUpdateItem = new Types.Item
            {
                Id = GameLogic.Methods.FormatBattlefieldId(response.RoundNumber, DateTime.UtcNow.ToString(),"THANOS", "UPDATE"),
                Value = $"GET READY FOR THE NEXT WAVE OF MINIONS {GameLogic.Methods.FormatThanosMessage(response.RoundNumber, "ROUND")}, {GameLogic.Methods.FormatThanosMessage(response.EarthPopulation, "PEOPLE REMAINING")}, {GameLogic.Methods.FormatThanosMessage(response.ThanosHealth, "HEALTH")} REMAINING"
            };
            await Methods.PostBattlefield(roundUpdateItem);
            
            
            // var thanosPower = Methods.GetThanosRoundPowerLevel(response.RoundNumber);
            var thanosUnitCount = Methods.GetThanosRoundUnitCount(response.RoundNumber);
            var newItem = new Types.Item
            {
                Id = GameLogic.Methods.FormatBattlefieldId(response.RoundNumber, DateTime.UtcNow.ToString(),"THANOS", "THREAT"),
                Value = $"ATTACKING - CHOOSE YOUR BEST {GameLogic.Methods.FormatThanosMessage(thanosUnitCount.ToString(), "UNITS")}, YOU HAVE {GameLogic.Methods.FormatThanosMessage(GameLogic.Constants.TIME_LIMIT.ToString(), "SECONDS")}"
            };
    
            
            // Post the threaten message to API Gateway
            await Methods.PostBattlefield(newItem);
            
            return response;
        }
    }
}
