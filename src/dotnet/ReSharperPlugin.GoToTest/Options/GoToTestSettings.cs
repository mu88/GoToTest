using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace ReSharperPlugin.GoToTest.Options
{
    [SettingsKey(typeof(EnvironmentSettings), "Settings for Go to Test")]
    public class GoToTestSettings
    {
        [SettingsEntry("Test, Tests", "Suffixes of test classes")]
        public string ConcatenatedSuffixes { get; set; }
    }
}