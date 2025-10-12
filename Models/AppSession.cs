using System.Collections.ObjectModel;
using Uygulamam.Models;

namespace Uygulamam
{
    public static class AppSession
    {
        public static User AktifKullanici { get; set; }
        public static string AktifKullaniciToken { get; set; }
        public static string ResimUrl { get; set; }

        public static ObservableCollection<NotModel> Notlar { get; set; } = new ObservableCollection<NotModel>();
    }
}
