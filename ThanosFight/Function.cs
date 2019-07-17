using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using My.TimeStone.GameLogic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace My.TimeStone.ThanosFight {

    public class FunctionRequest : Types.Payload {

    }

    public class FunctionResponse : Types.Payload {
    }

    public class Function : ALambdaFunction<FunctionRequest, FunctionResponse> {

        //--- Fields ---
        public int AvengerTeamTotalAttackPower;

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task<FunctionResponse> ProcessMessageAsync(FunctionRequest request) {

            var response = new FunctionResponse
            {
                RoundNumber = request.RoundNumber,
                EarthPopulation = request.EarthPopulation,
                ThanosHealth = request.ThanosHealth
            };

            // get data from Gateway - look for Avengers on battlefield
            var battlefield = await GameLogic.Methods.GetBattlefieldFromGateway();
            var recentItem = GameLogic.Methods.GetLastItemFromBattlefield(battlefield.Items.ToList(), "DEFEND", request.RoundNumber);
            var lastItem = GameLogic.Methods.ParseBattleFieldId(recentItem);
            var graveyard = await GameLogic.Methods.GetGrave();
            var thanosExpectedUnitCount = GameLogic.Methods.GetThanosRoundUnitCount(response.RoundNumber);
            var thanosTotalPower = Methods.GetThanosRoundPowerLevel(request.RoundNumber);
            var deserialzedListOfAvengers = new List<Types.Avenger>();

            try
            {
                deserialzedListOfAvengers = JsonConvert.DeserializeObject<List<Types.Avenger>>(recentItem.Value);

                // if wrong number of units submitted, automatically kill
                if (deserialzedListOfAvengers.Count > thanosExpectedUnitCount)
                {

                    response.EarthPopulation = (Int32.Parse(response.EarthPopulation) - thanosTotalPower).ToString();
                    var invalidUnitCountMessage = new Types.Item
                    {
                        Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(),
                            "THANOS", "INVALID SUBMISSION"),
                        Value = $"PLEASE SUBMIT <= ({thanosExpectedUnitCount} UNITS) TO ENTER THE BATTLE"
                    };

                    await Methods.PostBattlefield(invalidUnitCountMessage);

                    var invalidUnitCountKillMessage = new Types.Item
                    {
                        Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(),
                            "THANOS", "KILL"),
                        Value =
                            $" YOU LOST THIS ROUND.. I HAVE KILLED {GameLogic.Methods.FormatThanosMessage(Math.Abs(thanosTotalPower).ToString(), "PEOPLE")}"
                    };
                    await Methods.PostBattlefield(invalidUnitCountKillMessage);

                }




                // compute power of team and battle
                else
                {
                    // add attack power
                    var avengerTeamTotalPower = 0;
                    var cleanListOfAvengers = new List<Types.Avenger>();
                    foreach (var avenger in deserialzedListOfAvengers)
                    {
                        if (graveyard.Avengers.Any(x => x.id.Equals(avenger.id)))
                        {

                            var graveyardItem = new Types.Item
                            {
                                Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber,
                                    DateTime.UtcNow.ToString(), "THANOS", "GRAVE"),
                                Value = $"{avenger.name} IS ALREADY DEAD. IGNORING FROM TOTAL TEAM POWER"
                            };

                            // Post the threaten message to API Gateway
                            await Methods.PostBattlefield(graveyardItem);
                            continue;
                        }
                        cleanListOfAvengers.Add(avenger);
                        avengerTeamTotalPower += Int32.Parse(avenger.power);
                    }


                    var battleResult = avengerTeamTotalPower - thanosTotalPower;
                    var battleMessage = "";

                    var battleCasualties = CalculateDeaths(cleanListOfAvengers, battleResult, Int32.Parse(response.RoundNumber));
                    if (battleResult < 0)
                    {
                        response.EarthPopulation = (Int32.Parse(response.EarthPopulation) + battleResult).ToString();
                        battleMessage += $" YOU LOST THIS ROUND.. I HAVE KILLED {GameLogic.Methods.FormatThanosMessage(Math.Abs(battleResult).ToString(), "PEOPLE")}";

                    } else if (battleResult > 0)
                    {
                        response.ThanosHealth = (Int32.Parse(response.ThanosHealth) - battleResult).ToString();
                        battleMessage += $" YOU WON THIS ROUND.. I HAVE TAKEN {GameLogic.Methods.FormatThanosMessage(battleResult.ToString(), "HEALTH")} DAMAGE";
                    }
                    if(battleCasualties.Count > 0) {
                        battleMessage += $"THESE HEROS HAVE FALLEN {JsonConvert.SerializeObject(battleCasualties)} ";
                    }

                    var newItem = new Types.Item
                    {
                        Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(),
                            "THANOS", "BATTLE"),
                        Value = battleMessage
                    };
                    await Methods.PostBattlefield(newItem);
                    await Methods.PostGrave(battleCasualties);

                }
            }

            // if team submitted in invalid format then kill
            catch
            {
                response.EarthPopulation = (Int32.Parse(response.EarthPopulation) - thanosTotalPower).ToString();
                var invalidUnitCountMessage = new Types.Item
                {
                    Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(), "THANOS", "INVALID SUBMISSION"),
                    Value = $"SUBMIT YOUR TEAM IN THE CORRECT FORMAT!"
                };

                await Methods.PostBattlefield(invalidUnitCountMessage);

                var invalidUnitCountKillMessage = new Types.Item
                {
                    Id = GameLogic.Methods.FormatBattlefieldId(request.RoundNumber, DateTime.UtcNow.ToString(), "THANOS", "KILL"),
                    Value = $" YOU LOST THIS ROUND.. I HAVE KILLED {GameLogic.Methods.FormatThanosMessage(Math.Abs(thanosTotalPower).ToString(), "PEOPLE")}"
                };

                await Methods.PostBattlefield(invalidUnitCountKillMessage);
            }

            return response;
        }

        public List<Types.Avenger> CalculateDeaths(List<Types.Avenger> avengers, int powerDifference, int roundNumber) {
            var unitCount = GameLogic.Methods.GetThanosRoundUnitCount(roundNumber.ToString());
            var grave = new List<Types.Avenger>();
            foreach(var a in avengers) {

                // calcualate defence of hero
                var averagePower = GameLogic.Constants.AVERAGE_POWER;
                var heroPower = Int32.Parse(a.power);
                double heroPowerWeighted = (heroPower + (powerDifference / unitCount));
                if(heroPowerWeighted < averagePower) {
                    grave.Add(a);
                    continue;
                }
                double defence = (1 - (averagePower / heroPowerWeighted)) * 100;

                // calculate thanos attack power
                var attackMax = 100;
                if(roundNumber > GameLogic.Constants.DIFFICULTY_TRIGGER_ROUND) {
                    attackMax = roundNumber * roundNumber;
                }
                var random = new Random();
                var attack = random.Next(attackMax);

                if(attack > defence) {
                    grave.Add(a);
                }
            };
            return grave;
        }
    }
}
