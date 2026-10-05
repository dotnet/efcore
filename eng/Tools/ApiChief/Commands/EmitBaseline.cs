// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using ApiChief.Model;

namespace ApiChief.Commands;

internal static class EmitBaseline
{
    private sealed class EmitBaselineArgs
    {
        public FileInfo? AssemblyPath { get; set; }
        public string? Output { get; set; }
    }

    public static Command Create(Argument<FileInfo> assemblyPathArgument)
    {
        var outputOption = new Option<string?>("--output", ["-o"])
        {
            Description = "Path of the baseline file to produce"
        };

        var cmd = new Command("baseline", "Creates an API baseline")
        {
            outputOption,
        };

        cmd.SetAction(parseResult => ExecuteAsync(new EmitBaselineArgs
        {
            AssemblyPath = parseResult.GetValue(assemblyPathArgument),
            Output = parseResult.GetValue(outputOption),
        }));

        return cmd;
    }
    
    private static async Task<int> ExecuteAsync(EmitBaselineArgs args)
    {
        ApiModel model;
        try
        {
            model = ApiModel.LoadFromAssembly(args.AssemblyPath!.FullName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to decompile assembly '{args.AssemblyPath!.FullName}': {ex.Message}");
            return -1;
        }

        var result = model.ToString();

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
                Console.Error.WriteLine($"Unable to write output baseline report '{args.Output}': {ex.Message}");
                return -1;
            }
        }

        return 0;
    }
}
