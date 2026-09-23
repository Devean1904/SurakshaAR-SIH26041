namespace SurakshaAR.Backend.Services;

public class SmsService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly bool _isConfigured;

    public SmsService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
        _isConfigured = !string.IsNullOrEmpty(config["Twilio:AccountSid"]);
    }

    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otp)
    {
        if (!_isConfigured)
        {
            Console.WriteLine($"[SMS] Twilio not configured. OTP for {phoneNumber} not sent.");
            return false;
        }

        try
        {
            var accountSid = _config["Twilio:AccountSid"]!;
            var authToken = _config["Twilio:AuthToken"]!;
            var fromNumber = _config["Twilio:FromNumber"]!;

            var message = $"Your SurakshaAR verification code is: {otp}. Valid for 5 minutes.";

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("From", fromNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMS] Error sending OTP to {phoneNumber}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendCustomSmsAsync(string phoneNumber, string message)
    {
        if (!_isConfigured)
        {
            Console.WriteLine($"[SMS] Twilio not configured. Message to {phoneNumber} not sent.");
            return true;
        }

        try
        {
            var accountSid = _config["Twilio:AccountSid"]!;
            var authToken = _config["Twilio:AuthToken"]!;
            var fromNumber = _config["Twilio:FromNumber"]!;

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("From", fromNumber),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMS] Error sending to {phoneNumber}: {ex.Message}");
            return false;
        }
    }
}
