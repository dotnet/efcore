// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiChief.Model;

namespace ApiChief.Commands;

internal static class EmitDelta
{
    private static readonly HashSet<char> InvalidFileNameChars = [..Path.GetInvalidFileNameChars(), ','];

    public const int NoChangesExitCode = 2;

    private sealed class EmitDeltaArgs
    {
        public FileInfo? AssemblyPath { get; set; }

        public string BaselinePath { get; set; } = string.Empty;

        public string? Output { get; set; }

        public bool Diff { get; set; }
    }

    public static Command Create(Argument<FileInfo> assemblyPathArgument)
    {
        var baselinePathArgument = new Argument<string>("baseline-path")
        {
            Description = "Path to the baseline report to use for reference"
        };

        var outputOption = new Option<string?>("--output", ["-o"])
        {
            Description = "Path of the delta file or diff directory to produce"
        };

        var diffOption = new Option<bool>("--diff")
        {
            Description = "Emit GitHub-friendly markdown diff files instead of JSON delta output"
        };

        var command = new Command("delta", "Creates an API delta")
        {
            baselinePathArgument,
            outputOption,
            diffOption,
        };

        command.SetAction(parseResult => ExecuteAsync(new EmitDeltaArgs
        {
            AssemblyPath = parseResult.GetValue(assemblyPathArgument),
            BaselinePath = parseResult.GetValue(baselinePathArgument) ?? string.Empty,
            Output = parseResult.GetValue(outputOption),
            Diff = parseResult.GetValue(diffOption),
        }));

        return command;
    }


