using Microsoft.Maui.Controls;
using System;
using Uygulamam.Helper;
using Uygulamam.Models;

namespace Uygulamam.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void KayýtOlButton_Clicked(object sender, EventArgs e)
        {
            string username = KullanýcýAdýKayýt.Text?.Trim();
            string email = EmailKayýt.Text?.Trim();
            string password = ÞifreKayýt.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Hata", "Lütfen tüm alanlarý doldurun.", "Tamam");
                return;
            }

            var existingUsername = await FireBaseHelper.GetUserByUsername(username);
            if (existingUsername != null)
            {
                await DisplayAlert("Hata", "Bu kullanýcý adý zaten kullanýmda.", "Tamam");
                return;
            }

            var existingEmail = await FireBaseHelper.GetUserByEmail(email);
            if (existingEmail != null)
            {
                await DisplayAlert("Hata", "Bu e-posta adresi zaten kayýtlý.", "Tamam");
                return;
            }

            // Register ve LocalId al
            var registerResult = await FirebaseAuthHelper.RegisterUser(email, password);
            if (!registerResult.Success)
            {
                await DisplayAlert("Hata", $"Kayýt baþarýsýz: {registerResult.ErrorMessage}", "Tamam");
                return;
            }

            // Realtime DB’ye kaydet
            bool dbResult = await FireBaseHelper.AddUserProfile(registerResult.LocalId, username, email);
            if (!dbResult)
            {
                await DisplayAlert("Uyarý", "Profil verisi veritabanýna kaydedilemedi.", "Tamam");
            }

            // Email doðrulama
            var (mailSent, errorMessage) = await FirebaseAuthHelper.SendEmailVerification(registerResult.IdToken);
            if (mailSent)
                await DisplayAlert("Baþarýlý", "Doðrulama e-postasý gönderildi. Lütfen e-postanýzý kontrol edin.", "Tamam");
            else
                await DisplayAlert("Uyarý", $"E-posta gönderilemedi: {errorMessage}", "Tamam");

            // Debug alert (isteðe baðlý)
            await DisplayAlert("Debug", $"Kayýt tamamlandý. UID: {registerResult.LocalId}", "Tamam");

            // Login sayfasýna yönlendir
            await Navigation.PushAsync(new MainPage());
        }
    }
}
