using Avalonia;

namespace Capstone.Desktop;

internal static class Program
{
    // dotnet run --project src/Capstone.Desktop
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Also used by the XAML previewer in Rider.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
