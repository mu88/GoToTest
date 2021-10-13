using JetBrains.Application.Settings;
using JetBrains.Application.Settings.WellKnownRootKeys;

namespace ReSharperPlugin.TestLinker.Options
{
    [SettingsKey(typeof(EnvironmentSettings), "Settings for TestLinker")]
    public class TestLinkerSettings
    {
        [SettingsEntry("Test, Tests", "Suffixes of test classes")]
        public string ConcatenatedSuffixes { get; set; }
    }
}