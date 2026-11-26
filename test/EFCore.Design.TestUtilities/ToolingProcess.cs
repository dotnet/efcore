// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class ToolingProcess
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public static async Task<ToolingProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        string logPath,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        var command = startInfo.FileName + " " + string.Join(" ", startInfo.ArgumentList.Select(a => $"\"{a}\""));
        var environment = new[]
        {
            "DOTNET_ROOT", "DOTNET_CLI_HOME", "NUGET_PACKAGES", "EF_TEST_DATABASE", "EF_TEST_FACTORY_OUTPUT"
        };
        await File.WriteAllTextAsync(
            logPath + ".command.json",
            JsonSerializer.Serialize(
                new
                {
                    executable = startInfo.FileName,
                    arguments = startInfo.ArgumentList.ToArray(),
                    workingDirectory = startInfo.WorkingDirectory,
                    environment = environment.Where(startInfo.Environment.ContainsKey)
                        .ToDictionary(name => name, name => startInfo.Environment[name])
                },
                _jsonOptions),
            cancellationToken).ConfigureAwait(false);

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(linkedSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException) when (process.HasExited)
            {
            }

            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            await SaveOutputAsync().ConfigureAwait(false);

            var message = $"Command did not complete: {command}{Environment.NewLine}Logs: {logPath}.*";
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(message, exception, cancellationToken);
            }

            throw new TimeoutException(message, exception);
        }

        await SaveOutputAsync().ConfigureAwait(false);
        return new ToolingProcessResult(
            process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false), command, logPath);

        async Task SaveOutputAsync()
        {
            await File.WriteAllTextAsync(logPath + ".stdout.log", await output.ConfigureAwait(false), Encoding.UTF8)
                .ConfigureAwait(false);
            await File.WriteAllTextAsync(logPath + ".stderr.log", await error.ConfigureAwait(false), Encoding.UTF8)
                .ConfigureAwait(false);
        }
    }
}

public sealed record ToolingProcessResult(int ExitCode, string StandardOutput, string StandardError, string Command, string LogPath)
{
    public void EnsureSuccess()
    {
        if (ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Command exited with {ExitCode}: {Command}{Environment.NewLine}"
                + $"{Tail(StandardOutput)}{Environment.NewLine}{Tail(StandardError)}{Environment.NewLine}Logs: {LogPath}.*");
        }
    }

    public static string Tail(string output)
        => output.Length <= 8000 ? output : output[^8000..];
}
