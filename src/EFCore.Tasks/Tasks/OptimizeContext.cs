// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore.Tasks.Internal;

namespace Microsoft.EntityFrameworkCore.Tasks;

/// <summary>
///     Generates files that contain tailored code for some runtime services.
/// </summary>
public class OptimizeContext : OperationTaskBase
{
    /// <summary>
    ///     The name of the target DbContext.
    /// </summary>
    public string? DbContextName { get; set; }

    /// <summary>
    ///     The namespace to use for the generated classes.
    /// </summary>
    public string? TargetNamespace { get; set; }

    /// <summary>
    ///     The output directory. Usually, relative to the project directory.
    /// </summary>
    public ITaskItem? OutputDir { get; set; }

    /// <summary>
    ///     Generated files that should be include in the build.
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; private set; } = null!;

    /// <inheritdoc/>
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

            var dbContextName = MsBuildUtilities.TrimAndGetNullForEmpty(DbContextName);
            if(dbContextName != null)
            {
                AdditionalArguments.Add("--context");
                AdditionalArguments.Add(dbContextName);
            }

            AdditionalArguments.Add("--suffix");
            AdditionalArguments.Add(".g");

            var success = base.Execute();
            AdditionalArguments.Clear();

            if (!success
                || Output == null)
            {
                return false;
            }

            GeneratedFiles = Output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(f => new TaskItem(f)).ToArray();
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e);
        }

        return !Log.HasLoggedErrors;
    }
}
