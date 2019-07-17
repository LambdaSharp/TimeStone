using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal.Util;
using LambdaSharp;
using My.TimeStone.GameLogic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosCheckDamageStatus {

    public class FunctionRequest : Types.Payload {

    }

    public class FunctionResponse : Types.Payload {

    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {
        
        //--- Fields ---
        public int BattleResult;

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

            // increment round number
            var currentRoundNumber = Int32.Parse(response.RoundNumber);
            response.RoundNumber = (currentRoundNumber + 1).ToString();
            
            // set end game conditions
            if (Int32.Parse(response.ThanosHealth) <= 0 && Int32.Parse(response.EarthPopulation) <= 0)
            {
                response.Dead = "both";
            }
            if (Int32.Parse(response.EarthPopulation) <= 0)
            {
                response.Dead = "earth";
            }            
            if (Int32.Parse(response.ThanosHealth) <= 0)
            {
                response.Dead = "thanos";
            }

            return response;
        }
    }
}
