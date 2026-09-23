using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SurakshaAR.Backend.Services;

public class OtpService
{
    private const int MaxAttempts = 5;
    private readonly SmsService _sms;
    private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry, int Attempts)> _otpStore = new();

    public OtpService(SmsService sms)
    {
        _sms = sms;
    }

    public async Task<bool> GenerateOtpAsync(string phoneNumber)
    {
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var otp = (BitConverter.ToUInt32(bytes) % 900000 + 100000).ToString();
        var sent = await _sms.SendOtpSmsAsync(phoneNumber, otp);
        if (!sent)
        {
            _otpStore.TryRemove(phoneNumber, out _);
            return false;
        }
        _otpStore[phoneNumber] = (otp, DateTime.UtcNow.AddMinutes(5), 0);
        return true;
    }

    public bool VerifyOtp(string phoneNumber, string otp)
    {
        if (!_otpStore.TryGetValue(phoneNumber, out var stored))
            return false;

        if (stored.Expiry <= DateTime.UtcNow)
        {
            _otpStore.TryRemove(phoneNumber, out _);
            return false;
        }

        if (stored.Attempts >= MaxAttempts)
        {
            _otpStore.TryRemove(phoneNumber, out _);
            return false;
        }

        if (stored.Otp == otp)
        {
            _otpStore.TryRemove(phoneNumber, out _);
            return true;
        }

        _otpStore[phoneNumber] = (stored.Otp, stored.Expiry, stored.Attempts + 1);
        return false;
    }
}
