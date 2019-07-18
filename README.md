# TimeStone
## About this challenge
The world is under attack! You and your team must find a way to defeat Thanos and save the world from grave danger.
There are two main actors (step functions) in this battle simulator: Thanos and Avengers which act on a shared battlefield via an API Gateway.
You will be taking control of the Avengers step function during this challenge and creating a mechanism to query the Marvel API and dispatch a team of Avengers to the battlefield during the event of a Thanos Threat. If you do not respond to Thanos's threat in a timely manner he will kill earthlings.
# Possible Events #
All events on the battlefield will be in the following format: `ROUND#::DATETIME::ACTOR::MESSAGE`
## THANOS ##
### Start of Round ###
 - At the start of each round Thanos will post a game update. This update will include the `round number`, `current earch population`, and `thanos current health`:
```txt
R1::7/17/19 10:41:05 PM::THANOS::UPDATE | GET READY FOR THE NEXT WAVE OF MINIONS (1 ROUND), (1000000 PEOPLE REMAINING), (1000000 HEALTH) REMAINING
```
### Thanos Threat ###
 - Each round consist of a Thanos threat. These threats include a few actionable item you must take into consideration `units` and `seconds`.
```txt
R1::7/17/19 10:41:06 PM::THANOS::THREAT | ATTACKING - CHOOSE YOUR BEST (1 UNITS), YOU HAVE (60 SECONDS)
```
### Thanos Kill ###
 - If you fail to dispatch a defense team in the request timeframe or the team you submit is not in the expected format thanos will kill some of Earth's population.
```txt
R1::7/17/19 10:45:22 PM::THANOS::KILL | YOU HAVE FAILED TO DEFEND IN TIME. I HAVE KILLED 180 PEOPLE (999900 PEOPLE REMAINING)
```
## AVENGERS ##
## Avengers Defend ##
 - Following a Thanos threat it is your job to dispatch a team of avengers to the battlefield. This is done via your Avengers Step Function (AvengersRecruitAndSubmit Lambda Function). We have gone ahead and added some boilerplate code to help you get started. Make sure your team submission is in the format below and has <= the number of units supplied in the previous Thanos Threat.
```txt
R1::7/17/19 10:41:18 PM::AVENGERS::DEFEND | [{\"id\":1011334,\"name\":\"3-D Man\",\"power\":\"12\"}]
```

# Game Logic
## Hero Power
* The power of a hero is determined by the number of comics they appear in. 
* [marvel-csharp](https://github.com/rroa/marvel-csharp) API is used in the sample for recruiting an avenger and to calculate the power level. 
## Unit Count Limit
* The Unit Count Limit determines how many avengers you can submit to the battle. Thanos will reject any team submissions that have more than the unit count limit. 
* The round’s unit count limit is determined by the formula:
```
unit count limit = round number * round number
```
* The unit count will max out at Round 10. All following rounds will have the same unit count limit of 100
## Fights
* With each Fight encounter, heroes will have a chance of dying. The probability is determined by the following formula:
```
Defense = (1 - average hero power / (hero power + (power difference / unit count limit))
```
* If a hero dies, they will be added to the graveyard. Heroes that have died CANNOT be resubmitted back into a battlefield, and will be ignored for the battle. 
* After Round 10, the heroes will have a significantly higher chance of dying with each battle encounter. 
## Damage
* The damage done to Thanos or the Earth is determined by the following formula:
```
Damage = sum of avengers power - thanos power
```
* Thanos' power is determined by the following formula:
```
Thanos Power = unit count limit * average hero power
```



# Level 0: Setup
## Prerequisites - .NET, AWS, λ#, Deploy
* [Install .Net 2.1](https://dotnet.microsoft.com/download/dotnet-core/2.1)
* [Sign up for AWS account](https://aws.amazon.com/)
* [Install AWS CLI](https://aws.amazon.com/cli/)
* [Install the λ# Tool](https://github.com/LambdaSharp/LambdaSharpTool#install-%CE%BB-cli)
    * [Documentation](https://lambdasharp.net/articles/ReleaseNotes-Favorinus.html)
    * (Already installed) Upgrade tool
        ```
        dotnet tool update -g LambdaSharp.Tool
        ```
* [Get a public/private key from Marvel API](https://developer.marvel.com/account)

## Deploy Infrastructrure
1. `git clone git@github.com:LambdaSharp/TimeStone.git`
2. Update public and private key variables inside [AvengersRecruitAndSubmit/Function.cs](./AvengersRecruitAndSubmit/Function.cs)
3. `lash init --tier Timestone`
4. `lash deploy --tier Timestone`
    * Grab the API Gateway URL from the command line and add that to [GameLogic/Constants.cs](./GameLogic/Constants.cs)
    * Grab the website URL from the command line. That will be used later to display the battlefield events.
5. Redeploy `lash deploy --tier Timestone`

## Create Step Functions
1. Create an IAM Role
    1. AWS > Services > IAM > Roles
    2. Create role 
        * Select `Step Function`
        * Click `Next`
    3. Click `Next: Tags`
    4. Click `Next: Review`
    5. Review:
        * Role name: StepFunctionRole
    6. Click `Create role`
2. Modify the [Avengers](./avengersStep.json) and [Thanos](./thanosStep.json) step functions with Lambda arns
    1. AWS > Services > Lambda
    2. Find the functions for the respective step function state and fill in the ARN.
3. Create Avengers and Thanos Step Functions
    1. AWS > Services > Step Functions
    2. State machines:
        * Click `Create state machine`
    3. Details:
        * Name: Thanos
        * Code: Copy and paste the code from thanosStep.json
        * Click `Next`
    4. IAM
        * Choose `Choose an existing IAM role`
        * Select `StepFunctonRole`
        * Click `Create state machine`
    5. Do the same for Avengers

## Running the Game
### Start Thanos Execution
1. Navigate to your Thanos Step Function in AWS and click `Start Execution`. You do not need to modify the initial execution payload. 
2. Navigate to the website URL and verify that Thanos successfully posted his Welcome message.

### Start Avengers Execution
3. Navigate to your Avengers Step Function in AWS and click `Start Execution`. You do not need to modify the initial execution payload.
4. Navigate to the website URL and verify that Avengers successfully posted the Avengers team to defect Thanos' attack. 

# Level 1: We must keep defending!
Build out the Avengers step function so that it responds to continuous threats from Thanos. 

There is sample code to query the Marvel API and submit a team to the battle field in [AvengersRecruitAndSubmit](./AvengersRecruitAndSubmit/Function.cs)

# Level 2: We can't just get random heros!!
Build out the Avengers recruiting logic so that you can submit the most optimal team to fight Thanos.

# Level 3.. Boss: Beat Thanos. 