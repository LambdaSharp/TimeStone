using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using My.TimeStone.GameLogic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.AvengersGetBattlefieldStatus {

    public class FunctionRequest : Types.Payload {

    }

    public class FunctionResponse : Types.Payload {

    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request)
        {

            var response = new FunctionResponse();
            var battlefield = await GameLogic.Methods.GetBattlefieldFromGateway();
            var recentItem = GameLogic.Methods.GetLastItemFromBattlefield(battlefield.Items.ToList(), "THREAT");
            var lastItem = Methods.ParseBattleFieldId(recentItem);
            
            var roundNumber = 1;
            try
            {
                roundNumber = Int32.Parse(lastItem.Round.Substring(1, lastItem.Round.Length - 1));
            }
            catch
            {
            }

            response.RoundNumber = roundNumber.ToString();            

            // if thanos is threatening, move to recruit and submit
            if (lastItem.Team.Equals("THANOS") && lastItem.Type.Equals("THREAT"))
            {
                response.Status = "defend";
            }
            return response;
        }
    }
}
