// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiChief.Model;

namespace ApiChief.Commands;

internal static class CheckBreakingChanges
{

    private sealed class CheckBreakingChangesArgs
    {
        public FileInfo? AssemblyPath { get; set; }

        public string BaselinePath { get; set; } = string.Empty;
    }

    public static Command Create(Argument<FileInfo> assemblyPathArgument)
    {
        var baselinePathArgument = new Argument<string>("baseline-path")
        {
            Description = "Path to the baseline report to use for reference"
        };

        var command = new Command("breaking", "Performs a breaking change check")
        {
            baselinePathArgument,
        };

        command.SetAction(parseResult => ExecuteAsync(new CheckBreakingChangesArgs
        {
            AssemblyPath = parseResult.GetValue(assemblyPathArgument),
            BaselinePath = parseResult.GetValue(baselinePathArgument) ?? string.Empty,
        }));

        return command;
    }


    private static Task<int> ExecuteAsync(CheckBreakingChangesArgs args)
    {
        ApiModel current;
        ApiModel baseline;

        try
        {
            current = ApiModel.LoadFromAssembly(args.AssemblyPath!.FullName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to create the current API baseline report from '{args.AssemblyPath!.FullName}': {ex.Message}");

            return Task.FromResult(-1);
        }

        try
        {
            baseline = ApiModel.LoadFromFile(args.BaselinePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to load previous API baseline report '{args.BaselinePath}': {ex.Message}");

            return Task.FromResult(-1);
        }

        baseline.EvaluateDelta(current);

        if (current.HasRemovals())
        {
            Console.Error.WriteLine($"Detected removed APIs in the current baseline report: {string.Join(';', current.Types.Where(type => type.Removals != null).Select(type => type.Type))}");

            return Task.FromResult(-1);
        }

        return Task.FromResult(0);
    }
}
