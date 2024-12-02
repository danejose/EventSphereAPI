using EventSphereAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[action]")]
public class PaymentController : ControllerBase
{
    private readonly string baseUrl = "https://api-preprod.phonepe.com/apis/pg-sandbox/pg/v1/pay";
    private readonly string merchantId = "PHONEPEPGTEST43";
    private readonly string saltKey = "3cb6a6d7-5ae4-4a01-9bdc-61f27ccefe46";
    private readonly int saltIndex = 1;

    [HttpPost]
    [ActionName("callback")]
    public IActionResult PaymentCallback([FromBody] object callbackData)
    {
        Console.WriteLine("Callback received: " + JsonConvert.SerializeObject(callbackData));
        return Ok();
    }

    [HttpPost]
    public ActionResult PhonePe()
    {
        try
        {
            var data = new Dictionary<string, object>
        {
            { "merchantId", "PHONEPEPGTEST43" },
            { "merchantTransactionId", Guid.NewGuid().ToString() },
            { "merchantUserId", "MUID123" },
            { "amount", 10000 },
            { "redirectUrl", "http://localhost:7282/api/callback" },
            { "redirectMode", "POST" },
            { "callbackUrl", "http://localhost:7282/api/callback" },
            { "mobileNumber", "9562611195" },
            { "paymentInstrument", new Dictionary<string, string> { { "type", "PAY_PAGE" } } }
        };

            var encode = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));

            var stringToHash = encode + "/pg/v1/pay" + saltKey;
            var sha256 = BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).Replace("-", "").ToLower();
            var finalXHeader = sha256 + "###" + saltIndex;

            using (var client = new HttpClient())
            {
                var requestData = new Dictionary<string, string> { { "request", encode } };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl)
                {
                    Content = content
                };

                requestMessage.Headers.Add("X-VERIFY", finalXHeader);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.SendAsync(requestMessage).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Response from PhonePe: " + responseContent);

                try
                {
                    var rData = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (rData == null)
                    {
                        Console.WriteLine("Failed to deserialize response");
                        return BadRequest("Invalid response from PhonePe");
                    }

                    if (rData.code != null && rData.code != "PAYMENT_INITIATED")
                    {
                        Console.WriteLine($"Error Code: {rData.code}");
                        Console.WriteLine($"Error Message: {rData.message}");
                        return BadRequest($"PhonePe Error: {rData.message}");
                    }


                    var redirectUrl = rData?.data?.instrumentResponse?.redirectInfo?.url?.ToString();

                    if (string.IsNullOrEmpty(redirectUrl))
                    {
                        Console.WriteLine("Redirect URL is null or empty");
                        Console.WriteLine("Full Response Details: " + JsonConvert.SerializeObject(rData));
                        return BadRequest("Redirect URL not found in the response.");
                    }

                    return Ok(new { redirectUrl });
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Error parsing JSON response: " + ex.Message);
                    return StatusCode(500, "Error parsing response from PhonePe");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return StatusCode(500, "An error occurred while processing the payment request.");
        }
    }



}
