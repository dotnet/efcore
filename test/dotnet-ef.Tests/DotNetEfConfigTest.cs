// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class DotNetEfConfigTest
{
    [Fact]
    public void Load_returns_null_when_config_is_absent()
    {
        using var directory = new TestDirectory();

        Assert.Null(DotNetEfConfigLoader.Load(directory.Path));
    }

    [Fact]
    public void Load_discovers_and_applies_config_in_current_directory()
    {
        using var directory = new TestDirectory();
        var configFile = directory.CreateConfig(
            """
            {
              "project": "src/App.Infrastructure",
              "startupProject": "src/App.Api",
              "context": "AppDbContext",
              "framework": "net9.0",
              "configuration": "Debug"
            }
            """);

        var config = DotNetEfConfigLoader.Load(directory.Path);

        Assert.NotNull(config);
        Assert.Equal(Path.GetFullPath(configFile), config!.Path);
        Assert.Equal(Path.Combine(directory.Path, "src", "App.Infrastructure"), config.Project);
        Assert.Equal(Path.Combine(directory.Path, "src", "App.Api"), config.StartupProject);
        Assert.Equal("AppDbContext", config.Context);
        Assert.Equal("net9.0", config.Framework);
        Assert.Equal("Debug", config.Configuration);
    }

    [Fact]
    public void Load_discovers_config_in_parent_directory()
    {
        using var directory = new TestDirectory();
        var parentConfig = directory.CreateConfig("""{ "framework": "net9.0" }""");
        var workingDirectory = Directory.CreateDirectory(Path.Combine(directory.Path, "src", "App")).FullName;

        var config = DotNetEfConfigLoader.Load(workingDirectory);

        Assert.NotNull(config);
        Assert.Equal(Path.GetFullPath(parentConfig), config!.Path);
        Assert.Equal("net9.0", config.Framework);
    }

    [Fact]
    public void Load_prefers_nearest_config()
    {
        using var directory = new TestDirectory();
        directory.CreateConfig("""{ "framework": "net8.0" }""");
        var nestedDirectory = Directory.CreateDirectory(Path.Combine(directory.Path, "src", "App")).FullName;
        var nestedConfigDirectory = Directory.CreateDirectory(Path.Combine(nestedDirectory, ".config")).FullName;
        var nestedConfigFile = Path.Combine(nestedConfigDirectory, "dotnet-ef.json");
        File.WriteAllText(nestedConfigFile, """{ "framework": "net9.0" }""");

        var config = DotNetEfConfigLoader.Load(nestedDirectory);

        Assert.NotNull(config);
        Assert.Equal(Path.GetFullPath(nestedConfigFile), config!.Path);
        Assert.Equal("net9.0", config.Framework);
    }

    [Fact]
    public void Load_resolves_relative_paths_against_config_directory()
    {
        using var directory = new TestDirectory();
        var repositoryDirectory = Directory.CreateDirectory(Path.Combine(directory.Path, "repo")).FullName;
        var currentDirectory = Directory.CreateDirectory(Path.Combine(repositoryDirectory, "tools")).FullName;
        var configFile = CreateConfig(
            repositoryDirectory,
            """
            {
              "project": "src/App.Infrastructure",
              "startupProject": "src/App.Api"
            }
            """);

        var config = DotNetEfConfigLoader.Load(currentDirectory);

        Assert.NotNull(config);
        Assert.Equal(Path.Combine(repositoryDirectory, "src", "App.Infrastructure"), config!.Project);
        Assert.Equal(Path.Combine(repositoryDirectory, "src", "App.Api"), config.StartupProject);
    }

    [Fact]
    public void Load_preserves_absolute_paths()
    {
        using var directory = new TestDirectory();
        var projectPath = Path.Combine(directory.Path, "src", "App.Infrastructure", "App.Infrastructure.csproj");
        var startupProjectPath = Path.Combine(directory.Path, "src", "App.Api", "App.Api.csproj");
        directory.CreateConfig(
            $$"""
            {
              "project": "{{projectPath.Replace("\\", "\\\\")}}",
              "startupProject": "{{startupProjectPath.Replace("\\", "\\\\")}}"
            }
            """);

        var config = DotNetEfConfigLoader.Load(directory.Path);

        Assert.NotNull(config);
        Assert.Equal(projectPath, config!.Project);
        Assert.Equal(startupProjectPath, config.StartupProject);
    }

    [Theory]
    [InlineData("", "Fix the JSON and try again.")]
    [InlineData("{", "Fix the JSON and try again.")]
    [InlineData("null", "must contain a JSON object")]
    [InlineData("[]", "must contain a JSON object")]
    [InlineData("""{ "framework": 1 }""", "must be a non-empty JSON string")]
    [InlineData("""{ "framework": "" }""", "must be a non-empty JSON string")]
    [InlineData("""{ "framework": "   " }""", "must be a non-empty JSON string")]
    [InlineData("""{ "extra": "value" }""", "Remove the unsupported 'extra' property")]
    [InlineData("""{ "connection": "Data Source=test.db" }""", "The 'connection' property isn't supported")]
    [InlineData("""{ "connectionString": "Data Source=test.db" }""", "The 'connectionString' property isn't supported")]
    [InlineData("""{ "provider": "SqlServer" }""", "The 'provider' property isn't supported")]
    [InlineData("""{ "runtime": "win-x64" }""", "The 'runtime' property isn't supported")]
    public void Load_rejects_invalid_config(string contents, string messageFragment)
    {
        using var directory = new TestDirectory();
        var configFile = directory.CreateConfig(contents);

        var exception = Assert.Throws<CommandException>(() => DotNetEfConfigLoader.Load(directory.Path));

        Assert.Contains(Path.GetFullPath(configFile), exception.Message);
        Assert.Contains(messageFragment, exception.Message);
    }

    [Fact]
    public void Load_rejects_unreadable_config()
    {
        using var directory = new TestDirectory();
        var configFile = directory.CreateConfig("""{ "framework": "net9.0" }""");

        using var stream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.None);

        var exception = Assert.Throws<CommandException>(() => DotNetEfConfigLoader.Load(directory.Path));

        Assert.Contains(Path.GetFullPath(configFile), exception.Message);
        Assert.Contains("Ensure the file is accessible and try again.", exception.Message);
    }

    [Fact]
    public void Resolve_context_uses_config_for_supported_commands()
    {
        var context = RootCommand.ResolveContext(["migrations", "add", "InitialCreate"], "AppDbContext");

        Assert.Equal("AppDbContext", context);
    }

    [Theory]
    [InlineData("migrations", "add", "-c", "ExplicitContext")]
    [InlineData("migrations", "add", "--context", "ExplicitContext")]
    [InlineData("dbcontext", "scaffold", "--provider", "SqlServer")]
    public void Resolve_context_does_not_apply_when_explicit_or_unsupported(
        string command,
        string subcommand,
        string optionName,
        string optionValue)
    {
        var context = RootCommand.ResolveContext([command, subcommand, optionName, optionValue], "AppDbContext");

        Assert.Null(context);
    }

    [Fact]
    public void Create_remaining_arguments_adds_only_missing_config_defaults()
    {
        var args = RootCommand.CreateRemainingArguments(
            ["migrations", "add", "InitialCreate"],
            "AppDbContext");

        Assert.Equal(
        [
            "migrations",
            "add",
            "InitialCreate",
            "--context",
            "AppDbContext"
        ], args);
    }

    [Fact]
    public void Create_remaining_arguments_preserves_existing_arguments_when_config_is_absent()
    {
        var args = RootCommand.CreateRemainingArguments(
            ["database", "update", "--runtime", "win-x64"],
            null);

        Assert.Equal(["database", "update", "--runtime", "win-x64"], args);
    }

    private static string CreateConfig(string directory, string contents)
    {
        var configDirectory = Directory.CreateDirectory(Path.Combine(directory, ".config")).FullName;
        var configFile = Path.Combine(configDirectory, "dotnet-ef.json");
        File.WriteAllText(configFile, contents);
        return configFile;
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateConfig(string contents)
            => DotNetEfConfigTest.CreateConfig(Path, contents);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
