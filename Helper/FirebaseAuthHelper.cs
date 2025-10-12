using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Uygulamam.Helper
{
    internal class FirebaseAuthHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string apiKey = "AIzaSyCdb40UmqIm0Rwa-f4XayG3KPeRcJbHg3U";

        private static string _idToken;
        private static string _localId;

        public static string CurrentIdToken => _idToken;
        public static string CurrentLocalId => _localId;

        public class FirebaseErrorResponse { public FirebaseError error { get; set; } }
        public class FirebaseError { public int code { get; set; } public string message { get; set; } }

        // Strongly typed register response
        public class FirebaseRegisterResponse
        {
            public string idToken { get; set; }
            public string email { get; set; }
            public string refreshToken { get; set; }
            public string expiresIn { get; set; }
            public string localId { get; set; }
        }

        public static async Task<(bool Success, string IdToken, string LocalId, string ErrorMessage)> RegisterUser(string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
            var payload = new { email, password, returnSecureToken = true };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var respJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<FirebaseErrorResponse>(respJson);
                return (false, null, null, errorObj?.error?.message ?? "Bilinmeyen hata.");
            }

            var result = JsonConvert.DeserializeObject<FirebaseRegisterResponse>(respJson);

            _idToken = result.idToken;
            _localId = result.localId;

            // Debug için
            Console.WriteLine($"RegisterUser UID: {_localId}");
            return (true, _idToken, _localId, null);
        }

        // LoginUser ve diğer metodlar aynı kalabilir
        public static async Task<(bool Success, bool EmailVerified, string IdToken, string LocalId, string ErrorMessage)> LoginUser(string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
            var payload = new { email, password, returnSecureToken = true };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var respJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<FirebaseErrorResponse>(respJson);
                return (false, false, null, null, errorObj?.error?.message ?? "Bilinmeyen hata.");
            }

            dynamic result = JsonConvert.DeserializeObject(respJson);
            string idToken = result.idToken;
            string localId = result.localId;
            bool emailVerified = await IsEmailVerified(idToken);

            _idToken = idToken;
            _localId = localId;

            return (true, emailVerified, idToken, localId, null);
        }

        public static string GetIdToken() => _idToken;

        public static async Task<(bool Success, string ErrorMessage)> SendEmailVerification(string idToken)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";
            var payload = new { requestType = "VERIFY_EMAIL", idToken };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var respJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<FirebaseErrorResponse>(respJson);
                return (false, errorObj?.error?.message ?? "E-posta gönderilemedi.");
            }

            return (true, null);
        }

        public static async Task<bool> IsEmailVerified(string idToken)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={apiKey}";
            var payload = new { idToken };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return false;
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return result.users[0].emailVerified == true;
        }

        public static void Logout() { _idToken = null; _localId = null; }
    }
}
