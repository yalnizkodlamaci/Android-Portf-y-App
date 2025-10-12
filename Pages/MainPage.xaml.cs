using Microsoft.Maui.Controls;
using System;
using Uygulamam.Helper;
using Uygulamam.Models;
using Uygulamam.Pages;

namespace Uygulamam
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();


        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Hata", "Kullanıcı adı ve şifre boş olamaz.", "Tamam");
                return;
            }

            // 1. DB'den kullanıcı profili çek
            var userProfile = await FireBaseHelper.GetUserByUsername(username);
            if (userProfile == null)
            {
                await DisplayAlert("Hata", "Kullanıcı bulunamadı.", "Tamam");
                return;
            }

            // 2. Firebase Auth ile giriş yap
            var loginResult = await FirebaseAuthHelper.LoginUser(userProfile.Email, password);
            if (!loginResult.Success)
            {
                await DisplayAlert("Hata", $"Giriş başarısız: {loginResult.ErrorMessage}", "Tamam");
                return;
            }

            // 3. E-posta doğrulaması kontrol
            if (!loginResult.EmailVerified)
            {
                await DisplayAlert("Uyarı", "E-posta doğrulanmamış. Lütfen e-postanızı kontrol edin.", "Tamam");
                return;
            }

            // 4. AppSession güncelle (UID ve Token garantili)
            AppSession.AktifKullanici = new User
            {
                Id = userProfile.Id ?? loginResult.LocalId,  // UID boşsa LocalId kullan
                Username = userProfile.Username,
                Email = userProfile.Email,
                ResimUrl = userProfile.ResimUrl
            };

            AppSession.AktifKullaniciToken = loginResult.IdToken;

            // 5. Debug için UID kontrol
            await DisplayAlert("Debug",
                $"Aktif Kullanıcı: {AppSession.AktifKullanici.Username}\n" +
                $"UID: {AppSession.AktifKullanici.Id}\n" +
                $"Token: {AppSession.AktifKullaniciToken}", "Tamam");

            // 6. Profil sayfasına geç
            Application.Current.MainPage = new AppShell();
        }

        private async void KayıtOlTıkla(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
