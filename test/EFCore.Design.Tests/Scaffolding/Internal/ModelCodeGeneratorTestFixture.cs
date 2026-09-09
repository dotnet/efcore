// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class ModelCodeGeneratorTestFixture : IDisposable
{
    public ModelCodeGeneratorTestFixture()
    {
        var templatesDir = Path.Combine(ProjectDir, "CodeTemplates", "EFCore");
        Directory.CreateDirectory(templatesDir);

        using (var input = typeof(ModelCodeGeneratorTestBase).Assembly.GetManifestResourceStream(
                   "Microsoft.EntityFrameworkCore.Resources.CSharpDbContextGenerator.tt"))
        using (var output = File.Create(Path.Combine(templatesDir, "DbContext.t4")))
        {
            input.CopyTo(output);
        }

        using (var input = typeof(ModelCodeGeneratorTestBase).Assembly.GetManifestResourceStream(
                   "Microsoft.EntityFrameworkCore.Resources.CSharpEntityTypeGenerator.tt"))
        using (var output = File.Create(Path.Combine(templatesDir, "EntityType.t4")))
        {
            input.CopyTo(output);
        }
    }

    public TempDirectory ProjectDir { get; } = new();

    public void Dispose()
        => ProjectDir.Dispose();
}
