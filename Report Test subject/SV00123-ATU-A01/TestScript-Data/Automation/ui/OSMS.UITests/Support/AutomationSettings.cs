using System.Text.Json;

namespace OSMS.UITests.Support;

public sealed class AutomationSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:5068";
    public string Browser { get; set; } = "chrome";
    public bool Headless { get; set; }
    public bool Fullscreen { get; set; }
    public int DefaultTimeoutSeconds { get; set; } = 15;
    public int DemoPauseSeconds { get; set; }
    public string UiDataCsvPath { get; set; } = "Report Test subject/SV00123-ATU-A01/TestScript-Data/TestData/ui/OSMS-UI-Test-Data.csv";
    public string ScreenshotsDirectory { get; set; } = "Report Test subject/SV00123-ATU-A01/TestResults/Evidence/UI/automation";

    public static AutomationSettings Load()
    {
        var settings = new AutomationSettings();
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            var fileSettings = JsonSerializer.Deserialize<AutomationSettings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileSettings != null)
            {
                settings = fileSettings;
            }
        }

        settings.BaseUrl = ReadStringOverride("OSMS_UI_BASE_URL", settings.BaseUrl);
        settings.Browser = ReadStringOverride("OSMS_UI_BROWSER", settings.Browser);
        settings.UiDataCsvPath = ReadStringOverride("OSMS_UI_DATA_CSV", settings.UiDataCsvPath);
        settings.ScreenshotsDirectory = ReadStringOverride("OSMS_UI_SCREENSHOTS_DIR", settings.ScreenshotsDirectory);

        var headlessOverride = Environment.GetEnvironmentVariable("OSMS_UI_HEADLESS");
        if (bool.TryParse(headlessOverride, out var headless))
        {
            settings.Headless = headless;
        }

        var fullscreenOverride = Environment.GetEnvironmentVariable("OSMS_UI_FULLSCREEN");
        if (bool.TryParse(fullscreenOverride, out var fullscreen))
        {
            settings.Fullscreen = fullscreen;
        }

        var timeoutOverride = Environment.GetEnvironmentVariable("OSMS_UI_TIMEOUT_SECONDS");
        if (int.TryParse(timeoutOverride, out var timeoutSeconds) && timeoutSeconds > 0)
        {
            settings.DefaultTimeoutSeconds = timeoutSeconds;
        }

        var demoPauseOverride = Environment.GetEnvironmentVariable("OSMS_UI_DEMO_PAUSE_SECONDS");
        if (int.TryParse(demoPauseOverride, out var demoPauseSeconds) && demoPauseSeconds >= 0)
        {
            settings.DemoPauseSeconds = demoPauseSeconds;
        }

        settings.BaseUrl = settings.BaseUrl.TrimEnd('/');
        settings.Browser = settings.Browser.Trim().ToLowerInvariant();
        return settings;
    }

    public Uri ToAbsoluteUri(string relativePath)
    {
        var normalized = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        return new Uri(BaseUrl + normalized);
    }

    private static string ReadStringOverride(string variableName, string fallbackValue)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? fallbackValue : value.Trim();
    }
}
