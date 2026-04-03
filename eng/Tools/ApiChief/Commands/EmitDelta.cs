// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using ApiChief.Model;

namespace ApiChief.Commands;

internal static class EmitDelta
{
    public const int NoChangesExitCode = 2;

    private sealed class EmitDeltaArgs
    {
        public FileInfo? AssemblyPath { get; set; }

        public string BaselinePath { get; set; } = string.Empty;

        public string? Output { get; set; }
    }

    public static Command Create(Argument<FileInfo> assemblyPathArgument)
    {
        var baselinePathArgument = new Argument<string>("baseline-path")
        {
            Description = "Path to the baseline report to use for reference"
        };

        var outputOption = new Option<string?>("--output", ["-o"])
        {
            Description = "Path of the delta file to produce"
        };

        var command = new Command("delta", "Creates an API delta")
        {
            baselinePathArgument,
            outputOption,
        };

        command.SetAction(parseResult => ExecuteAsync(new EmitDeltaArgs
        {
            AssemblyPath = parseResult.GetValue(assemblyPathArgument),
            BaselinePath = parseResult.GetValue(baselinePathArgument) ?? string.Empty,
            Output = parseResult.GetValue(outputOption),
        }));

        return command;
    }


    private static async Task<int> ExecuteAsync(EmitDeltaArgs args)
    {
        ApiModel current;
        ApiModel baseline;

        try
        {
            current = LoadCurrentModel(args.AssemblyPath!.FullName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to load current API model from '{args.AssemblyPath!.FullName}': {ex.Message}");
            return -1;
        }

        try
        {
            baseline = ApiModel.LoadFromFile(args.BaselinePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to load baseline report '{args.BaselinePath}': {ex.Message}");
            return -1;
        }

        baseline.EvaluateDelta(current);

        var result = current.ToString();
        var hasChanges = current.Types.Count > 0;

        if (args.Output == null)
        {
            Console.Write(result);
        }
        else
        {
            try
            {
                await File.WriteAllTextAsync(args.Output, result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to write output delta report '{args.Output}': {ex.Message}");
                return -1;
            }
        }

        return hasChanges ? 0 : NoChangesExitCode;
    }

    private static ApiModel LoadCurrentModel(string path)
        => Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)
            ? ApiModel.LoadFromFile(path)
            : ApiModel.LoadFromAssembly(path);
}
