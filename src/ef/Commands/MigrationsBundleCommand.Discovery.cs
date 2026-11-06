// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET
using System.Text.Json;
#endif
using System.Text;
using Microsoft.EntityFrameworkCore.Design;
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

    // Routes one line of the discovery child's output as it arrives. The child runs with
    // --prefix-output, so everything ef.dll itself reports (error/warn/info/verbose, and the data:
    // lines carrying the result) is tagged, and all of it is written to stdout. Data payloads are
    // collected for ParseContextType; tagged diagnostics go to the matching report method, so the
    // user sees actionable guidance (e.g. the multi-context "--context" hint) immediately, without
    // waiting for the child to exit. Untagged lines are output from the user's application or the
    // runtime host (an unhandled exception, a framework roll-forward failure, startup noise): they are
    // collected in 'other' so the caller can show them as errors if the child fails, or as verbose
    // output if it succeeds. Callbacks for stdout and stderr arrive on different threads, so the
    // collections are guarded by a lock on 'data'.
    // See https://github.com/dotnet/efcore/issues/25555.
    internal static void HandleDiscoveryOutput(
        string? line,
        StringBuilder data,
        List<string> other,
        Action<string> onError,
        Action<string> onWarning,
        Action<string> onInformation,
        Action<string> onVerbose)
    {
        line = line?.TrimEnd('\r');
        if (line == null || line.Length == 0)
        {
            return;
        }

        if (line.StartsWith(Reporter.DataPrefix, StringComparison.Ordinal))
        {
            lock (data)
            {
                data.Append(line, Reporter.DataPrefix.Length, line.Length - Reporter.DataPrefix.Length);
            }
        }
        else if (line.StartsWith(Reporter.ErrorPrefix, StringComparison.Ordinal))
        {
            onError(line.Substring(Reporter.ErrorPrefix.Length));
        }
        else if (line.StartsWith(Reporter.WarningPrefix, StringComparison.Ordinal))
        {
            onWarning(line.Substring(Reporter.WarningPrefix.Length));
        }
        else if (line.StartsWith(Reporter.InfoPrefix, StringComparison.Ordinal))
        {
            onInformation(line.Substring(Reporter.InfoPrefix.Length));
        }
        else if (line.StartsWith(Reporter.VerbosePrefix, StringComparison.Ordinal))
        {
            onVerbose(line.Substring(Reporter.VerbosePrefix.Length));
        }
        else
        {
            lock (data)
            {
                other.Add(line);
            }
        }
    }

    private string DiscoverContextType(IReadOnlyList<string> applicationArgs)
        => DiscoverContextType(
            applicationArgs,
            (args, handleLine) => Exe.Run("dotnet", args, handleOutput: handleLine, handleError: handleLine),
            Reporter.WriteError,
            Reporter.WriteWarning,
            Reporter.WriteInformation,
            Reporter.WriteVerbose);

    // 'run' launches the child with the given arguments, passing each stdout/stderr line to the supplied
    // callback, and returns its exit code. Split out from the Exe.Run call so it can be tested.
    internal string DiscoverContextType(
        IReadOnlyList<string> applicationArgs,
        Func<IReadOnlyList<string>, Action<string?>, int> run,
        Action<string> onError,
        Action<string> onWarning,
        Action<string> onInformation,
        Action<string> onVerbose)
    {
#if NET
        var efAssemblyPath = typeof(Program).Assembly.Location;
        var discoveryArgs = BuildDiscoveryArguments(
            efAssemblyPath,
            _depsFile!.Value(),
            _runtimeConfig!.Value(),
            _fxVersion!.Value(),
            _additionalProbingPaths!.Values.Where(v => v != null).Cast<string>().ToList(),
            GetCommonProjectArgs(),
            Context!.Value(),
            Reporter.IsVerbose,
            applicationArgs);

        var data = new StringBuilder();
        var other = new List<string>();

        var exitCode = run(
            discoveryArgs,
            line => HandleDiscoveryOutput(line, data, other, onError, onWarning, onInformation, onVerbose));

        if (exitCode != 0)
        {
            // The tagged diagnostics have already been reported. Untagged output is most likely the
            // reason for a failure the child could not report itself (unhandled exception, missing
            // framework), so show it now.
            foreach (var line in other)
            {
                onError(line);
            }

            throw new CommandException(Resources.BundleContextDiscoveryFailed);
        }

        foreach (var line in other)
        {
            onVerbose(line);
        }

        return ParseContextType(data.ToString());
#else
        // The .NET Framework build of ef.dll is a build-only reference (see ef.csproj) and is never
        // executed by dotnet-ef, which always runs tools/net/ef.dll. Bundling is a .NET-only path.
        throw new NotSupportedException();
#endif
    }

    // Extracts the "type" field from the JSON that `dbcontext info --json` emitted, as collected from
    // the child's data: lines by HandleDiscoveryOutput. See https://github.com/dotnet/efcore/issues/25555.
    internal static string ParseContextType(string json)
    {
#if NET
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
