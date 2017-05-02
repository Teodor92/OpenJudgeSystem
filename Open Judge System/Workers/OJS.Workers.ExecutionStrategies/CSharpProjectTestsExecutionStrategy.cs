﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.Build.Evaluation;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CSharpProjectTestsExecutionStrategy : ExecutionStrategy
    {
        protected const string SetupFixtureTemplate = @"
        using System;
        using System.IO;
        using NUnit.Framework;


        [SetUpFixture]
        public class SetUpClass
        {
            [OneTimeSetUp]
            public void RedirectConsoleOutputBeforeEveryTest()
            {
                TextWriter writer = new StringWriter();
                Console.SetOut(writer);
            }
        }
";
        protected const string SetupFixtureFileName = "_$SetupFixture";
        protected const string ZippedSubmissionName = "Submission.zip";
        protected const string CompeteTest = "Test";
        protected const string TrialTest = "Test.000";
        protected const string CsProjFileSearchPattern = "*.csproj";
        protected const string NUnitReference =
            "nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL";

        protected const string AdditionalExecutionArguments = "--noresult --inprocess";

        // Extracts error/failure messages and the class which threw it
        private static readonly string ErrorMessageRegex = $@"\d+\) (.*){Environment.NewLine}((?:.+{Environment.NewLine})*?)\s*at (?:[^(){Environment.NewLine}]+?)\(\) in \w:\\(?:[^\\{Environment.NewLine}]+\\)*(.+).cs";

        public CSharpProjectTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc)
        {
            if (!File.Exists(nUnitConsoleRunnerPath))
            {
                throw new ArgumentException(
                    $"NUnitConsole not found in: {nUnitConsoleRunnerPath}",
                    nameof(nUnitConsoleRunnerPath));
            }

            this.NUnitConsoleRunnerPath = nUnitConsoleRunnerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
            this.GetCompilerPathFunc = getCompilerPathFunc;
            this.TestNames = new List<string>();
        }

        ~CSharpProjectTestsExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        protected string NUnitConsoleRunnerPath { get; }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        protected string WorkingDirectory { get; }

        protected string SetupFixturePath { get; set; }

        protected List<string> TestNames { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);

            var csProjFilePath = FileHelpers.FindFirstFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern);

            this.ExtractTestNames(executionContext.Tests);

            // Modify Project file
            var project = new Project(csProjFilePath);
            var compileDirectory = project.DirectoryPath;
            this.SetupFixturePath = $"{compileDirectory}\\{SetupFixtureFileName}.cs";

            this.CorrectProjectReferences(executionContext.Tests, project);
            project.Save(csProjFilePath);
            project.ProjectCollection.UnloadAllProjects();

            // Write Test files and SetupFixture           
            var index = 0;
            var testPaths = new List<string>();
            foreach (var test in executionContext.Tests)
            {
                var testName = this.TestNames[index++];
                var testedCodePath = $"{compileDirectory}\\{testName}.cs";
                testPaths.Add(testedCodePath);
                File.WriteAllText(testedCodePath, test.Input);
            }
      
            testPaths.Add(this.SetupFixturePath);

            // Compiling
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments, 
                csProjFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;

            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(testPaths.ToArray());

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, compilerResult.OutputFile);
            return result;
        }

        protected virtual ExecutionResult RunUnitTests(
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string compiledFile)
        {
            var arguments = new List<string> { compiledFile };
            arguments.AddRange(AdditionalExecutionArguments.Split(' '));

            var processExecutionResult = executor.Execute(
                this.NUnitConsoleRunnerPath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var message = "yes";
                var testFile = this.TestNames[testIndex++];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected virtual Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            var errorsByFiles = new Dictionary<string, string>();
            var errorRegex = new Regex(ErrorMessageRegex);
            var errors = errorRegex.Matches(receivedOutput);

            foreach (Match error in errors)
            {
                var errorMethod = error.Groups[1].Value;
                var cause = error.Groups[2].Value.Replace(Environment.NewLine, string.Empty);
                var fileName = error.Groups[3].Value;
                errorsByFiles.Add(fileName, $"{errorMethod} {cause}");
            }

            return errorsByFiles;
        }

        private void CorrectProjectReferences(IEnumerable<TestContext> tests, Project project)
        {
            File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);
            project.AddItem("Compile", $"{SetupFixtureFileName}.cs");

            foreach (var testName in this.TestNames)
            {
                project.AddItem("Compile", $"{testName}.cs");
            }

            project.SetProperty("OutputType", "Library");
            var nUnitPrevReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains("nunit.framework"));
            if (nUnitPrevReference != null)
            {
                project.RemoveItem(nUnitPrevReference);
            }

            // Add our NUnit Reference, if private is false, the .dll will not be copied and the tests will not run
            var nUnitMetaData = new Dictionary<string, string>();
            nUnitMetaData.Add("SpecificVersion", "False");
            nUnitMetaData.Add("Private", "True");
            project.AddItem("Reference", NUnitReference, nUnitMetaData);

            // Check for VSTT just in case, we don't want Assert conflicts
            var vsTestFrameworkReference = project.Items
                .FirstOrDefault(x => x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));
            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }
        }

        private void ExtractTestNames(IEnumerable<TestContext> tests)
        {
            var trialTests = 1;
            var competeTests = 1;

            foreach (var test in tests)
            {
                if (test.IsTrialTest)
                {
                    var testNumber = trialTests < 10 ? $"00{trialTests}" : $"0{trialTests}";
                    this.TestNames.Add($"{TrialTest}.{testNumber}");
                    trialTests++;
                }
                else
                {
                    var testNumber = competeTests < 10 ? $"00{competeTests}" : $"0{competeTests}";
                    this.TestNames.Add($"{CompeteTest}.{testNumber}");
                    competeTests++;
                }
            }
        }
    }
}
