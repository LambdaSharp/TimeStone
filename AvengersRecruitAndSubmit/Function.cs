using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using Marvel.Api;
using Marvel.Api.Filters;
using My.TimeStone.GameLogic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.AvengersRecruitAndSubmit {

    public class FunctionRequest : Types.Payload {
    }

    public class FunctionResponse : Types.Payload {
    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {
        //--- Constants ---
        const string PUBLICKEY = "ADD MARVEL PUBLIC KEY HERE";
        const string PRIVATEKEY = "ADD MARVEL PRIVATE KEY HERE";

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request) {

            // Initialize the marvel api client
            var client = new MarvelRestClient(PUBLICKEY, PRIVATEKEY);

            // Initialize an avengers team
            var team = new Types.AvengerTeam();
            
            // Initialize character request filter
            var characterRequestFilter = new CharacterRequestFilter();

            // Query marvel api for desired avengers
            // TODO: refine this logic to assemble stronger team 
            characterRequestFilter.OrderBy(OrderResult.NameAscending);
            characterRequestFilter.Limit = 1;
            var characterResponse = client.FindCharacters(characterRequestFilter);

            foreach (var character in characterResponse.Data.Results)
            {

                // Initialize a comic listing filter to determine power for each avenger
                var comicRequestFilter =  new ComicRequestFilter();
                comicRequestFilter.AddCharacter(character.Id);

                // Form avenger object
                var currentAvenger = new Types.Avenger();
                currentAvenger.name = character.Name;
                currentAvenger.id = character.Id;
                currentAvenger.power = client.FindCharacterComics(character.Id.ToString(), comicRequestFilter).Data.Total;

                // Add avenger to team
                team.AddAvengerToTeam(currentAvenger);
            }

            var message = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(), "AVENGERS", "DEFEND");
            
            await Methods.PostBattlefield(new Types.Item 
            {
                Id = message,
                Value = JsonConvert.SerializeObject(team.avengerTeam)
            });      
            
            return new FunctionResponse();

        }
    }
}
