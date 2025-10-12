using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Uygulamam.Models;
using System.Net.Http;

namespace Uygulamam.Helper
{
    internal class FireBaseHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string baseUrl = "https://uygulamam-a5f2c-default-rtdb.firebaseio.com/";

        // Kullanıcı profilini UID ile kaydeder
        public static async Task<bool> AddUserProfile(string uid, string username, string email)
        {
            var userProfile = new User
            {
                Username = username,
                Email = email
            };

            var json = JsonConvert.SerializeObject(userProfile);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(baseUrl + $"kullanicilar/{uid}.json", content);
            return response.IsSuccessStatusCode;
        }

        // UID ile kullanıcı profili getirir
        public static async Task<User> GetUserProfile(string uid)
        {
            var response = await client.GetAsync(baseUrl + $"kullanicilar/{uid}.json");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonConvert.DeserializeObject<User>(json);
        }

        public static async Task<bool> AddNote(string aktifkullanıcı, string mesaj, string resimUrl)
        {
            var noteProfil = new NotModel
            {
                KullanıcıAdı = aktifkullanıcı,
                Mesaj = mesaj,
                ResimUrl = resimUrl,
                Tarih = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
            };
            var json = JsonConvert.SerializeObject(noteProfil);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(baseUrl + $"NotAtanlar/{aktifkullanıcı}.json", content);
            return response.IsSuccessStatusCode;
        }

        // Tüm kullanıcı profillerini getirir
        public static async Task<List<User>> GetAllUserProfiles()
        {
            var response = await client.GetAsync(baseUrl + "kullanicilar.json");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<User>();

            var dict = JsonConvert.DeserializeObject<Dictionary<string, User>>(json);
            return dict?.Values.ToList() ?? new List<User>();
        }

        // Email ile kullanıcı profili getirir
        public static async Task<User> GetUserByEmail(string email)
        {
            var users = await GetAllUserProfiles();
            return users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        // Kullanıcı adı ile kullanıcı profili getirir
        public static async Task<User> GetUserByUsername(string username)
        {
            var users = await GetAllUserProfiles();
            return users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }
    }
}