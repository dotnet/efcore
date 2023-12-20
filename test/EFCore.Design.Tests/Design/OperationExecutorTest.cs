// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Design;

public class OperationExecutorTest(ITestOutputHelper testOutputHelper)
{
    private static readonly char S = Path.DirectorySeparatorChar;

    [ConditionalFact]
    public void Ctor_validates_arguments()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(null!, null!));
        Assert.Equal("reportHandler", ex.ParamName);

        ex = Assert.Throws<ArgumentNullException>(() => new OperationExecutor(new OperationReportHandler(), null!));
        Assert.Equal("args", ex.ParamName);
    }

    [ConditionalTheory]
    [PlatformSkipCondition(
        TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac,
        SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("MgOne", "MgOne")]
    [InlineData("Name with Spaces", "NamewithSpaces")]
    [InlineData(" Space Space ", "SpaceSpace")]
    public void AddMigration_can_scaffold_for_different_names(string migrationName, string processedMigrationName)
        => TestAddMigrationPositive(
            migrationName, processedMigrationName,
            "output", "output",
            ProductInfo.GetVersion());

    [ConditionalTheory] // Issue #24024
    [PlatformSkipCondition(
        TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac,
        SkipReason = "Tested negative cases and baselines are Windows-specific")]
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
    [PlatformSkipCondition(
        TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac,
        SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("output", "output")]
    [InlineData("Name with Spaces", "Name with Spaces")]
    [InlineData(" Space Space", " Space Space")]
    public void AddMigration_can_scaffold_for_different_output_dirs(string outputDir, string processedOutputDir)
        => TestAddMigrationPositive(
            "MgTwo", "MgTwo",
            outputDir, processedOutputDir,
            ProductInfo.GetVersion());

    [ConditionalTheory]
    [PlatformSkipCondition(
        TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac,
        SkipReason = "Tested negative cases and baselines are Windows-specific")]
    [InlineData("Something:Else")]
    public void AddMigration_errors_for_bad_output_dirs(string outputDir)
        => TestAddMigrationNegative("MgTwo", outputDir, ProductInfo.GetVersion(), typeof(IOException), null);

    [ConditionalFact]
    public void AddMigration_errors_if_migration_name_is_same_as_context_name()
        => TestAddMigrationNegative(
            "GnomeContext", "output", ProductInfo.GetVersion(), typeof(OperationException),
            DesignStrings.ConflictingContextAndMigrationName("GnomeContext"));

    private void TestAddMigrationPositive(
        string migrationName,
        string processedMigrationName,
        string outputDir,
        string processedOutputDir,
        string productVersion)
    {
        using var tempPath = new TempDirectory();
        var resultHandler = ExecuteAddMigration(tempPath, migrationName, Path.Combine(tempPath, outputDir), productVersion);

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

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath")]
    public void No_output_path(string projectDir, string expectedPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        var files = GenerateFilesDryRun(projectDir, null, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""null"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout", @"putout/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout", @"putout/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"putout", @"/putout/", @"SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/putout/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout/", @"putout/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/", @"putout/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/", @"/putout/", @"SomePath/SomeSubpath/")]
    public void Relative_output_path(string projectDir, string outputDir, string expectedPrefix, string expectedSnapshotPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}putout{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("putout", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("putout", files.Migration!.MigrationSubNamespace);
        Assert.Equal("putout", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("putout", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("putout", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout/output/", @"putout/output/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output/", @"putout/output/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"SomePath/SomeSubpath/")]
    public void Relative_multipart_output_path(string projectDir, string outputDir, string expectedPrefix, string expectedSnapshotPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}putout{S}output{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("putout.output", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("putout.output", files.Migration!.MigrationSubNamespace);
        Assert.Equal("putout.output", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("putout.output", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("putout.output", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout", @"/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout", @"/", @"SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/", @"", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/", @"", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout/", @"", @"SomePath/SomeSubpath/")]
    public void Absolute_output_path(string projectDir, string outputDir, string expectedPrefix, string expectedSnapshotPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output", @"/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout/output", @"/", @"SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output/", @"", @"/SomePath/SomeSubpath/")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output/", @"", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output/", @"", @"SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout/output/", @"", @"SomePath/SomeSubpath/")]
    public void Absolute_multipart_output_path(string projectDir, string outputDir, string expectedPrefix, string expectedSnapshotPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/")]
    public void Output_path_is_empty_string(string projectDir, string outputDir, string expectedPrefix, string expectedSnapshotPrefix)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath", "Acme")]
    public void No_output_path_with_root_namespace(string projectDir, string expectedPrefix, string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        var files = GenerateFilesDryRun(projectDir, null, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""null"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedPrefix}{S}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout", @"putout/", @"/SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/", @"putout/", @"SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/", @"/putout/", @"SomePath/SomeSubpath/", "Acme.Parts")]
    public void Relative_output_path_with_root_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}putout{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("putout", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("putout", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".putout", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".putout", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".putout", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"/SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output/", @"putout/output/", @"SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"SomePath/SomeSubpath/", "Acme.Parts")]
    public void Relative_multipart_output_path_with_root_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}putout{S}output{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("putout.output", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("putout.output", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".putout.output", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".putout.output", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".putout.output", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout", @"/", @"SomePath/SomeSubpath/", "Acme")]
    public void Absolute_output_path_with_root_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Acme")]
    public void Absolute_multipart_output_path_with_root_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/", "Acme.Parts")]
    public void Output_path_is_empty_string_with_root_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, null);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
        Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(rootNamespace + ".Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath", "Subway.To.Kfc")]
    public void No_output_path_with_sub_namespace(string projectDir, string expectedPrefix, string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        var files = GenerateFilesDryRun(projectDir, null, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""null"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        var namespacePath = subNamespace.Replace('.', S);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout", @"putout/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath", @"putout", @"/putout/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/putout/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout", @"putout/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath", @"putout", @"/putout/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/putout/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    public void Relative_output_path_with_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    public void Relative_multipart_output_path_with_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/", @"", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/", @"", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    public void Absolute_output_path_with_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output/", @"", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output/", @"", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output/", @"", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output/", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    public void Absolute_multipart_output_path_with_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/", "Subway")]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc")]
    public void Output_path_is_empty_string_with_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, null, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath", "Subway.To.Kfc", "Acme.Parts")]
    public void No_output_path_with_root_namespace_and_sub_namespace(
        string projectDir,
        string expectedPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        var files = GenerateFilesDryRun(projectDir, null, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""null"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        var namespacePath = subNamespace.Replace('.', S);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedPrefix}{S}{namespacePath}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout", @"putout/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout", @"/putout/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/putout/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/putout/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout", @"putout/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout", @"/putout/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/putout/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    public void Relative_output_path_with_root_namespace_and_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"putout/output", @"putout/output/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath", @"putout/output", @"/putout/output/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"putout/output/", @"/putout/output/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    public void Relative_multipart_output_path_with_root_namespace_and_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/", @"", @"/SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"", @"SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout", @"/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/", @"", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    public void Absolute_output_path_with_root_namespace_and_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output/", @"", @"/SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output/", @"", @"SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"/SomePath/SomeSubpath", @"/putout/output", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output", @"/", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath/", @"/putout/output/", @"", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    [InlineData(@"SomePath/SomeSubpath/", @"/putout/output/", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    public void Absolute_multipart_output_path_with_root_namespace_and_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(outputDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    [ConditionalTheory]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/", "Subway", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/", "Subway", "Acme")]
    [InlineData(@"/SomePath/SomeSubpath", @"", @"/", @"/SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme.Parts")]
    [InlineData(@"SomePath/SomeSubpath/", @"", @"", @"SomePath/SomeSubpath/", "Subway.To.Kfc", "Acme")]
    public void Output_path_is_empty_string_with_root_namespace_and_sub_namespace(
        string projectDir,
        string outputDir,
        string expectedPrefix,
        string expectedSnapshotPrefix,
        string rootNamespace,
        string subNamespace)
    {
        expectedPrefix = expectedPrefix.Replace('/', S);
        expectedSnapshotPrefix = expectedSnapshotPrefix.Replace('/', S);
        var basePath = Path.GetFullPath(projectDir);
        var files = GenerateFilesDryRun(projectDir, outputDir, rootNamespace, subNamespace);

        testOutputHelper.WriteLine(
            $@"\""{projectDir}"", ""{outputDir}"", ""{files.MigrationFile}"", ""{files.MetadataFile}"", ""{files.SnapshotFile}""");

        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
        Assert.Equal($@"{basePath}{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
        Assert.Equal($@"{expectedSnapshotPrefix}{subNamespace.Replace('.', S)}{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);

        Assert.Equal(subNamespace, files.Migration!.SnapshotSubnamespace);
        Assert.Equal(subNamespace, files.Migration!.MigrationSubNamespace);
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MigrationCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.MetadataCode));
        Assert.Equal(subNamespace, ExtractNamespace(files.Migration.SnapshotCode));
    }

    private static string ExtractNamespace(string migrationMigrationCode)
        => migrationMigrationCode.Split(Environment.NewLine).First(s => s.StartsWith("namespace ", StringComparison.Ordinal)).Substring(10);

    // [ConditionalTheory]
    // [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath")]
    // [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpat` h")]
    // public void Migration_files_are_created_in_the_Migrations_folder(string projectDir, string expectedPrefix)
    // {
    //     expectedPrefix = expectedPrefix.Replace('/', Path.DirectorySeparatorChar);
    //     var files = GenerateFilesDryRun(projectDir, null, null, null);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.cs", files.MigrationFile);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.Designer.cs", files.MetadataFile);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);
    //
    //     Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
    //     Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    // }
    //
    // private static string ExtractNamespace(string migrationMigrationCode)
    //     => migrationMigrationCode.Split(Environment.NewLine).First(s => s.StartsWith("namespace ", StringComparison.Ordinal)).Substring(10);
    //
    // [ConditionalTheory]
    // [InlineData(@"C:/SomePath/SomeSubpath", @"C:/SomePath/SomeSubpath")]
    // [InlineData(@"K:/SomePath/SomeSubpath/", @"K:/SomePath/SomeSubpath")]
    // [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "Windows-specific paths")]
    // public void Migration_files_are_created_in_the_Migrations_folder_with_drive_letter(string projectDir, string expectedPrefix)
    // {
    //     expectedPrefix = expectedPrefix.Replace('/', Path.DirectorySeparatorChar);
    //     var files = GenerateFilesDryRun(projectDir, null, null, null);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.cs", files.MigrationFile);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}11112233445566_M.Designer.cs", files.MetadataFile);
    //     Assert.Equal($@"{expectedPrefix}{S}Migrations{S}GnomeContextModelSnapshot.cs", files.SnapshotFile);
    //
    //     Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
    //     Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    // }
    //
    // [ConditionalTheory]
    // [InlineData(@"/SomePath/SomeSubpath", @"putout", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"putout/", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"putout/", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"putout/putout", @"/SomePath/SomeSubpath/putout/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"putout/putout/", @"/SomePath/SomeSubpath/putout/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"putout/putout/", @"/SomePath/SomeSubpath/putout/putout/")]
    // public void Migration_files_are_created_in_the_output_path(string projectDir, string outputDir, string expectedPrefix)
    // {
    //     expectedPrefix = Path.GetFullPath(expectedPrefix.Replace('/', Path.DirectorySeparatorChar));
    //     var files = GenerateFilesDryRun(projectDir, outputDir, null, null);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
    //     // Assert.Equal($@"{expectedPrefix}GnomeContextModelSnapshot.cs", files.SnapshotFile);
    //
    //     Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
    //     Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    // }
    //
    // [ConditionalTheory]
    // [InlineData(@"/SomePath/SomeSubpath", @"/putout", @"/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/putout/", @"/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"/putout/", @"/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/putout/putout", @"/putout/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/putout/putout/", @"/putout/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"/putout/putout/", @"/putout/putout/")]
    // public void Migration_files_are_created_in_the_absolute_output_path(string projectDir, string outputDir, string expectedPrefix)
    // {
    //     expectedPrefix = Path.GetFullPath(expectedPrefix.Replace('/', Path.DirectorySeparatorChar));
    //     var files = GenerateFilesDryRun(projectDir, outputDir, null, null);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
    //     // Assert.Equal($@"{expectedPrefix}GnomeContextModelSnapshot.cs", files.SnapshotFile);
    //
    //     Assert.Equal("Migrations", files.Migration!.SnapshotSubnamespace);
    //     Assert.Equal("Migrations", files.Migration!.MigrationSubNamespace);
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MigrationCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.MetadataCode));
    //     Assert.Equal("Migrations", ExtractNamespace(files.Migration.SnapshotCode));
    // }
    //
    // [ConditionalTheory]
    // [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath/putout", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath/putout/", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath/putout/", @"/SomePath/SomeSubpath/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath/putout/putout", @"/SomePath/SomeSubpath/putout/putout/")]
    // [InlineData(@"/SomePath/SomeSubpath", @"/SomePath/SomeSubpath/putout/putout/", @"/SomePath/SomeSubpath/putout/putout/")]
    // [InlineData(@"SomePath/SomeSubpath/", @"SomePath/SomeSubpath/putout/putout/", @"/SomePath/SomeSubpath/putout/putout/")]
    // public void Migration_files_are_created_in_the_output_path_when_subpath(string projectDir, string outputDir, string expectedPrefix)
    // {
    //     expectedPrefix = Path.GetFullPath(expectedPrefix.Replace('/', Path.DirectorySeparatorChar));
    //     var files = GenerateFilesDryRun(projectDir, outputDir, null, null);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.cs", files.MigrationFile);
    //     // Assert.Equal($@"{expectedPrefix}11112233445566_M.Designer.cs", files.MetadataFile);
    //     // Assert.Equal($@"{expectedPrefix}GnomeContextModelSnapshot.cs", files.SnapshotFile);
    //
    //     Assert.Equal("putout", files.Migration!.SnapshotSubnamespace);
    //     Assert.Equal("putout", files.Migration!.MigrationSubNamespace);
    //     Assert.Equal("putout", ExtractNamespace(files.Migration.MigrationCode));
    //     Assert.Equal("putout", ExtractNamespace(files.Migration.MetadataCode));
    //     Assert.Equal("putout", ExtractNamespace(files.Migration.SnapshotCode));
    // }

    private MigrationFiles GenerateFilesDryRun(string projectDir, string? outputDir, string? rootNamespace, string? @namespace)
    {
        projectDir = projectDir.Replace('/', Path.DirectorySeparatorChar);
        outputDir = outputDir?.Replace('/', Path.DirectorySeparatorChar);

        var reportHandler = new OperationReportHandler();
        var assembly = Assembly.GetExecutingAssembly();
        var executor = new OperationExecutor(
            reportHandler,
            new Dictionary<string, object?>
            {
                { "targetName", assembly.FullName },
                { "startupTargetName", assembly.FullName },
                { "projectDir", projectDir },
                { "rootNamespace", rootNamespace },
                { "language", "C#" },
                { "nullable", false },
                { "toolsVersion", ProductInfo.GetVersion() },
                { "remainingArguments", null }
            });

        return executor.MigrationsOperations.AddMigration("M", outputDir, nameof(GnomeContext), @namespace, dryRun: true);
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
