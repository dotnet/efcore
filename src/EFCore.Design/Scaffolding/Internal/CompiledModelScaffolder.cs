// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompiledModelScaffolder : ICompiledModelScaffolder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompiledModelScaffolder(ICompiledModelCodeGeneratorSelector modelCodeGeneratorSelector)
    {
        ModelCodeGeneratorSelector = modelCodeGeneratorSelector;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private ICompiledModelCodeGeneratorSelector ModelCodeGeneratorSelector { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<string> ScaffoldModel(
        IModel model,
        string outputDir,
        CompiledModelCodeGenerationOptions options)
    {
        var codeGenerator = ModelCodeGeneratorSelector.Select(options);

        var scaffoldedModel = codeGenerator.GenerateModel(model, options);

        CheckOutputFiles(scaffoldedModel, outputDir);

        Directory.CreateDirectory(outputDir);

        var savedFiles = new List<string>();
        foreach (var file in scaffoldedModel)
        {
            var filePath = Path.Combine(outputDir, file.Path);
            File.WriteAllText(filePath, file.Code, Encoding.UTF8);
            savedFiles.Add(filePath);
        }

        return savedFiles;
    }

    private static void CheckOutputFiles(
        IReadOnlyCollection<ScaffoldedFile> scaffoldedModel,
        string outputDir)
    {
        var paths = scaffoldedModel.Select(f => f.Path).ToList();

        var existingFiles = new List<string>();
        var readOnlyFiles = new List<string>();
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(outputDir, path);

            if (File.Exists(fullPath))
            {
                existingFiles.Add(path);

                if (File.GetAttributes(fullPath).HasFlag(FileAttributes.ReadOnly))
                {
                    readOnlyFiles.Add(path);
                }
            }
        }

        if (readOnlyFiles.Count != 0)
        {
            throw new OperationException(
                DesignStrings.ReadOnlyFiles(
                    outputDir,
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, readOnlyFiles)));
        }
    }
}