    private static async Task<int> ExecuteAsync(EmitDeltaArgs args)
    {
        if (args.Diff)
        {
            return await ExecuteDiffAsync(args).ConfigureAwait(false);
        }

        var exitCode = TryCreateDeltaModel(args.AssemblyPath!.FullName, args.BaselinePath, out var current);
        if (exitCode != 0 && exitCode != NoChangesExitCode)
        {
            return exitCode;
        }

        var result = current!.ToString();
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

    private static async Task<int> ExecuteDiffAsync(EmitDeltaArgs args)
    {
        var exitCode = TryCreateDeltaModel(args.AssemblyPath!.FullName, args.BaselinePath, out var deltaModel);
        if (exitCode != 0)
        {
            return exitCode;
        }

        var output = args.Output;
        if (string.IsNullOrWhiteSpace(output))
        {
            var name = Path.GetFileNameWithoutExtension(args.AssemblyPath!.Name);
            output = $"API.{name}.Diff";
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in deltaModel!.Types.OrderBy(t => t.Type, StringComparer.Ordinal))
        {
            var file = GetOutputFileStem(type.Type);

            var tmp = file;
            if (names.Contains(tmp))
            {
                var count = 2;
                while (names.Contains(tmp))
                {
                    tmp = file + "_" + count++;
                }
            }

            names.Add(tmp);
            var fullFile = Path.Combine(output, tmp + ".md");

            try
            {
                Directory.CreateDirectory(output);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to create output directory '{output}': {ex.Message}");
                return -1;
            }

            try
            {
                await File.WriteAllTextAsync(fullFile, FormatDiffMarkdown(type)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to write output delta diff file '{fullFile}': {ex.Message}");
                return -1;
            }
        }

        return 0;
    }

    private static string FormatDiffMarkdown(ApiType type)
    {
        List<string> lines = [];

        var typeAdded = type.IsNew;
        var typeRemoved = type.IsRemoved;
        var stageTag = FormatStageTag(type.Stage);

        if (typeRemoved)
        {
            lines.Add($"- {stageTag}{type.Type}");
        }
        else if (typeAdded)
        {
            lines.Add($"+ {stageTag}{type.Type}");
        }
        else if (type.PreviousType != null && type.PreviousType != type.Type)
        {
            lines.Add($"- {stageTag}{type.PreviousType}");
            lines.Add($"+ {stageTag}{type.Type}");
        }
        else
        {
            lines.Add($"  {stageTag}{type.Type}");
        }

        AppendStageDiffLine(lines, type.Removals, '-');
        AppendStageDiffLine(lines, type.Additions, '+');
        AppendGroupedDiffMembers(lines, type);

        var wrapped = lines.SelectMany(WrapDiffLine);
        return $"```diff{Environment.NewLine}{string.Join(Environment.NewLine, wrapped)}{Environment.NewLine}```{Environment.NewLine}";
    }

    private const int MaxDiffLineLength = 160;

    private static IEnumerable<string> WrapDiffLine(string line)
    {
        if (line.Length <= MaxDiffLineLength)
        {
            yield return line;
            yield break;
        }

        var prefix = line.Length >= 2 ? line[..2] : "  ";
        var content = line.Length >= 2 ? line[2..] : line;
        const string continuationIndent = "    ";
        var firstMax = MaxDiffLineLength - prefix.Length;
        var contMax = MaxDiffLineLength - prefix.Length - continuationIndent.Length;

        var isFirst = true;
        while (content.Length > (isFirst ? firstMax : contMax))
        {
            var max = isFirst ? firstMax : contMax;
            var breakAt = FindWrapBreak(content, max);
            var chunk = content[..breakAt].TrimEnd();
            yield return isFirst ? prefix + chunk : prefix + continuationIndent + chunk;
            content = content[breakAt..].TrimStart();
            isFirst = false;
        }

        if (content.Length > 0)
        {
            yield return isFirst ? prefix + content : prefix + continuationIndent + content;
        }
    }

    private static int FindWrapBreak(string value, int maxLen)
    {
        if (value.Length <= maxLen)
        {
            return value.Length;
        }

        // Prefer breaking right after a comma to keep type lists readable.
        var commaIdx = value.LastIndexOf(',', maxLen - 1);
        if (commaIdx > 0 && commaIdx + 1 < value.Length && value[commaIdx + 1] == ' ')
        {
            return commaIdx + 2;
        }

        var spaceIdx = value.LastIndexOf(' ', maxLen - 1);
        if (spaceIdx > 0)
        {
            return spaceIdx + 1;
        }

        // No good break point — break at the limit so we still wrap.
        return maxLen;
    }

    private static string FormatStageTag(ApiStage stage)
        => stage != ApiStage.Stable ? $"[{stage}] " : string.Empty;

    private static void AppendStageDiffLine(List<string> lines, ApiType? changeSet, char prefix)
    {
        if (changeSet?.Stage != null && changeSet.Stage != ApiStage.Stable)
        {
            lines.Add($"{prefix} [Stage] {changeSet.Stage}");
        }
    }

    private static void AppendGroupedDiffMembers(List<string> lines, ApiType type)
    {
        var removedEntries = GetDiffEntries(type.Removals, '-');
        var addedEntries = GetDiffEntries(type.Additions, '+');
        var unchangedEntries = GetDiffEntries(type, ' ');

        var sharedNames = removedEntries
            .Select(static entry => entry.Name)
            .Intersect(addedEntries.Select(static entry => entry.Name), StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var name in sharedNames.OrderBy(static name => name, StringComparer.Ordinal))
        {
            foreach (var entry in removedEntries.Where(entry => entry.Name == name))
            {
                lines.Add(entry.Line);
            }

            foreach (var entry in addedEntries.Where(entry => entry.Name == name))
            {
                lines.Add(entry.Line);
            }
        }

        foreach (var entry in removedEntries.Where(entry => !sharedNames.Contains(entry.Name)))
        {
            lines.Add(entry.Line);
        }

        foreach (var entry in addedEntries.Where(entry => !sharedNames.Contains(entry.Name)))
        {
            lines.Add(entry.Line);
        }

        foreach (var entry in unchangedEntries)
        {
            lines.Add(entry.Line);
        }
    }

    private static List<(string Name, string Line)> GetDiffEntries(ApiType? changeSet, char prefix)
    {
        List<(string Name, string Line)> entries = [];

        if (changeSet == null)
        {
            return entries;
        }

        AddDiffMembers(entries, changeSet.Fields, prefix);
        AddDiffMembers(entries, changeSet.Properties, prefix);
        AddDiffMembers(entries, changeSet.Methods, prefix);

        return entries;
    }

    private static void AddDiffMembers(List<(string Name, string Line)> entries, ISet<ApiMember>? members, char prefix)
    {
        if (members == null)
        {
            return;
        }

        foreach (var member in members.OrderBy(m => GetMemberName(m.Member), StringComparer.Ordinal).ThenBy(m => m.Member, StringComparer.Ordinal))
        {
            var stageTag = FormatStageTag(member.Stage);
            entries.Add((GetMemberName(member.Member), $"{prefix} {stageTag}{member.Member}"));
        }
    }

    private static string GetMemberName(string declaration)
    {
        var headerEnd = declaration.IndexOf(" {", StringComparison.Ordinal);
        var parameterListStart = declaration.IndexOf('(');

        if (headerEnd < 0 || (parameterListStart >= 0 && parameterListStart < headerEnd))
        {
            headerEnd = parameterListStart;
        }

        if (headerEnd < 0)
        {
            headerEnd = declaration.Length;
        }

        var header = declaration[..headerEnd].Trim();
        var lastDot = header.LastIndexOf('.');
        if (lastDot >= 0)
        {
            return header[(lastDot + 1)..];
        }

        var lastSpace = header.LastIndexOf(' ');
        return lastSpace >= 0 ? header[(lastSpace + 1)..] : header;
    }

    private static string GetOutputFileStem(string typeDeclaration)
        => SanitizeFileName(StripTypeNameDecorations(typeDeclaration));

    private static string StripTypeNameDecorations(string typeDeclaration)
    {
        var typeName = RemoveBracketedSections(typeDeclaration).Trim();

        var removedPrefix = true;
        while (removedPrefix)
        {
            removedPrefix = false;

            foreach (var prefix in new[]
                     {
                         "abstract ", "sealed ", "static ", "readonly ", "ref ", "partial ",
                         "record ", "class ", "struct ", "interface ", "enum ", "delegate "
                     })
            {
                if (!typeName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                typeName = typeName[prefix.Length..].TrimStart();
                removedPrefix = true;
                break;
            }
        }

        var whereIndex = typeName.IndexOf(" where ", StringComparison.Ordinal);
        if (whereIndex >= 0)
        {
            typeName = typeName[..whereIndex];
        }

        var terminator = typeName.IndexOfAny([':', '(']);
        if (terminator >= 0)
        {
            typeName = typeName[..terminator];
        }

        return typeName.Trim();
    }

    private static string RemoveBracketedSections(string value)
    {
        var buffer = new char[value.Length];
        var index = 0;
        var depth = 0;

        foreach (var c in value)
        {
            switch (c)
            {
                case '[':
                    depth++;
                    continue;
                case ']' when depth > 0:
                    depth--;
                    continue;
                default:
                    if (depth == 0)
                    {
                        buffer[index++] = c;
                    }

                    break;
            }
        }

        return new string(buffer, 0, index);
    }

    private static string SanitizeFileName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return "_";
        }

        var buffer = new char[trimmed.Length];
        var current = 0;
        foreach (var c in trimmed)
        {
            if (c == ' ')
            {
                continue;
            }

            buffer[current++] = InvalidFileNameChars.Contains(c) ? '_' : c;
        }

        return  new string(buffer, 0, current);
    }

    private static int TryCreateDeltaModel(string currentPath, string baselinePath, out ApiModel? current)
    {
        ApiModel baseline;

        try
        {
            current = LoadCurrentModel(currentPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to load current API model from '{currentPath}': {ex.Message}");
            current = null;
            return -1;
        }

        try
        {
            baseline = ApiModel.LoadFromFile(baselinePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to load baseline report '{baselinePath}': {ex.Message}");
            current = null;
            return -1;
        }

        baseline.EvaluateDelta(current);

        return current.Types.Count > 0 ? 0 : NoChangesExitCode;
    }

    private static ApiModel LoadCurrentModel(string path)
        => Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)
            ? ApiModel.LoadFromFile(path)
            : ApiModel.LoadFromAssembly(path);
}
