// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore.Tasks.Internal;

namespace Microsoft.EntityFrameworkCore.Tasks;

/// <summary>
///     Generates files that contain tailored code for some runtime services.
/// </summary>
public class OptimizeDbContext : OperationTaskBase
{
    /// <summary>
    ///     The type of the target DbContext.
    /// </summary>
    public string? DbContextType { get; set; }

    /// <summary>
    ///     The namespace to use for the generated classes.
    /// </summary>
    public string? TargetNamespace { get; set; }

    /// <summary>
    ///     The output directory. Usually, relative to the project directory.
    /// </summary>
    public ITaskItem? OutputDir { get; set; }

    /// <summary>
    ///     Don't generate a compiled model.
    /// </summary>
    public bool NoScaffold { get; set; }

    /// <summary>
    ///     Generate precompiled queries.
    /// </summary>
    public bool PrecompileQueries { get; set; }

    /// <summary>
    ///     Generated files that should be include in the build.
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; private set; } = null!;

    /// <inheritdoc />
    public override bool Execute()
    {
        try
        {
            Log.LogMessage(MessageImportance.High, "Optimizing DbContext...");

            AdditionalArguments.Add("dbcontext");
            AdditionalArguments.Add("optimize");
            if (OutputDir != null)
            {
                AdditionalArguments.Add("--output-dir");
                AdditionalArguments.Add(OutputDir.ItemSpec);
            }

            var targetNamespace = MsBuildUtilities.TrimAndGetNullForEmpty(TargetNamespace);
            if (targetNamespace != null)
            {
                AdditionalArguments.Add("--namespace");
                AdditionalArguments.Add(targetNamespace);
            }

            var dbContextType = MsBuildUtilities.TrimAndGetNullForEmpty(DbContextType);
            if (dbContextType != null)
            {
                AdditionalArguments.Add("--context");
                AdditionalArguments.Add(dbContextType);
            }

            if (NoScaffold)
            {
                AdditionalArguments.Add("--no-scaffold");
            }

            if (PrecompileQueries)
            {
                AdditionalArguments.Add("--precompile-queries");
            }

            AdditionalArguments.Add("--nativeaot");

            AdditionalArguments.Add("--suffix");
            AdditionalArguments.Add(".g");

            var success = base.Execute();
            AdditionalArguments.Clear();

            if (!success
                || Output == null)
            {
                return false;
            }

            GeneratedFiles = Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(f => new TaskItem(f)).ToArray();
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e);
        }

        return !Log.HasLoggedErrors;
    }
}
