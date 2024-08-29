using Positron.Pages;

namespace PositronApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		var mp = new PositronMainPage() {
			Url = "https://socialmail.me"
		};
		mp.WebView.UserAgent = "Hybrid-Mobile-App/1.0 Android/1.1";


        MainPage = mp;
	}
}
