using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.GameLogic {

    public class FunctionRequest {

        //--- Properties ---

        // TO-DO: add request fields
    }

    public class FunctionResponse {

        //--- Properties ---

        // TO-DO: add response fields
    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {
                

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request) {

            // TO-DO: add business logic

            return new FunctionResponse();
        }
    }
}
