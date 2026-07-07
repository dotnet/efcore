// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET
using System.Text.Json;
#endif
using System.Text;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsBundleCommand
{
    // Builds the `dotnet exec <host args> ef.dll dbcontext info --json <tool args>` command line for
    // out-of-process DbContext discovery. The host args (consumed by the runtime host) precede ef.dll;
    // the tool args follow it. See https://github.com/dotnet/efcore/issues/25555.
    internal static IReadOnlyList<string> BuildDiscoveryArguments(
        string efAssemblyPath,
        string? depsFile,
        string? runtimeConfig,
        string? fxVersion,
        IReadOnlyList<string> additionalProbingPaths,
        IReadOnlyList<string> commonProjectArgs,
        string? context,
        bool verbose,
        IReadOnlyList<string> applicationArgs)
    {
        var args = new List<string> { "exec" };

        if (!string.IsNullOrEmpty(depsFile))
        {
            args.Add("--depsfile");
            args.Add(depsFile!);
        }

        foreach (var path in additionalProbingPaths)
        {
            args.Add("--additionalprobingpath");
            args.Add(path);
        }

        // Mirror how dotnet-ef launches the parent ef.dll (RootCommand): prefer the startup project's
        // runtimeconfig.json, but when it has none, pin the shared framework with --fx-version so the
        // child rolls forward to the same framework the app targets rather than ef.dll's own.
        if (!string.IsNullOrEmpty(runtimeConfig))
        {
            args.Add("--runtimeconfig");
            args.Add(runtimeConfig!);
        }
        else if (!string.IsNullOrEmpty(fxVersion))
        {
            args.Add("--fx-version");
            args.Add(fxVersion!);
        }

        args.Add(efAssemblyPath);
        args.Add("dbcontext");
        args.Add("info");
        args.Add("--json");

        // The child invokes the user's application entry point to build design-time services, so the
        // app may write to stdout. --prefix-output tags ef.dll's own output so we can isolate the JSON
        // data lines from arbitrary app output; --no-color keeps those lines plain.
        args.Add("--prefix-output");
        args.Add("--no-color");

        args.AddRange(commonProjectArgs);

        if (!string.IsNullOrEmpty(context))
        {
            args.Add("--context");
            args.Add(context!);
        }

        if (verbose)
        {
            args.Add("--verbose");
        }

        // Forward the application arguments (everything after "--") so the child's design-time
        // discovery sees the same args the in-process path did — e.g. an IDesignTimeDbContextFactory
        // or CreateHostBuilder that selects the context/connection from them. See
        // https://github.com/dotnet/efcore/issues/25555.
        if (applicationArgs.Count > 0)
        {
            args.Add("--");
            args.AddRange(applicationArgs);
        }

        return args;
    }

    // Builds the failure for a non-zero discovery child. Under --prefix-output the child writes its
    // prefixed error lines to STDOUT (Reporter.WriteError -> WriteStdErr -> WriteLine -> stdout), so
    // the actionable text is in the captured stdout, not stderr; scan stdout first, then stderr for
    // prefixed lines, and finally fall back to the raw stderr capture for unprefixed failures.
    // See https://github.com/dotnet/efcore/issues/25555.
    internal static CommandException CreateDiscoveryFailure(string output, IEnumerable<string?> errorOutput)
    {
        var errorLines = errorOutput as IReadOnlyCollection<string?> ?? errorOutput.ToList();

        // Prefer prefixed error lines (actionable EF guidance like the multi-context "--context" hint):
        // scan the child's stdout first (where --prefix-output routes its error: lines), then its stderr.
        // As a last resort fall back to the raw stderr text, so host-level failures that carry no
        // Reporter.ErrorPrefix (framework roll-forward, missing runtime, an unhandled exception) still
        // reach the user instead of collapsing into the generic message.
        var detail = ExtractErrorMessage(output.Split('\n').Select(l => l.TrimEnd('\r')))
            ?? ExtractErrorMessage(errorLines)
            ?? JoinRawErrorLines(errorLines);

        return new CommandException(
            detail != null
                ? Resources.BundleContextDiscoveryFailedWithError(detail)
                : Resources.BundleContextDiscoveryFailed);
    }

    // Joins the raw captured stderr lines verbatim, used as a last-resort fallback when the child
    // produced no Reporter.ErrorPrefix-prefixed error text.
    internal static string? JoinRawErrorLines(IEnumerable<string?> errorLines)
    {
        var builder = new StringBuilder();
        foreach (var line in errorLines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                builder.Append(line);
            }
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }

    // Pulls the human-readable error text out of prefixed output lines (those beginning with
    // Reporter.ErrorPrefix), so actionable discovery errors (e.g. the multi-context "--context"
    // guidance) are surfaced instead of collapsed into a generic failure.
    internal static string? ExtractErrorMessage(IEnumerable<string?> errorLines)
    {
        var builder = new StringBuilder();
        foreach (var line in errorLines)
        {
            if (line != null
                && line.StartsWith(Reporter.ErrorPrefix, StringComparison.Ordinal))
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                builder.Append(line.Substring(Reporter.ErrorPrefix.Length));
            }
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }

    private string DiscoverContextType(IReadOnlyList<string> applicationArgs)
    {
#if NET
        var efAssemblyPath = typeof(Program).Assembly.Location;
        var discoveryArgs = BuildDiscoveryArguments(
            efAssemblyPath,
            DepsFile!.Value(),
            RuntimeConfig!.Value(),
            FxVersion!.Value(),
            AdditionalProbingPaths!.Values.Where(v => v != null).Cast<string>().ToList(),
            GetCommonProjectArgs(),
            Context!.Value(),
            Reporter.IsVerbose,
            applicationArgs);

        var output = new StringBuilder();
        var errorOutput = new List<string?>();
        var exitCode = Exe.Run(
            "dotnet",
            discoveryArgs,
            handleOutput: line =>
            {
                if (!string.IsNullOrEmpty(line))
                {
                    output.AppendLine(line);
                }
            },
            handleError: line =>
            {
                if (!string.IsNullOrEmpty(line))
                {
                    errorOutput.Add(line);
                    Reporter.WriteVerbose(line);
                }
            });

        if (exitCode != 0)
        {
            throw CreateDiscoveryFailure(output.ToString(), errorOutput);
        }

        return ParseContextType(output.ToString());
#else
        // The .NET Framework build of ef.dll is a build-only reference (see ef.csproj) and is never
        // executed by dotnet-ef, which always runs tools/net/ef.dll. Bundling is a .NET-only path.
        throw new NotSupportedException();
#endif
    }

    // Extracts the "type" field from the JSON emitted by `dbcontext info --json`. The child runs with
    // --prefix-output, so its structured data lines start with Reporter.DataPrefix; only those lines
    // are considered, so arbitrary stdout the user's app prints during discovery cannot corrupt the
    // JSON. See https://github.com/dotnet/efcore/issues/25555.
    internal static string ParseContextType(string output)
    {
#if NET
        var builder = new StringBuilder();
        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (line.StartsWith(Reporter.DataPrefix, StringComparison.Ordinal))
            {
                builder.Append(line.Substring(Reporter.DataPrefix.Length));
            }
        }

        var json = builder.ToString();
        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            try
            {
                using var document = JsonDocument.Parse(json.Substring(start, end - start + 1));
                if (document.RootElement.TryGetProperty("type", out var type)
                    && type.ValueKind == JsonValueKind.String
                    && type.GetString() is { Length: > 0 } value)
                {
                    return value;
                }
            }
            catch (JsonException)
            {
            }
        }

        throw new CommandException(Resources.BundleContextDiscoveryFailed);
#else
        throw new NotSupportedException();
#endif
    }
}
