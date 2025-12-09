using Microsoft.Extensions.Logging;

namespace GoogleMapTest;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseMauiMaps();

#if DEBUG
		builder.Logging.AddDebug();
#endif
		// Register Location Database as a Singleton
		builder.Services.AddSingleton<LocationDatabase>(s => new LocationDatabase(LocationDatabase.GetDatabasePath()));
		return builder.Build();
	}
}
