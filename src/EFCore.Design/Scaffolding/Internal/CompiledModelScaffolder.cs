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
    public static readonly int MaxFileNameLength = 255;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompiledModelScaffolder(ICompiledModelCodeGeneratorSelector modelCodeGeneratorSelector)
        => ModelCodeGeneratorSelector = modelCodeGeneratorSelector;

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
        var scaffoldedModel = ModelCodeGeneratorSelector.Select(options).GenerateModel(model, options);
        return WriteFiles(scaffoldedModel, outputDir);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<string> WriteFiles(
        IReadOnlyCollection<ScaffoldedFile> scaffoldedModel,
        string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var paths = scaffoldedModel.Select(f => f.Path).ToList();

        var readOnlyFiles = new List<string>();
        var savedFiles = new List<string>();
        foreach (var file in scaffoldedModel)
        {
            var fullPath = Path.Combine(outputDir, file.Path);

            if (File.Exists(fullPath)
                && File.GetAttributes(fullPath).HasFlag(FileAttributes.ReadOnly))
            {
                readOnlyFiles.Add(file.Path);
            }
            else
            {
                File.WriteAllText(fullPath, file.Code, Encoding.UTF8);
                savedFiles.Add(fullPath);
            }
        }

        return readOnlyFiles.Count != 0
            ? throw new OperationException(
                DesignStrings.ReadOnlyFiles(
                    outputDir,
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, readOnlyFiles)))
            : (IReadOnlyList<string>)savedFiles;
    }
}
