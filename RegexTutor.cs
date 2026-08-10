using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.OpenAI;
using Azure;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace RegexTutor
{
    public static class RegexTutor
    {
        #region key

        private static readonly string API_KEY = Environment.GetEnvironmentVariable("API_KEY");
        private static readonly string AZURE_OPENAI_ENDPOINT = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        
        #endregion

        [FunctionName("ExplainRegex")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function,
                                                    "get", "post", Route = null)]
                                                    HttpRequest req,
                                                    ILogger log)
        {
            string regex = req.Query["regex"];

            string explanation = await ExplainRegex(regex);

            return new OkObjectResult(JsonConvert.SerializeObject(explanation));
        }

        public static async Task<string> ExplainRegex(string regEx)
        {
            OpenAIClient client =
                new OpenAIClient(new Uri(AZURE_OPENAI_ENDPOINT),
                new AzureKeyCredential(API_KEY));


            Response<Completions> completionsResponse = await client.GetCompletionsAsync(
                deploymentOrModelName: "DavinciPrototype",
                new CompletionsOptions()
                {
                    Prompts = { "Explain what the below regular expression does : " + regEx },
                    Temperature = (float)0.7,
                    MaxTokens = 250,
                    NucleusSamplingFactor = (float)1,
                    FrequencyPenalty = (float)0,
                    PresencePenalty = (float)0,
                    GenerationSampleCount = 1,
                });

            Completions completions = completionsResponse.Value;

            return completions.Choices.FirstOrDefault().Text;
        }
    }
}
