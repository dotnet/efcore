// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using ApiChief.Format;
using ApiChief.Processing;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;

namespace ApiChief.Commands;

internal static class EmitSummary
{

    private sealed class EmitSummaryArgs
    {
        public FileInfo? AssemblyPath { get; set; }
        public string? Output { get; set; }
        public bool OmitXmlComments { get; set; }
    }

    public static Command Create(Argument<FileInfo> assemblyPathArgument)
    {
        var outputOption = new Option<string?>("--output", ["-o"])
        {
            Description = "Path of the summary file to produce"
        };

        var omitXmlCommentsOption = new Option<bool>("--omit-xml-comments", ["-x"])
        {
            Description = "Omit the XML documentation comments"
        };

        var command = new Command("summary", "Creates an API summary")
        {
            outputOption,
            omitXmlCommentsOption,
        };

        command.SetAction(parseResult => ExecuteAsync(new EmitSummaryArgs
        {
            AssemblyPath = parseResult.GetValue(assemblyPathArgument),
            Output = parseResult.GetValue(outputOption),
            OmitXmlComments = parseResult.GetValue(omitXmlCommentsOption),
        }));

        return command;
    }


    private static async Task<int> ExecuteAsync(EmitSummaryArgs args)
    {
        var formatting = args.OmitXmlComments ? Formatter.BaselineFormatting : Formatter.FormattingWithXmlComments;
        CSharpDecompiler decompiler;

        try
        {
            var path = args.AssemblyPath!.FullName;

            decompiler = args.OmitXmlComments
                ? DecompilerFactory.Create(path)
                : DecompilerFactory.CreateWithXmlComments(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to decompile assembly '{args.AssemblyPath!.FullName}': {ex.Message}");
            return -1;
        }

        var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile(true);

        // remove private stuff from the tree
        syntaxTree.AcceptVisitor(new PublicFilterVisitor());

        using var writer = new StringWriter();
        var visitor = new CSharpOutputVisitor(writer, formatting);
        syntaxTree.AcceptVisitor(visitor);
        var result = writer.ToString();

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
                Console.Error.WriteLine($"Unable to write output summary file '{args.Output}': {ex.Message}");
                return -1;
            }
        }

        return 0;
    }
}
