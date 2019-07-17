using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using LambdaSharp;
using LambdaSharp.ApiGateway;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.Gateway {

    public class Function : ALambdaApiGatewayFunction {

        //--- Fields ---
        private string BUCKET_NAME;
        IAmazonS3 S3Client { get; set; }

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config)
        {
            // config.ReadText()
            BUCKET_NAME = AwsConverters.ReadS3BucketName(config, "WebsiteBucket");
            S3Client = new AmazonS3Client();

            return Task.CompletedTask;
        }

        public async Task<Types.AddItemsResponse> AddItems(GameLogic.Types.Item item)
        {
            var battlefieldList = new List<GameLogic.Types.Item>();
            try
            {
                var battlefield = await GetDataFromS3();
                battlefieldList = battlefield.ToList();
            }
            catch
            {
            }

            battlefieldList.Add(item);

            await PutDataToS3(battlefieldList);

            return new Types.AddItemsResponse
            {
                Response = "Success"
            };
        }


        public async Task<Types.GetItemsResponse> GetItems(string contains = null, int offset = 0, int limit = 10)
        {

            var battlefieldList = new List<GameLogic.Types.Item>();

            try
            {
                var battlefield = await GetDataFromS3();
                battlefieldList = battlefield.ToList();

            }
            catch
            {
                
            }


          return new Types.GetItemsResponse
          {
              Items = battlefieldList
          };
        }

        public async Task<Types.GetGraveResponse> GetGrave(string contains = null, int offset = 0, int limit = 10)
        {
            
            var graveyardList = new List<GameLogic.Types.Avenger>();

            try
            {
                var graveyard = await GetDataFromS3(true);
                graveyardList = graveyard.ToList();

            }
            catch
            {
                
            }


            return new Types.GetGraveResponse()
            {
                Avengers = graveyardList
            };
        }
        
        public async Task<Types.PostGraveResponse> AddGrave(List<GameLogic.Types.Avenger> avengers)
        {
            var graveyardList = new List<GameLogic.Types.Avenger>();
            try
            {
                var graveyard = await GetDataFromS3(true);
                graveyardList = graveyard.ToList();
            }
            catch
            {
            }

            foreach (var member in avengers)
            {
                graveyardList.Add(member);
            }
            

            await PutDataToS3(graveyardList, true);

            return new Types.PostGraveResponse()
            {
                Avengers = graveyardList
            };
        }

        public async Task<Types.ClearResponse> Clear() {
            await PutDataToS3(new List<GameLogic.Types.Item>());
            await PutDataToS3(new List<GameLogic.Types.Avenger>(), true);
            return new Types.ClearResponse();
        }

        private async Task PutDataToS3(List<GameLogic.Types.Item> items) {
            var responseBody = "";
            try
            {
                var request = new PutObjectRequest {
                    BucketName = BUCKET_NAME,
                    Key = "battlefield.json",
                    ContentBody = JsonConvert.SerializeObject(items)
                };
                await S3Client.PutObjectAsync(request);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error putting object battlefield.json into bucket {BUCKET_NAME}. Make sure it exist and your bucket is in the same region as this function.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }
        
        private async Task PutDataToS3(List<GameLogic.Types.Avenger> avengers, bool graveyard ) {
            var responseBody = "";
            try
            {
                var request = new PutObjectRequest {
                    BucketName = BUCKET_NAME,
                    Key = "graveyard.json",
                    ContentBody = JsonConvert.SerializeObject(avengers)
                };
                await S3Client.PutObjectAsync(request);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error putting object graveyard.json into bucket {BUCKET_NAME}. Make sure it exist and your bucket is in the same region as this function.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }


        private async Task<IEnumerable<GameLogic.Types.Item>> GetDataFromS3() {
            var responseBody = "";
            try
            {
                var request = new GetObjectRequest {
                    BucketName = BUCKET_NAME,
                    Key = "battlefield.json"
                };
                using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseBody = reader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error getting object battlefield.json from bucket {BUCKET_NAME}. Make sure they exist and your bucket is in the same region as this function.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
            return JsonConvert.DeserializeObject<List<GameLogic.Types.Item>>(responseBody);
        }
        
        private async Task<IEnumerable<GameLogic.Types.Avenger>> GetDataFromS3(bool graveyard) {
            var responseBody = "";
            try
            {
                var request = new GetObjectRequest {
                    BucketName = BUCKET_NAME,
                    Key = "graveyard.json"
                };
                using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseBody = reader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error getting object graveyard.json from bucket {BUCKET_NAME}. Make sure they exist and your bucket is in the same region as this function.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
            return JsonConvert.DeserializeObject<List<GameLogic.Types.Avenger>>(responseBody);
        }
    }
}