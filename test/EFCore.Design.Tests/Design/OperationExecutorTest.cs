// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Design;

public class OperationExecutorTest
{
    [ConditionalFact]
    public void Ctor_validates_arguments()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(null!, null!));
        Assert.Equal("reportHandler", ex.ParamName);

        ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(new OperationReportHandler(), null!));
        Assert.Equal("args", ex.ParamName);
    }

    [ConditionalTheory]
    [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("MgOne", "MgOne")]
    [InlineData("Name with Spaces", "NamewithSpaces")]
    [InlineData(" Space Space ", "SpaceSpace")]
    public void AddMigration_can_scaffold_for_different_names(string migrationName, string processedMigrationName)
        => TestAddMigrationPositive(
            migrationName, processedMigrationName,
            "output", "output",
            ProductInfo.GetVersion());

    [ConditionalTheory] // Issue #24024
    [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("to fix error: add column is_deleted")]
    [InlineData(@"A\B\C")]
    public void AddMigration_errors_for_bad_names(string migrationName)
        => TestAddMigrationNegative(
            migrationName,
            "output",
            ProductInfo.GetVersion(),
            typeof(OperationException),
            DesignStrings.BadMigrationName(migrationName, string.Join("','", Path.GetInvalidFileNameChars())));

    [ConditionalTheory]
    [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("output", "output")]
    [InlineData("Name with Spaces", "Name with Spaces")]
    [InlineData(" Space Space", " Space Space")]
    public void AddMigration_can_scaffold_for_different_output_dirs(string outputDir, string processedOutputDir)
        => TestAddMigrationPositive(
            "MgTwo", "MgTwo",
            outputDir, processedOutputDir,
            ProductInfo.GetVersion());

    [ConditionalTheory]
    [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("Something:Else")]
    public void AddMigration_errors_for_bad_output_dirs(string outputDir)
        => TestAddMigrationNegative("MgTwo", outputDir, ProductInfo.GetVersion(), typeof(IOException), null);

    [ConditionalFact]
    public void AddMigration_errors_if_migration_name_is_same_as_context_name()
        => TestAddMigrationNegative(
            "GnomeContext", "output", ProductInfo.GetVersion(), typeof(OperationException),
            DesignStrings.ConflictingContextAndMigrationName("GnomeContext"));

    private void TestAddMigrationPositive(
        string migrationName, string processedMigrationName,
        string outputDir, string processedOutputDir,
        string productVersion)
    {
        using var tempPath = new TempDirectory();
        var resultHandler = ExecuteAddMigration(tempPath, migrationName, outputDir, productVersion);

        Assert.True(resultHandler.HasResult);
        var files = (Hashtable)resultHandler.Result!;
        Assert.Equal(3, files.Count);
        var metadataFilePath = (string)files["MetadataFile"]!;
        var migrationFilePath = (string)files["MigrationFile"]!;
        var snapshotFilePath = (string)files["SnapshotFile"]!;

        Assert.StartsWith(tempPath, metadataFilePath);
        Assert.StartsWith(tempPath, migrationFilePath);
        Assert.StartsWith(tempPath, snapshotFilePath);

        var metadataFileName = metadataFilePath.Substring(tempPath.Path.Length + 1);
        var migrationFileName = migrationFilePath.Substring(tempPath.Path.Length + 1);
        var snapshotFileName = snapshotFilePath.Substring(tempPath.Path.Length + 1);

        Assert.Equal(Path.Combine(processedOutputDir, $"11112233445566_{migrationName}.Designer.cs"), metadataFileName);
        Assert.Equal(Path.Combine(processedOutputDir, $"11112233445566_{migrationName}.cs"), migrationFileName);

        var snapshotDir = "";
        foreach (var part in "My.Gnomespace.Data".Split('.'))
        {
            snapshotDir = Path.Combine(snapshotDir, part);
        }

        Assert.Equal(Path.Combine(snapshotDir, "GnomeContextModelSnapshot.cs"), snapshotFileName);

        var metadataFile = File.ReadAllText(metadataFilePath);
        var migrationFile = File.ReadAllText(migrationFilePath);
        var snapshotFile = File.ReadAllText(snapshotFilePath);

        Assert.Equal(
            $$"""
// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace My.Gnomespace.Data
{
    [DbContext(typeof(OperationExecutorTest.GnomeContext))]
    [Migration("11112233445566_{{migrationName}}")]
    partial class {{processedMigrationName}}
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "{{productVersion}}");
#pragma warning restore 612, 618
        }
    }
}

""", metadataFile);

        Assert.Equal(
            $$"""
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My.Gnomespace.Data
{
    /// <inheritdoc />
    public partial class {{processedMigrationName}} : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

""", migrationFile);

        Assert.Equal(
            $$"""
// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace My.Gnomespace.Data
{
    [DbContext(typeof(OperationExecutorTest.GnomeContext))]
    partial class GnomeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "{{productVersion}}");
#pragma warning restore 612, 618
        }
    }
}

""", snapshotFile);
    }

    private void TestAddMigrationNegative(
        string migrationName,
        string outputDir,
        string productVersion,
        Type errorType,
        string? message)
    {
        using var tempPath = new TempDirectory();
        var resultHandler = ExecuteAddMigration(tempPath, migrationName, outputDir, productVersion);

        Assert.False(resultHandler.HasResult);
        Assert.Equal(errorType.FullName, resultHandler.ErrorType);

        if (message != null)
        {
            Assert.Equal(message, resultHandler.ErrorMessage);
        }
    }

    private static OperationResultHandler ExecuteAddMigration(
        string tempPath,
        string migrationName,
        string outputDir,
        string productVersion)
    {
        var reportHandler = new OperationReportHandler();
        var resultHandler = new OperationResultHandler();
        var assembly = Assembly.GetExecutingAssembly();
        var executor = new OperationExecutor(
            reportHandler,
            new Dictionary<string, object?>
            {
                { "targetName", assembly.FullName },
                { "startupTargetName", assembly.FullName },
                { "projectDir", tempPath },
                { "rootNamespace", "My.Gnomespace.Data" },
                { "language", "C#" },
                { "nullable", false },
                { "toolsVersion", productVersion },
                { "remainingArguments", null }
            });

        // ReSharper disable once ObjectCreationAsStatement
        new OperationExecutor.AddMigration(
            executor,
            resultHandler,
            new Dictionary<string, object?>
            {
                { "name", migrationName },
                { "outputDir", Path.Combine(tempPath, outputDir) },
                { "contextType", "GnomeContext" },
                { "namespace", "My.Gnomespace.Data" }
            });

        return resultHandler;
    }

    public class OperationBaseTests
    {
        [ConditionalFact]
        public void Execute_catches_exceptions()
        {
            var handler = new OperationResultHandler();
            var error = new ArgumentOutOfRangeException("Needs to be about 20% more cool.");

            new MockOperation<string>(handler, (Action)(() => throw error));

            Assert.Equal(error.GetType().FullName, handler.ErrorType);
            Assert.Equal(error.Message, handler.ErrorMessage);
            Assert.NotEmpty(handler.ErrorStackTrace!);
        }

        [ConditionalFact]
        public void Execute_sets_results()
        {
            var handler = new OperationResultHandler();
            var result = "Twilight Sparkle";

            new MockOperation<string>(handler, () => result);

            Assert.Equal(result, handler.Result);
        }

        [ConditionalFact]
        public void Execute_enumerates_results()
        {
            var handler = new OperationResultHandler();

            new MockOperation<string>(handler, () => YieldResults());

            Assert.IsType<string[]>(handler.Result);
            Assert.Equal(new[] { "Twilight Sparkle", "Princess Celestia" }, handler.Result);
        }

        private IEnumerable<string> YieldResults()
        {
            yield return "Twilight Sparkle";
            yield return "Princess Celestia";
        }

        private class MockOperation<T> : OperationExecutor.OperationBase
        {
            public MockOperation(IOperationResultHandler resultHandler, Action action)
                : base(resultHandler)
            {
                Execute(action);
            }

            public MockOperation(IOperationResultHandler resultHandler, Func<T> action)
                : base(resultHandler)
            {
                Execute(action);
            }

            public MockOperation(IOperationResultHandler resultHandler, Func<IEnumerable<T>> action)
                : base(resultHandler)
            {
                Execute(action);
            }
        }
    }

    public class GnomeContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlite()
                .ReplaceService<IMigrationsIdGenerator, FakeMigrationsIdGenerator>();

        private class FakeMigrationsIdGenerator : MigrationsIdGenerator
        {
            public override string GenerateId(string name)
                => "11112233445566_" + name;
        }
    }
}
