using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Collections;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.NavigationExtensions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.RiderTutorials.Utils;
using JetBrains.Util;

namespace ReSharperPlugin.TestLinker
{
    [ContextNavigationProvider]
    public class NavigateToTestFilesProvider : INavigateFromHereProvider
    {
        private readonly string[] _testClassSuffixes = { "Test", "Tests" };

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
                   _testClassSuffixes.Any(suffix =>
                       testCandidate.DeclaredName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        private IOrderedEnumerable<KeyValuePair<ICSharpTypeDeclaration, IProject>> OrderByProjectAndClassName(
            Dictionary<ICSharpTypeDeclaration, IProject> testTypes)
            => testTypes.OrderBy(x => x.Value.Name).ThenBy(x => x.Key.DeclaredName);
    }
}