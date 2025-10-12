using Microsoft.Maui.Controls;
using System;
using Uygulamam.Models;

namespace Uygulamam.Pages
{
    public partial class Not_Bırak : ContentPage
    {
        public Not_Bırak()
        {
            InitializeComponent();
        }

        private async void Gönder_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(notYaz.Text))
            {
                await DisplayAlert("Hata", "Lütfen bir not yaz.", "Tamam");
                return;
            }

            if (AppSession.AktifKullanici == null)
            {
                await DisplayAlert("Hata", "Kullanıcı bilgisi bulunamadı. Lütfen giriş yapın.", "Tamam");
                return;
            }

            var note = new NotModel
            {
                KullanıcıAdı = AppSession.AktifKullanici.Username,
                Tarih = DateTime.Now.ToString("dd.MM.yyyy"),
                Mesaj = notYaz.Text,
                ResimUrl = AppSession.AktifKullanici.ResimUrl ?? "https://your-default-image-url.png"
            };

            // Hafızaya ekle
            AppSession.Notlar.Add(note);

            // TODO: Firebase veya DB kaydı yapılacak
            // await firestore.Collection("notes").AddAsync(note);

            await DisplayAlert("Başarılı", "Not paylaşıldı!", "Tamam");

            // Üyeler sayfasına yönlendir
            
        }
    }
}
