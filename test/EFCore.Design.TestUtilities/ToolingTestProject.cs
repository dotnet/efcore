// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public enum ToolingProjectLayout
{
    SingleProject,
    SeparateData,
    SeparateMigrations,
    SeparateQueries
}

public enum ToolingProjectKind
{
    Application,
    ProcessProbe,
    AotTests,
    AotProbe
}

public sealed class ToolingTestProject : IAsyncDisposable
{
    private readonly ToolingTestAssemblyFixture _fixture;
    private readonly List<string> _projects = [];
    private int _commandCount;
    private bool _successful;

    public ToolingTestProject(
        ToolingTestAssemblyFixture fixture,
        string name,
        ToolingProjectLayout layout,
        ToolingProjectKind kind)
    {
        _fixture = fixture;
        Layout = layout;
        Kind = kind;
        // Native SQLite loading is subject to Windows path limits even when managed file APIs are not.
        DirectoryPath = Path.Combine(fixture.ResultsDirectory, name[..Math.Min(name.Length, 24)] + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(Path.Combine(DirectoryPath, "test-name.txt"), name);
        File.Copy(Path.Combine(fixture.RepositoryRoot, "NuGet.config"), Path.Combine(DirectoryPath, "NuGet.config"));
        File.WriteAllText(
            Path.Combine(DirectoryPath, "global.json"),
            JsonSerializer.Serialize(
                new
                {
                    sdk = new { version = fixture.Property("SdkVersion"), rollForward = "disable", allowPrerelease = true,
                        paths = new[] { fixture.DotNetRoot } },
                    test = new { runner = "Microsoft.Testing.Platform" }
                }));
        File.WriteAllText(Path.Combine(DirectoryPath, "Directory.Build.props"), fixture.Source("Directory.Build.props.template"));
        File.WriteAllText(Path.Combine(DirectoryPath, "Directory.Build.targets"), "<Project />");
        File.WriteAllText(
            Path.Combine(DirectoryPath, "Directory.Packages.props"),
            "<Project><PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup></Project>");
        var tools = Path.Combine(DirectoryPath, ".config");
        Directory.CreateDirectory(tools);
        File.WriteAllText(Path.Combine(tools, "dotnet-tools.json"), """{"version":1,"isRoot":true,"tools":{}}""");

        if (kind is ToolingProjectKind.ProcessProbe or ToolingProjectKind.AotProbe)
        {
            StartupProject = ContextProject = MigrationsProject = QueryProject = CreateProject("Probe", executable: true);
            WriteSource(
                StartupProject, "Program.cs",
                fixture.Source(kind == ToolingProjectKind.ProcessProbe ? "ProcessProbe.cs.template" : "AotProbe.cs.template"));
        }
        else
        {
            ContextProject = CreateProject(
                layout == ToolingProjectLayout.SingleProject ? "App" : "DataLayer",
                executable: layout == ToolingProjectLayout.SingleProject);
            MigrationsProject = layout is ToolingProjectLayout.SeparateMigrations or ToolingProjectLayout.SeparateQueries
                ? CreateProject("MigrationsLayer", executable: false)
                : ContextProject;
            QueryProject = layout == ToolingProjectLayout.SeparateQueries
                ? CreateProject("Queries", executable: false)
                : ContextProject;
            StartupProject = layout == ToolingProjectLayout.SingleProject
                ? ContextProject
                : CreateProject("Startup", executable: true);

            foreach (var project in _projects)
            {
                foreach (var product in fixture.ProductProjects)
                {
                    AddProductReference(project, product);
                }
            }

            AddPackage(ContextProject, "SQLitePCLRaw.bundle_e_sqlite3", fixture.Property("SqliteVersion"));
            AddProjectReference(MigrationsProject, ContextProject);
            AddProjectReference(QueryProject, ContextProject);
            AddProjectReference(StartupProject, ContextProject);
            AddProjectReference(StartupProject, MigrationsProject);
            AddProjectReference(StartupProject, QueryProject);
            WriteSource(
                ContextProject, "Model.cs",
                fixture.Source("Model.cs.template").Replace("{{MigrationsAssembly}}", ProjectName(MigrationsProject)));
            WriteSource(QueryProject, "Queries.cs", fixture.Source("Queries.cs.template"));

            foreach (var project in _projects.Where(project => project != ContextProject))
            {
                WriteSource(
                    project, "ContextFactory.cs",
                    fixture.Source("ContextFactory.cs.template").Replace("{{FactoryName}}", ProjectName(project) + "ContextFactory"));
            }

            WriteSource(
                StartupProject, "Program.cs",
                fixture.Source(kind == ToolingProjectKind.AotTests ? "DeploymentTests.cs.template" : "Program.cs.template"));
        }

        if (kind is ToolingProjectKind.AotTests or ToolingProjectKind.AotProbe)
        {
            ConfigureXunitFeed();
            AddPackage(StartupProject, "xunit.v3.aot.mtp-v2", fixture.Property("XUnitVersion"));
            AddPackage(StartupProject, "Microsoft.Testing.Extensions.TrxReport", fixture.Property("TestingPlatformVersion"));
            SetProperty(StartupProject, "UseMicrosoftTestingPlatformRunner", "true");
            SetProperty(StartupProject, "IsTestProject", "true");
            var document = XDocument.Load(StartupProject);
            document.Root!.Add(
                new XElement("PropertyGroup", new XAttribute("Condition", "'$(PublishAot)' == 'true'"),
                    new XElement("DefineConstants", "$(DefineConstants);NATIVE_AOT")));
            document.Save(StartupProject);
        }

        if (kind is ToolingProjectKind.Application or ToolingProjectKind.AotTests)
        {
            CreateDatabase();
        }
    }

    public ToolingProjectLayout Layout { get; }
    public ToolingProjectKind Kind { get; }
    public string DirectoryPath { get; }
    public string StartupProject { get; }
    public string ContextProject { get; }
    public string MigrationsProject { get; }
    public string QueryProject { get; }
    public IReadOnlyList<string> Projects => _projects;
    public string DatabasePath => Path.Combine(DirectoryPath, "test.db");
    public string PublishDirectory => Path.Combine(DirectoryPath, "publish");
    public string LogDirectory => Path.Combine(_fixture.ResultsDirectory, "logs", Path.GetFileName(DirectoryPath));
    public string ChildResultsDirectory => Path.Combine(LogDirectory, "child-results");
    public string FactoryArgumentsPath => Path.Combine(DirectoryPath, "factory-arguments.txt");

    public void EnableTasks(string modelStage = "publish", string queryStage = "publish")
    {
        foreach (var project in new[] { ContextProject, QueryProject, StartupProject }.Distinct())
        {
            var document = XDocument.Load(project);
            var props = new XElement("Import",
                new XAttribute("Project", Path.Combine(_fixture.AssetsDirectory, "buildTransitive", "Microsoft.EntityFrameworkCore.Tasks.props")));
            var targets = new XElement("Import",
                new XAttribute("Project", Path.Combine(_fixture.AssetsDirectory, "buildTransitive", "Microsoft.EntityFrameworkCore.Tasks.targets")));
            document.Root!.Elements("Import").Last().AddBeforeSelf(props);
            document.Root.Add(targets);
            document.Save(project);
            SetProperty(project, "EFScaffoldModelStage", project == ContextProject ? modelStage : "none");
            SetProperty(project, "EFPrecompileQueriesStage", project == QueryProject ? queryStage : "none");
        }
    }

    public void SetProperty(string project, string name, string value)
    {
        var document = XDocument.Load(project);
        var properties = document.Root!.Element("PropertyGroup")!;
        properties.SetElementValue(name, value);
        document.Save(project);
    }

    public void WriteSource(string project, string name, string source)
        => File.WriteAllText(Path.Combine(Path.GetDirectoryName(project)!, name), source);

    public void ConfigureDeployment(bool native)
    {
        SetProperty(StartupProject, "PublishAot", native ? "true" : "false");
        SetProperty(StartupProject, "PublishTrimmed", "true");
        SetProperty(StartupProject, "SelfContained", "true");
        SetProperty(StartupProject, "RuntimeIdentifier", RuntimeInformation.RuntimeIdentifier);
    }

    public async Task RestoreAsync()
    {
        foreach (var project in _projects)
        {
            var result = await RunDotNetAsync(
                ["restore", project, "--no-dependencies", "--disable-build-servers", "--verbosity", "minimal"]).ConfigureAwait(false);
            result.EnsureSuccess();
        }
    }

    public Task<ToolingProcessResult> BuildAsync(string? project = null, IEnumerable<string>? arguments = null)
        => RunBuildCommandAsync("build", project ?? StartupProject, arguments);

    public Task<ToolingProcessResult> PublishAsync(IEnumerable<string>? arguments = null)
        => RunBuildCommandAsync("publish", StartupProject, ["--output", PublishDirectory, .. arguments ?? []]);

    public async Task CleanAsync()
    {
        foreach (var project in _projects.AsEnumerable().Reverse())
        {
            var result = await RunDotNetAsync(
                ["clean", project, "--configuration", _fixture.Configuration, "-p:BuildProjectReferences=false"]).ConfigureAwait(false);
            result.EnsureSuccess();
        }
    }

    public Task<ToolingProcessResult> RunApplicationAsync(params string[] arguments)
        => RunDotNetAsync(
            [Path.Combine(Path.GetDirectoryName(StartupProject)!, "bin", _fixture.Configuration, _fixture.TargetFramework,
                ProjectName(StartupProject) + ".dll"), .. arguments]);

    public Task<int> RunPublishedTestsAsync()
        => RunPublishedTestsAsync(PublishDirectory, ChildResultsDirectory);

    public async Task<int> RunRelocatedPublishedTestsAsync()
    {
        var relocated = Directory.CreateTempSubdirectory("efcore-published-").FullName;
        try
        {
            foreach (var file in Directory.EnumerateFiles(PublishDirectory, "*", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(relocated, Path.GetRelativePath(PublishDirectory, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }

            Directory.Delete(PublishDirectory, recursive: true);
            return await RunPublishedTestsAsync(relocated, Path.Combine(LogDirectory, "relocated-results")).ConfigureAwait(false);
        }
        finally
        {
            Directory.Delete(relocated, recursive: true);
        }
    }

    private async Task<int> RunPublishedTestsAsync(string publishDirectory, string resultsDirectory)
    {
        var executable = Path.Combine(
            publishDirectory, ProjectName(StartupProject) + (OperatingSystem.IsWindows() ? ".exe" : ""));
        var result = await RunExecutableAsync(
            executable, ["--report-trx", "--results-directory", resultsDirectory]).ConfigureAwait(false);
        result.EnsureSuccess();
        var reports = Directory.GetFiles(resultsDirectory, "*.trx", SearchOption.AllDirectories);
        if (reports.Length != 1)
        {
            throw new InvalidOperationException($"Expected one published test report in {resultsDirectory}, got {reports.Length}.");
        }

        var counters = XDocument.Load(reports[0]).Descendants().Single(element => element.Name.LocalName == "Counters");
        var executed = Count("executed");
        if (executed == 0 || Count("failed") != 0)
        {
            throw new InvalidOperationException($"Published tests did not execute successfully. Report: {reports[0]}");
        }

        return executed;

        int Count(string name)
            => int.Parse(
                counters.Attribute(name)?.Value ?? throw new InvalidDataException($"Missing TRX counter '{name}' in {reports[0]}."),
                CultureInfo.InvariantCulture);
    }

    public Task<ToolingProcessResult> EfAsync(
        IReadOnlyList<string> arguments,
        string? project = null,
        string? startupProject = null,
        string? configuration = null)
        => RunDotNetAsync(
            ["ef", "--project", project ?? ContextProject, "--startup-project", startupProject ?? StartupProject,
                "--configuration", configuration ?? _fixture.Configuration, "--no-color", "--verbose", "--prefix-output", .. arguments]);

    public Task<ToolingProcessResult> RunDotNetAsync(
        IReadOnlyList<string> arguments,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        => RunExecutableAsync(_fixture.DotNetPath, arguments, timeout, cancellationToken);

    public async Task<ToolingProcessResult> RunExecutableAsync(
        string executable,
        IReadOnlyList<string> arguments,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo(executable) { WorkingDirectory = DirectoryPath };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["DOTNET_ROOT"] = _fixture.DotNetRoot;
        startInfo.Environment["PATH"] = _fixture.AssetsDirectory + Path.PathSeparator + _fixture.DotNetRoot
            + Path.PathSeparator + startInfo.Environment["PATH"];
        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(DirectoryPath, ".dotnet-home");
        startInfo.Environment["DOTNET_NOLOGO"] = "1";
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en-US";
        startInfo.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["NUGET_PACKAGES"] = _fixture.Property("NuGetPackageRoot");
        startInfo.Environment["EF_TEST_DATABASE"] = DatabasePath;
        startInfo.Environment["EF_TEST_FACTORY_OUTPUT"] = FactoryArgumentsPath;

        var logPath = Path.Combine(LogDirectory, Interlocked.Increment(ref _commandCount).ToString("D3"));
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, TestContext.Current.CancellationToken);
        var result = await ToolingProcess.RunAsync(
            startInfo, logPath, timeout ?? TimeSpan.FromMinutes(5), linkedSource.Token).ConfigureAwait(false);
        if (TestContext.Current.Test != null)
        {
            TestContext.Current.TestOutputHelper!.WriteLine($"{result.Command}{Environment.NewLine}Exit: {result.ExitCode}; logs: {logPath}.*");
            if (result.ExitCode != 0)
            {
                TestContext.Current.AddAttachment(
                    "Command " + _commandCount, ToolingProcessResult.Tail(result.StandardOutput + result.StandardError));
            }
        }

        return result;
    }

    public IEnumerable<string> GeneratedFiles(string stage)
        => Projects.SelectMany(project =>
                Directory.EnumerateFiles(Path.GetDirectoryName(project)!, $"*.EFGeneratedSources.{stage}.txt", SearchOption.AllDirectories))
            .SelectMany(File.ReadAllLines)
            .Where(line => line.Length > 0);

    public void MarkSuccessful()
        => _successful = true;

    public ValueTask DisposeAsync()
    {
        if (_successful && Directory.Exists(DirectoryPath))
        {
            Directory.Delete(DirectoryPath, recursive: true);
        }
        else
        {
            TestContext.Current.SendDiagnosticMessage("Retained tooling test project: {0}", DirectoryPath);
        }

        return ValueTask.CompletedTask;
    }

    private string CreateProject(string name, bool executable)
    {
        var directory = Path.Combine(DirectoryPath, name);
        Directory.CreateDirectory(directory);
        var project = Path.Combine(directory, name + ".csproj");
        File.WriteAllText(
            project,
            _fixture.Source("Project.csproj.template")
                .Replace("{{TargetFramework}}", _fixture.TargetFramework)
                .Replace("{{OutputType}}", executable ? "Exe" : "Library"));
        _projects.Add(project);
        return project;
    }

    private void AddProductReference(string project, string path)
    {
        var document = XDocument.Load(project);
        document.Root!.Add(
            new XElement("ItemGroup",
                new XElement("ProjectReference",
                    new XAttribute("Include", path),
                    new XAttribute("Targets", "GetTargetPath"),
                    new XAttribute("AdditionalProperties",
                        "Configuration=" + _fixture.Configuration + ";RuntimeIdentifier=;SelfContained=false;PublishAot=false;PublishTrimmed=false"))));
        document.Save(project);
    }

    private static void AddProjectReference(string project, string reference)
    {
        if (project == reference)
        {
            return;
        }

        var document = XDocument.Load(project);
        var path = Path.GetRelativePath(Path.GetDirectoryName(project)!, reference);
        if (!document.Descendants("ProjectReference").Any(element => (string?)element.Attribute("Include") == path))
        {
            document.Root!.Add(new XElement("ItemGroup", new XElement("ProjectReference", new XAttribute("Include", path))));
            document.Save(project);
        }
    }

    private static void AddPackage(string project, string name, string version)
    {
        var document = XDocument.Load(project);
        document.Root!.Add(new XElement("ItemGroup",
            new XElement("PackageReference", new XAttribute("Include", name), new XAttribute("Version", version))));
        document.Save(project);
    }

    private void ConfigureXunitFeed()
    {
        var path = Path.Combine(DirectoryPath, "NuGet.config");
        var document = XDocument.Load(path);
        var sources = document.Root!.Element("packageSources")!;
        var mapping = document.Root.Element("packageSourceMapping");
        if (mapping == null)
        {
            mapping = new XElement("packageSourceMapping",
                sources.Elements("add").Select(source =>
                    new XElement("packageSource", new XAttribute("key", source.Attribute("key")!.Value),
                        new XElement("package", new XAttribute("pattern", "*")))));
            document.Root.Add(mapping);
        }

        sources.Add(new XElement("add",
            new XAttribute("key", "tooling-xunit"), new XAttribute("value", "https://api.nuget.org/v3/index.json")));
        mapping.Add(new XElement("packageSource", new XAttribute("key", "tooling-xunit"),
            new XElement("package", new XAttribute("pattern", "xunit.*"))));
        document.Save(path);
    }

    private Task<ToolingProcessResult> RunBuildCommandAsync(string command, string project, IEnumerable<string>? arguments)
        => RunDotNetAsync(
            [command, project, "--no-restore", "--configuration", _fixture.Configuration, "--disable-build-servers",
                "--verbosity", "minimal", "-bl:" + Path.Combine(LogDirectory, $"{_commandCount + 1:D3}-{command}.binlog"),
                .. arguments ?? []],
            TimeSpan.FromMinutes(10));

    private void CreateDatabase()
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Blogs (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);
            INSERT INTO Blogs (Name) VALUES ('Alice'), ('Bob');
            CREATE TABLE "Blog Entries" (BlogKey INTEGER NOT NULL PRIMARY KEY, blog_title TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static string ProjectName(string project)
        => Path.GetFileNameWithoutExtension(project);
}
