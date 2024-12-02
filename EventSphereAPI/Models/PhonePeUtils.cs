using System.Security.Cryptography;
using System.Text;

public class PhonePeUtils
{
    public static string GenerateXVerify(string requestBody, string endpoint, string saltKey, int saltIndex)
    {
        string data = requestBody + endpoint + saltKey;

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return $"{hash}###{saltIndex}";
        }
    }
}
