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

        private async void Kay�tOlButton_Clicked(object sender, EventArgs e)
        {
            string username = Kullan�c�Ad�Kay�t.Text?.Trim();
            string email = EmailKay�t.Text?.Trim();
            string password = �ifreKay�t.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Hata", "L�tfen t�m alanlar� doldurun.", "Tamam");
                return;
            }

            var existingUsername = await FireBaseHelper.GetUserByUsername(username);
            if (existingUsername != null)
            {
                await DisplayAlert("Hata", "Bu kullan�c� ad� zaten kullan�mda.", "Tamam");
                return;
            }

            var existingEmail = await FireBaseHelper.GetUserByEmail(email);
            if (existingEmail != null)
            {
                await DisplayAlert("Hata", "Bu e-posta adresi zaten kay�tl�.", "Tamam");
                return;
            }

            // Register ve LocalId al
            var registerResult = await FirebaseAuthHelper.RegisterUser(email, password);
            if (!registerResult.Success)
            {
                await DisplayAlert("Hata", $"Kay�t ba�ar�s�z: {registerResult.ErrorMessage}", "Tamam");
                return;
            }

            // Realtime DB�ye kaydet
            bool dbResult = await FireBaseHelper.AddUserProfile(registerResult.LocalId, username, email);
            if (!dbResult)
            {
                await DisplayAlert("Uyar�", "Profil verisi veritaban�na kaydedilemedi.", "Tamam");
            }

            // Email do�rulama
            var (mailSent, errorMessage) = await FirebaseAuthHelper.SendEmailVerification(registerResult.IdToken);
            if (mailSent)
                await DisplayAlert("Ba�ar�l�", "Do�rulama e-postas� g�nderildi. L�tfen e-postan�z� kontrol edin.", "Tamam");
            else
                await DisplayAlert("Uyar�", $"E-posta g�nderilemedi: {errorMessage}", "Tamam");

            // Debug alert (iste�e ba�l�)
            await DisplayAlert("Debug", $"Kay�t tamamland�. UID: {registerResult.LocalId}", "Tamam");

            // Login sayfas�na y�nlendir
            await Navigation.PushAsync(new MainPage());
        }
    }
}
