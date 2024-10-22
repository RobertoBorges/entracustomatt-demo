using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace nonprodborges
{
    public class CustomAuthenticationExtensionsAPI
    {
        private readonly ILogger<CustomAuthenticationExtensionsAPI> _logger;

        public CustomAuthenticationExtensionsAPI(ILogger<CustomAuthenticationExtensionsAPI> logger)
        {
            _logger = logger;
        }

        [Function("CustomAuthenticationExtensionsAPI")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            _logger.LogInformation($"Received from Entra {JsonConvert.SerializeObject(data)}");

            // Read the correlation ID from the Microsoft Entra request    
            string correlationId = data?.data.authenticationContext.correlationId;

            // Claims to return to Microsoft Entra
            ResponseContent r = new ResponseContent();
            r.data.actions[0].claims.CorrelationId = correlationId;
            r.data.actions[0].claims.ApiVersion = "1.0.0";
            r.data.actions[0].claims.DateOfBirth = "01/01/2000";
            r.data.actions[0].claims.CustomRoles.Add("Writer");
            r.data.actions[0].claims.CustomRoles.Add("Editor");
            r.data.actions[0].claims.CustomFreeText = "This is a custom free text";
            _logger.LogInformation($"Returning claims to Microsoft Entra {JsonConvert.SerializeObject(r)}");
            return new OkObjectResult(r);
        }
    }

    public class ResponseContent
    {
        [JsonProperty("data")]
        public Data data { get; set; }
        public ResponseContent()
        {
            data = new Data();
        }
    }
    public class Data
    {
        [JsonProperty("@odata.type")]
        public string odatatype { get; set; }
        public List<Action> actions { get; set; }
        public Data()
        {
            odatatype = "microsoft.graph.onTokenIssuanceStartResponseData";
            actions = new List<Action>();
            actions.Add(new Action());
        }
    }
    public class Action
    {
        [JsonProperty("@odata.type")]
        public string odatatype { get; set; }
        public Claims claims { get; set; }
        public Action()
        {
            odatatype = "microsoft.graph.tokenIssuanceStart.provideClaimsForToken";
            claims = new Claims();
        }
    }
    public class Claims
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CorrelationId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DateOfBirth { get; set; }
        public string ApiVersion { get; set; }
        public List<string> CustomRoles { get; set; }
        public string CustomFreeText { get; set; }
        public Claims()
        {
            CustomRoles = new List<string>();
        }
    }
}
