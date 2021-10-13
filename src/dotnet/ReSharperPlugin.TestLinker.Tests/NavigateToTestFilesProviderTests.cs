using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using Moq;
using NUnit.Framework;

namespace ReSharperPlugin.TestLinker.Tests
{
    [TestFixture]
    public class NavigateToTestFilesProviderTests
    {
        [Test]
        [Ignore("Don't know how to mock yet")]
        public void FindTestTypes()
        {
            var csharpTypeMock = new Mock<ICSharpTypeDeclaration>();
            csharpTypeMock.Setup(type => type.DeclaredName).Returns("MyCodeTests");
            var csharpFileMock = new Mock<ICSharpFile>();
            csharpFileMock.Setup(file => file.TypeDeclarations)
                .Returns(new TreeNodeCollection<ICSharpTypeDeclaration>(new[] { csharpTypeMock.Object }));
            var projectFileMock = new Mock<IProjectFile>();
            projectFileMock.Setup(projectFile => projectFile.GetPrimaryPsiFile(null)).Returns(csharpFileMock.Object);
            var projectMock = new Mock<IProject>();
            projectMock.Setup(project => project.GetAllProjectFiles(null))
                .Returns(new Collection<IProjectFile> { projectFileMock.Object });
            var solutionMock = new Mock<ISolution>();
            solutionMock.Setup(solution => solution.GetAllProjects())
                .Returns(new Collection<IProject> { projectMock.Object });
            var classDeclarationMock = new Mock<IClassDeclaration>();
            classDeclarationMock.Setup(type => type.DeclaredName).Returns("MyCode");
            var testee = new NavigateToTestFilesProvider(new Lifetime(), null);

            var results = testee.FindTestTypesWithinSolution(solutionMock.Object, classDeclarationMock.Object);

            results.Should()
                .BeEquivalentTo(
                    new KeyValuePair<ICSharpTypeDeclaration, IProject>(csharpTypeMock.Object, projectMock.Object));
        }

        [TestCase("", new[] { "Test", "Tests" })]
        [TestCase("   ", new[] { "Test", "Tests" })]
        [TestCase("Test,Tests", new[] { "Test", "Tests" })]
        [TestCase("Test", new[] { "Test" })]
        [TestCase(" Test", new[] { "Test" })]
        [TestCase("Test ", new[] { "Test" })]
        [TestCase("*.Test, *.Tests*", new[] { "Test", "Tests" })]
        public void DeriveSuffixes(string concatenatedSuffixes, string[] expectedSuffixes)
        {
            var testee = new NavigateToTestFilesProvider(new Lifetime(), null);

            var results = testee.DeriveSuffixes(concatenatedSuffixes);

            results.Should().BeEquivalentTo(expectedSuffixes);
        }
    }
}