// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using ApiChief.Commands;

namespace ApiChief;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        var assemblyPathArgument = new Argument<FileInfo>("assembly-path")
        {
            Description = "Path to the assembly or baseline file to work with."
        };

        var rootCommand = new RootCommand("Helps with .NET API management activities")
        {
            assemblyPathArgument,

            new Command("emit", "Emits a file resulting from processing the assembly")
            {
                EmitBaseline.Create(assemblyPathArgument),
                EmitDelta.Create(assemblyPathArgument),
                EmitSummary.Create(assemblyPathArgument),
                EmitReview.Create(assemblyPathArgument),
            },

            new Command("check", "Performs checks on the assembly")
            {
                CheckBreakingChanges.Create(assemblyPathArgument),
            }
        };

        return rootCommand.Parse(args).InvokeAsync();
    }
}
