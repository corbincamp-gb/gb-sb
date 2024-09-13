using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Model.Db.QuestionPro;

namespace SkillBridge.CMS.Controllers
{
    public class WebhookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WebhookController> _logger;
        private readonly IEmailSender _emailSender;

        public WebhookController(ILogger<WebhookController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /*public IActionResult Index()
        {
            return View();
        }*/

        [HttpGet]
        public async Task<ActionResult> GenerateQuestionProApplication(string externalReference)
        {
            var chainedSurveys = new List<QPResponse>();

            while (!String.IsNullOrWhiteSpace(externalReference))
            {
                var chainedSurvey = await _db.QPResponses.Where(o => o.ResponseId.ToString() == externalReference).FirstOrDefaultAsync();

                if (chainedSurvey != null)
                {
                    chainedSurveys.Add(chainedSurvey);
                    externalReference = chainedSurvey.ExternalReference;
                }
                else
                {
                    externalReference = String.Empty;
                }
            }

            if (chainedSurveys != null)
            {
                var chainedResponseIds = chainedSurveys.Select(o => o.ResponseId).ToList();

                ViewBag.ChainedSurveys = chainedSurveys;
                ViewBag.Questions = await _db.QPResponseQuestions.Where(o => chainedResponseIds.Contains(o.ResponseId)).ToListAsync();
                ViewBag.Answers = await _db.QPResponseQuestionAnswers.Where(o => chainedResponseIds.Contains(o.ResponseId)).ToListAsync();
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public void UpdateZohoTicket(string ticketId)
        {
            Console.WriteLine("ticketId received from powerautomate system... ID is: " + ticketId);

            // Need to pull ticket ID from the email Body
            string id = GetStringBetweenCharacters(ticketId, "[", "]");

            ZohoAPICalls(id);
        }

        public async Task<string> ZohoAPICalls(string id)
        {
            /*$ curl -X GET https://desk.zoho.com/api/v1/tickets/1892000000143237
              -H "orgId:2389290"
              -H "Authorization:Zoho-oauthtoken 1000.67013ab3960787bcf3affae67e649fc0.83a789c859e040bf11e7d05f9c8b5ef6"*/
            Token tok = new Token();
            string generatedAccessCode = "1000.13f4367a2a431b638cea5ff52a1e460c.5c0029964a0e6e66a21d25f18af6048c";
            string clientId = "1000.JVOZFSFFFKLNHWSHB65XYYFL1FF8RF";
            string clientSecret = "bedd1eb7922b7e2f6b27721c47aafbd1344326f52a";

            var apiState = _db.APIState.FirstOrDefault(e => e.Id == 1);
            /*Console.WriteLine("apiState.AccessToken: " + apiState.AccessToken);
            Console.WriteLine("apiState.RefreshToken: " + apiState.RefreshToken);
            Console.WriteLine("apiState.ExpiresIn: " + apiState.ExpiresIn);
            Console.WriteLine("apiState.TokenType: " + apiState.TokenType);
            Console.WriteLine("apiState.ExpirationDate: " + apiState.ExpirationDate);*/

            tok.AccessToken = apiState.AccessToken;
            tok.RefreshToken = apiState.RefreshToken;
            tok.TokenType = apiState.TokenType;
            tok.ExpiresIn = apiState.ExpiresIn;

            // Check if ExpirationDate has passed
            if(DateTime.Compare(DateTime.Now, apiState.ExpirationDate) > 0)
            {
                // Refresh the token
                if (apiState.RefreshToken != null)
                {
                    // Refresh token
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://accounts.zoho.com/oauth/v2/token?refresh_token=" + apiState.RefreshToken + "&client_id=" + clientId + "&client_secret=" + clientSecret + "&scope=Desk.tickets.ALL&redirect_uri=https://localhost:44368/&grant_type=refresh_token"))
                        {
                            var response = await httpClient.SendAsync(request);
                            Console.WriteLine("response: " + response);

                            var jsonContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("jsonContent of response: " + jsonContent);

                            tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                            Console.WriteLine("access_token is " + tok.AccessToken);
                        }
                    }

                    
                }
            }

            

            // Handshake token
            /*using (var httpClient = new HttpClient())
            {
                using(var request = new HttpRequestMessage(new HttpMethod("POST"), "https://accounts.zoho.com/oauth/v2/token?code=" + generatedAccessCode + "&grant_type=authorization_code&client_id=" + clientId + "&client_secret=" + clientSecret + "&redirect_uri=https://localhost:44368/"))
                {
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);

                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);

                    tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                    Console.WriteLine("access_token is " + tok.AccessToken);
                }
            }*/

            // Pulling ticket information
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://desk.zoho.com/api/v1/tickets/" + id))
                {
                    Console.WriteLine("URI: " + "https://desk.zoho.com/api/v1/tickets/" + id);
                    Console.WriteLine("tok.AccessToken: " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("orgId", "791268895");
                    request.Headers.TryAddWithoutValidation("Authorization", "Zoho-oauthtoken " + tok.AccessToken);
                    var response = await httpClient.SendAsync(request);

                    Console.WriteLine("response: " + response);

                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);
                }
            }

            // Remove old tag
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://desk.zoho.com/api/v1/tickets/" + id + "/dissociateTag"))
                {
                    Console.WriteLine("URI: " + "https://desk.zoho.com/api/v1/tickets/" + id + "/dissociateTag");
                    Console.WriteLine("tok.AccessToken: " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("orgId", "791268895");
                    request.Headers.TryAddWithoutValidation("Authorization", "Zoho-oauthtoken " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    request.Headers.TryAddWithoutValidation("Content-Encoding", "UTF8");
                    request.Content = new StringContent("{\"tags\":[\"intake questionnaire not received\"]}", Encoding.UTF8, "application/json");

                    Console.WriteLine("request.Content: " + request.Content);
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);
                }
            }

            // Add new tag
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://desk.zoho.com/api/v1/tickets/" + id + "/associateTag"))
                {
                    Console.WriteLine("URI: " + "https://desk.zoho.com/api/v1/tickets/" + id + "/associateTag");
                    Console.WriteLine("tok.AccessToken: " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("orgId", "791268895");
                    request.Headers.TryAddWithoutValidation("Authorization", "Zoho-oauthtoken " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    request.Headers.TryAddWithoutValidation("Content-Encoding", "UTF8");
                    request.Content = new StringContent("{\"tags\":[\"intake questionnaire received\"]}", Encoding.UTF8, "application/json");

                    Console.WriteLine("request.Content: " + request.Content);
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);
                }
            }

            // Attachment API
            // Exmaple: https://desk.zoho.com/api/v1/tickets/783499000000602562/attachments/1892000000047041/content?orgId=791268895
            // https://desk.zoho.com/DeskAPIDocument#TicketAttachments

            // Refresh token
            /*using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://accounts.zoho.com/oauth/v2/token?refresh_token=" + tok.RefreshToken + "&client_id=" + clientId + "&client_secret=" + clientSecret + "&scope=Desk.tickets.ALL&redirect_uri=https://localhost:44368/&grant_type=refresh_token"))
                {
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);

                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);

                    tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                    Console.WriteLine("access_token is " + tok.AccessToken);
                }
            }*/

            apiState.AccessToken = tok.AccessToken;
            apiState.RefreshToken = tok.RefreshToken;
            apiState.ExpiresIn = tok.ExpiresIn;
            apiState.TokenType = tok.TokenType;
            apiState.ExpirationDate = DateTime.Now.AddSeconds(tok.ExpiresIn - 1000);

            _db.APIState.Update(apiState);
            _db.SaveChanges();

            return null;
        }

        /*public async Task<string> QPAPICalls(string id, string responseId)
        {
            // QP API Key
            string apiKey = "b928d5af-44ed-4c23-81a0-cdd72ac3da6b";

            Console.WriteLine("id: " + id);
            Console.WriteLine("responseId: " + responseId);
            
            // Combined/Starting Intake Survey Example
            //string intakeSurveyId = "10723592";
            //string testResponseId = "157947206";

            // Last Question Survey Example (Final Question of the entire chained survey)
            string intakeSurveyId = "10763276";
            string testResponseId = "157948235";


            // Pull response data
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.questionpro.com/a/api/v2/surveys/" + intakeSurveyId + "/responses/" + testResponseId + "?apiKey=" + apiKey))
                {
                    //Console.WriteLine("URI: " + "https://desk.zoho.com/api/v1/tickets/" + id + "/dissociateTag");
                    //Console.WriteLine("tok.AccessToken: " + tok.AccessToken);
                    //request.Headers.TryAddWithoutValidation("orgId", "791268895");
                    //request.Headers.TryAddWithoutValidation("Authorization", "Zoho-oauthtoken " + tok.AccessToken);
                    //request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    //request.Headers.TryAddWithoutValidation("Content-Encoding", "UTF8");
                    //request.Content = new StringContent("{\"tags\":[\"intake questionnaire not received\"]}", Encoding.UTF8, "application/json");

                    Console.WriteLine("request.Content: " + request.Content);
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);
                }
            }

            return null;
        }*/

        /*public async Task<string> ZohoAPIAttachTest()
        {
            string id = "783499000001223021";   // Test ticket to attach file to

            Token tok = new Token();
            string generatedAccessCode = "1000.13f4367a2a431b638cea5ff52a1e460c.5c0029964a0e6e66a21d25f18af6048c";
            string clientId = "1000.JVOZFSFFFKLNHWSHB65XYYFL1FF8RF";
            string clientSecret = "bedd1eb7922b7e2f6b27721c47aafbd1344326f52a";

            SB_APIState apiState = _db.APIState.FirstOrDefault(e => e.Id == 1);

            tok.AccessToken = apiState.AccessToken;
            tok.RefreshToken = apiState.RefreshToken;
            tok.TokenType = apiState.TokenType;
            tok.ExpiresIn = apiState.ExpiresIn;

            // Check if ExpirationDate has passed
            if (DateTime.Compare(DateTime.Now, apiState.ExpirationDate) > 0)
            {
                // Refresh the token
                if (apiState.RefreshToken != null)
                {
                    // Refresh token
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://accounts.zoho.com/oauth/v2/token?refresh_token=" + apiState.RefreshToken + "&client_id=" + clientId + "&client_secret=" + clientSecret + "&scope=Desk.tickets.ALL&redirect_uri=https://localhost:44368/&grant_type=refresh_token"))
                        {
                            var response = await httpClient.SendAsync(request);
                            Console.WriteLine("response: " + response);

                            var jsonContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("jsonContent of response: " + jsonContent);

                            tok = JsonConvert.DeserializeObject<Token>(jsonContent);
                            Console.WriteLine("access_token is " + tok.AccessToken);
                        }
                    }
                }
            }

            // Create PDF


            // Create test text file
            byte[] bytes = Encoding.UTF8.GetBytes("this is a test text file");


            // Attach test file
            using (var httpClient = new HttpClient())
            {
                Stream stream = new MemoryStream(bytes);

                //new StreamContent("", "file", "Test.txt" }
                //};

                var byteArrayContent = new ByteArrayContent(bytes);
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/txt");

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://desk.zoho.com/api/v1/tickets/" + id + "/attachments"))
                {
                    //Console.WriteLine("URI: " + "https://desk.zoho.com/api/v1/tickets/" + id + "/associateTag");
                    //Console.WriteLine("tok.AccessToken: " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("orgId", "791268895");
                    //request.Headers.TryAddWithoutValidation("file", "bytes");
                    request.Headers.TryAddWithoutValidation("Authorization", "Zoho-oauthtoken " + tok.AccessToken);
                    request.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data");
                    request.Headers.TryAddWithoutValidation("Content-Encoding", "UTF8");


                    request.Content = new MultipartFormDataContent
                    {
                        {byteArrayContent, "\"file\"", "\"IntakeSurveyResponses.pdf\""}
                    };


                    Console.WriteLine("request.Content: " + request.Content);
                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine("response: " + response);
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("jsonContent of response: " + jsonContent);
                }
            }

            return null;
        }*/

        public string GetStringBetweenCharacters(string input, string charFrom, string charTo)
        {
            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
    }

    internal class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
