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
using Azure.AI.Vision.Core.Input;
using Azure.AI.Vision.Core.Options;
using Azure.AI.Vision.ImageAnalysis;

namespace AzureOpenAIPlayground
{
    public static class AltTextProcessor
    {
        #region key

        private static readonly string VISION_API_KEY = Environment.GetEnvironmentVariable("VISION_API_KEY");
        private static readonly string VISION_ENDPOINT = Environment.GetEnvironmentVariable("VISION_ENDPOINT");

        #endregion

        [FunctionName("AltTextProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["imageurl"];

            string altText = await GenerateAltText(name);

            return new OkObjectResult(altText);
        }

        public static async Task<string> GenerateAltText(string imagUrl)
        {
            // generate alt text for image
            var serviceOptions = new VisionServiceOptions(
           Environment.GetEnvironmentVariable("VISION_ENDPOINT"),
           new AzureKeyCredential(Environment.GetEnvironmentVariable("VISION_API_KEY")));

            var imageSource = VisionSource.FromUrl(new Uri(imagUrl));

            var analysisOptions = new ImageAnalysisOptions()
            {
                Features = ImageAnalysisFeature.Caption | ImageAnalysisFeature.Text,
                Language = "en"
            };

            using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);
            {
                var result = await analyzer.AnalyzeAsync();

                if (result != null)
                {
                    if (result.Reason == ImageAnalysisResultReason.Analyzed)
                    {
                        if (result.Caption != null)
                        {
                            return result.Caption.Content;
                        }
                        else
                        {
                            return "No caption.";
                        }
                    }
                    else if (result.Reason == ImageAnalysisResultReason.Error)
                    {
                        var errorDetails = ImageAnalysisErrorDetails.FromResult(result);

                        return "No caption.";
                    }
                }
            }

            return "No caption.";
        }
    }
}
