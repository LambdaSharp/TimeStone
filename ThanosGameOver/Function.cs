using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using My.TimeStone.GameLogic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosGameOver {

    public class FunctionRequest : Types.Payload {
    }

    public class FunctionResponse : Types.Payload {
    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request)
        {

            var message = "";
            switch (request.Dead)
            {
                case "thanos":
                    message = "EARTH IS SAVED";
                    break;
                case "earth":
                    message = "EARTH IS DESTROYED";
                    break;
                case "both":
                    message = "THERE IS NOTHING LEFT";
                    break;
                    
            }
            var newItem = new Types.Item
            {
                Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(), "MOD", "UPDATE"),
                Value = message
            };

            // Post the threaten message to API Gateway
            await Methods.PostBattlefield(newItem);

            return new FunctionResponse();
        }
    }
}
