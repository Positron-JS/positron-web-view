using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Positron;

namespace SocialMailApp;

public static partial class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		    builder.UseMauiCommunityToolkit()
            .RegisterPushServices();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
