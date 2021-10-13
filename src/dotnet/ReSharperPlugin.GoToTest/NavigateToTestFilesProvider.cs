using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.Collections;
using JetBrains.DataFlow;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.RiderTutorials.Utils;
using JetBrains.Util;
using ReSharperPlugin.GoToTest.Options;

namespace ReSharperPlugin.GoToTest
{
    [ContextNavigationProvider]
    public class NavigateToTestFilesProvider : INavigateFromHereProvider
    {
        private const string DefaultSuffixes = "Test, Tests";
        private IEnumerable<string> _suffixes;

        public NavigateToTestFilesProvider(Lifetime lifetime, ISettingsStore settingsStore)
        {
            if (settingsStore == null) return;

            var concatenatedSuffixesOption = settingsStore
                .BindToContextLive(lifetime, ContextRange.ApplicationWide)
                .GetValueProperty(lifetime, (GoToTestSettings key) => key.ConcatenatedSuffixes);

            concatenatedSuffixesOption.Change.Advise_HasNew(lifetime, v => { _suffixes = DeriveSuffixes(v.New); });
        }

        public IEnumerable<ContextNavigation> CreateWorkflow(IDataContext dataContext)
        {
            var currentClass = dataContext.GetSelectedTreeNode<ITreeNode>()?.GetParentOfType<IClassDeclaration>();
            var solution = dataContext.GetData(ProjectModelDataConstants.SOLUTION);

            if (solution == null || currentClass == null) yield break;

            foreach (var (testClass, projectOfTestClass) in FindTestTypesWithinSolution(solution, currentClass))
            {
                var testClassName = testClass.DeclaredName;
                var projectNameOfTestClass = projectOfTestClass.Name;

                yield return new ContextNavigation($"Test: {testClassName} ({projectNameOfTestClass})", null,
                    NavigationActionGroup.UnitTests, () => { testClass.NavigateToTreeNode(true); });
            }
        }

        internal IOrderedEnumerable<KeyValuePair<ICSharpTypeDeclaration, IProject>> FindTestTypesWithinSolution(
            ISolution solution,
            IClassDeclaration classToCheck)
        {
            var testTypes = new Dictionary<ICSharpTypeDeclaration, IProject>();

            foreach (var project in solution.GetAllProjects())
            foreach (var fileOfProject in project.GetAllProjectFiles())
                testTypes.AddRange(FindTestTypesWithinProjectFile(fileOfProject, classToCheck));

            return OrderByProjectAndClassName(testTypes);
        }

        internal IEnumerable<string> DeriveSuffixes(string concatenatedSuffixes)
        {
            var results = new Collection<string>();

            var stringToSplit = string.IsNullOrWhiteSpace(concatenatedSuffixes)
                ? DefaultSuffixes
                : concatenatedSuffixes;
            var suffixes = stringToSplit.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var suffix in suffixes) results.Add(SanitizeSuffix(suffix));

            return results;
        }

        private Dictionary<ICSharpTypeDeclaration, IProject> FindTestTypesWithinProjectFile(
            IProjectFile fileOfProject, IClassDeclaration classToCheck)
        {
            var testTypes = new Dictionary<ICSharpTypeDeclaration, IProject>();

            if (!(fileOfProject.GetPrimaryPsiFile() is ICSharpFile cSharpFile)) return testTypes;

            foreach (var typeWithinFile in GetTypesOfFile(cSharpFile))
                if (IsMatchingTestType(classToCheck, typeWithinFile))
                    testTypes.Add(typeWithinFile, typeWithinFile.GetProject());

            return testTypes;
        }

        private IEnumerable<ICSharpTypeDeclaration> GetTypesOfFile(ICSharpFile cSharpFile)
            => cSharpFile.NamespaceDeclarationsEnumerable.SelectMany(namespaceDeclaration =>
                namespaceDeclaration.TypeDeclarations);

        private bool IsMatchingTestType(IClassDeclaration classToCheck, ICSharpTypeDeclaration testCandidate)
        {
            if (string.Equals(testCandidate.DeclaredName, classToCheck.DeclaredName)) return false;

            return testCandidate.DeclaredName.StartsWith(classToCheck.DeclaredName) &&
                   _suffixes.Any(suffix =>
                       testCandidate.DeclaredName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        private IOrderedEnumerable<KeyValuePair<ICSharpTypeDeclaration, IProject>> OrderByProjectAndClassName(
            Dictionary<ICSharpTypeDeclaration, IProject> testTypes)
            => testTypes.OrderBy(x => x.Value.Name).ThenBy(x => x.Key.DeclaredName);

        private string SanitizeSuffix(string suffix) => suffix.Trim(' ', '.', '*');
    }
}