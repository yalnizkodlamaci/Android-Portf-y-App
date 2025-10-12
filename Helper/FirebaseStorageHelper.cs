using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

internal class FirebaseStorageHelper
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string bucket = "uygulamam-a5f2c.firebasestorage.app";

    public static async Task<string> UploadImagePut(string fileName, Stream fileStream, string contentType, string idToken)
    {
        try
        {
            if (fileStream.CanSeek) fileStream.Position = 0;

            var encodedFilePath = Uri.EscapeDataString($"NotResimleri/{fileName}");
            var url = $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o?uploadType=media&name={encodedFilePath}";

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // Firebase için POST kullanıyoruz (PUT değil!)
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            if (!string.IsNullOrEmpty(idToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            using var response = await client.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Upload failed! StatusCode: {response.StatusCode}\nResponse: {responseText}";

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    errorMessage += "\nHata: Token geçersiz veya oturum yok.";

                throw new Exception(errorMessage);
            }

            // Response zaten JSON dönüyor
            dynamic uploadResponse = JObject.Parse(responseText);

            // Eğer direkt download URL istiyorsan:
            string downloadUrl = uploadResponse.mediaLink;

            return downloadUrl;
        }
        catch (Exception ex)
        {
            // Exception detayını fırlatıyoruz
            throw new Exception($"UploadImagePut Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }
}
