// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Tools;

[Trait("Suite", "CLI")]
public class MigrationsCommandTest(ToolingTestAssemblyFixture fixture)
{
    [Theory(Timeout = 600_000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Separate_migrations_workflow(bool migrationsStartup)
    {
        await using var project = fixture.CreateProject(
            nameof(Separate_migrations_workflow), ToolingProjectLayout.SeparateMigrations);
        File.Delete(project.DatabasePath);
        await project.RestoreAsync();
        var startup = migrationsStartup ? project.MigrationsProject : project.StartupProject;
        (await project.EfAsync(
            ["migrations", "add", "Initial", "--output-dir", "Migrations"],
            project.MigrationsProject, startup)).EnsureSuccess();

        var migrationDirectory = Path.Combine(Path.GetDirectoryName(project.MigrationsProject)!, "Migrations");
        Assert.Single(Directory.GetFiles(migrationDirectory, "*_Initial.cs"));
        Assert.Single(Directory.GetFiles(migrationDirectory, "*ModelSnapshot.cs"));
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(project.ContextProject)!, "*ModelSnapshot.cs", SearchOption.AllDirectories));
        (await project.BuildAsync()).EnsureSuccess();
        using (var listed = CommandData.ReadJson(
                   await project.EfAsync(["migrations", "list", "--no-connect", "--no-build", "--json"], project.MigrationsProject, startup)))
        {
            var migration = Assert.Single(listed.RootElement.EnumerateArray());
            Assert.Equal("Initial", migration.GetProperty("name").GetString());
        }

        var script = Path.Combine(project.DirectoryPath, "migration.sql");
        (await project.EfAsync(
            ["migrations", "script", "--no-build", "--output", script], project.MigrationsProject, startup)).EnsureSuccess();
        Assert.Contains("CREATE TABLE \"Blogs\"", File.ReadAllText(script));
        (await project.EfAsync(["database", "update", "--no-build"], project.MigrationsProject, startup)).EnsureSuccess();
        Assert.Equal(1L, Scalar(project.DatabasePath, """SELECT COUNT(*) FROM "__EFMigrationsHistory";"""));
        var application = await project.RunApplicationAsync("migrate");
        application.EnsureSuccess();
        Assert.Contains("MIGRATED", application.StandardOutput);

        (await project.EfAsync(
            ["migrations", "remove", "--force", "--no-build"], project.MigrationsProject, startup)).EnsureSuccess();
        Assert.Empty(Directory.GetFiles(migrationDirectory, "*_Initial.cs"));
        Assert.Empty(Directory.GetFiles(migrationDirectory, "*ModelSnapshot.cs"));
        Assert.Equal(0L, Scalar(project.DatabasePath, """SELECT COUNT(*) FROM "__EFMigrationsHistory";"""));
        project.MarkSuccessful();
    }

    private static object? Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}
