using Microsoft.Maui.Controls;
using System;
using System.IO;
using Uygulamam.Helper;
using Uygulamam.Models;

namespace Uygulamam.Pages
{
    public partial class Profilim : ContentPage
    {
        public Profilim() => InitializeComponent();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (AppSession.AktifKullanici == null || string.IsNullOrEmpty(AppSession.AktifKullaniciToken))
            {
                DisplayAlert("Hata", "Kullanıcı oturumu bulunamadı.", "Tamam");
                return;
            }

            // Eğer varsa profil fotoğrafını göster
            if (!string.IsNullOrEmpty(AppSession.ResimUrl))
            {
                notResim.Source = AppSession.ResimUrl;
            }
        }

        private async void PickImage_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Bir resim seç"
                });

                if (result == null) return;

                var aktifKullanici = AppSession.AktifKullanici;
                var token = AppSession.AktifKullaniciToken;

                if (aktifKullanici == null || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(aktifKullanici.Id))
                {
                    await DisplayAlert("Hata", "Kullanıcı girişi yok veya UID/token boş.", "Tamam");
                    return;
                }

                // Stream’i aç ve iki kopya oluştur
                using var originalStream = await result.OpenReadAsync();
                using var previewStream = new MemoryStream();
                using var uploadStream = new MemoryStream();

                await originalStream.CopyToAsync(previewStream);
                await originalStream.CopyToAsync(uploadStream);

                previewStream.Position = 0;
                uploadStream.Position = 0;

                notResim.Source = ImageSource.FromStream(() => previewStream);

                var ext = Path.GetExtension(result.FileName)?.ToLowerInvariant();
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "image/png"
                };

                var fileName = $"{aktifKullanici.Id}/{Guid.NewGuid()}{ext}";

                string url = await FirebaseStorageHelper.UploadImagePut(fileName, uploadStream, contentType, token);

                if (!string.IsNullOrEmpty(url))
                {
                    AppSession.ResimUrl = url;
                    aktifKullanici.ResimUrl = url;
                    await DisplayAlert("Başarılı", "Fotoğraf başarıyla yüklendi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Genel Hata: {ex.Message}", "Tamam");
            }
        }
    }
}
