using System;
using System.Linq.Expressions;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.Options.ThemedIcons;
using JetBrains.Application.UI.Options.OptionsDialog;
using JetBrains.DataFlow;
using JetBrains.IDE.UI.Extensions;
using JetBrains.IDE.UI.Options;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Daemon.OptionPages;

namespace ReSharperPlugin.TestLinker.Options
{
    [OptionsPage(Id, PageTitle, typeof(OptionsThemedIcons.EnvironmentGeneral), ParentId = CodeInspectionPage.PID)]
    public class TestLinkerOptionsPage : BeSimpleOptionsPage
    {
        private const string Id = nameof(TestLinkerOptionsPage);
        private const string PageTitle = "TestLinker Options";

        private readonly Lifetime _lifetime;

        public TestLinkerOptionsPage(
            Lifetime lifetime,
            OptionsPageContext optionsPageContext,
            OptionsSettingsSmartContext optionsSettingsSmartContext)
            : base(lifetime, optionsPageContext, optionsSettingsSmartContext)
        {
            _lifetime = lifetime;

            IProperty<string> concatenatedSuffixesOption =
                new Property<string>(lifetime, $"{nameof(TestLinkerOptionsPage)}::ConcatenatedSuffixes");
            concatenatedSuffixesOption.SetValue(
                optionsSettingsSmartContext.StoreOptionsTransactionContext.GetValue(
                    (TestLinkerSettings key) => key.ConcatenatedSuffixes));

            concatenatedSuffixesOption.Change.Advise(lifetime, a =>
            {
                if (!a.HasNew) return;
                optionsSettingsSmartContext.StoreOptionsTransactionContext.SetValue(
                    (TestLinkerSettings key) => key.ConcatenatedSuffixes, a.New);
            });

            AddTextBox((TestLinkerSettings x) => x.ConcatenatedSuffixes, "Suffixes of test classes");
            AddCommentText("Enter suffixes of test classes to look for and separate them by comma");
        }

        private void AddTextBox<TKeyClass>(Expression<Func<TKeyClass, string>> lambdaExpression, string description)
        {
            var property = new Property<string>(description);
            OptionsSettingsSmartContext.SetBinding(_lifetime, lambdaExpression, property);
            var control = property.GetBeTextBox(_lifetime);
            AddControl(control.WithDescription(description, _lifetime));
        }
    }
}