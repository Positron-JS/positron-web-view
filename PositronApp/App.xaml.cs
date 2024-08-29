using Positron.Pages;

namespace SocialMailApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		var mp = new PositronMainPage() {
			Url = "https://m.800casting.com/ProfileEditor/Agency"
		};
		mp.WebView.UserAgent = "Hybrid-Mobile-App/1.0 Android/1.1";


        MainPage = mp;
	}
}
