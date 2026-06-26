// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using ApiChief.Model;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public static class ApiBaselineTest
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static bool IsCi
        => Environment.GetEnvironmentVariable("CI") == "true"
            || Environment.GetEnvironmentVariable("BUILD_BUILDID") != null
            || Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null
            || Environment.GetEnvironmentVariable("GITHUB_RUN_ID") != null;

    public static void AssertBaselineMatch(string projectName, string assemblyFileName)
    {
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, assemblyFileName);
        Assert.True(File.Exists(assemblyPath), $"Assembly not found: {assemblyPath}");

        var baselinePath = Path.Combine(RepoRoot, "src", projectName, $"{projectName}.baseline.json");

        var current = ApiModel.LoadFromAssembly(assemblyPath);

        if (!File.Exists(baselinePath))
        {
            if (IsCi)
            {
                Assert.Fail($"Baseline file not found: {baselinePath}");
            }

            File.WriteAllText(baselinePath, current.ToString());
            return;
        }

        var baseline = ApiModel.LoadFromFile(baselinePath);
        baseline.EvaluateDelta(current);

        if (current.Types.Count > 0)
        {
            if (IsCi)
            {
                var additions = current.Types
                    .Where(t => t.Additions != null)
                    .Select(t => t.Type)
                    .ToList();

                var removals = current.Types
                    .Where(t => t.Removals != null)
                    .Select(t => t.Type)
                    .ToList();

                var message =
                    $"API baseline mismatch for {projectName}. "
                    + $"Update the baselines by running the tests locally.{Environment.NewLine}"
                    + (additions.Count > 0
                        ? $"  Types with additions: {string.Join(", ", additions)}{Environment.NewLine}"
                        : "")
                    + (removals.Count > 0
                        ? $"  Types with removals: {string.Join(", ", removals)}{Environment.NewLine}"
                        : "")
                    + $"{Environment.NewLine}Delta:{Environment.NewLine}{current}";

                Assert.Fail(message);
            }

            // Running locally — regenerate the baseline from the assembly
            var updated = ApiModel.LoadFromAssembly(assemblyPath);
            File.WriteAllText(baselinePath, updated.ToString());
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EFCore.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException(
            "Could not find repository root. Ensure the test is run from within the EF Core repository.");
    }
}
