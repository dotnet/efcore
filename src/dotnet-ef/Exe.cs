// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Exe
{
    public static int Run(
        string executable,
        IReadOnlyList<string> args,
        string? workingDirectory = null,
        Action<string?>? handleOutput = null,
        Action<string?>? handleError = null,
        Action<string>? processCommandLine = null)
    {
        var arguments = ToArguments(args);

        processCommandLine ??= Reporter.WriteVerbose;
        processCommandLine(executable + " " + arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = handleOutput != null,
            RedirectStandardError = handleError != null
        };
        if (workingDirectory != null)
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        var process = new Process
        {
            StartInfo = startInfo
        };

        if (handleOutput != null)
        {
            process.OutputDataReceived += (sender, args) => handleOutput(args.Data);
        }

        if (handleError != null)
        {
            process.ErrorDataReceived += (sender, args) => handleError(args.Data);
        }

        process.Start();

        if (handleOutput != null)
        {
            process.BeginOutputReadLine();
        }

        if (handleError != null)
        {
            process.BeginErrorReadLine();
        }

        process.WaitForExit();

        return process.ExitCode;
    }

    private static string ToArguments(IReadOnlyList<string> args)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < args.Count; i++)
        {
            if (i != 0)
            {
                builder.Append(' ');
            }

            if (args[i].Length == 0)
            {
                builder.Append("\"\"");

                continue;
            }

            if (args[i].IndexOf(' ') == -1)
            {
                builder.Append(args[i]);

                continue;
            }

            builder.Append('"');

            var pendingBackslashes = 0;
            for (var j = 0; j < args[i].Length; j++)
            {
                switch (args[i][j])
                {
                    case '\"':
                        if (pendingBackslashes != 0)
                        {
                            builder.Append('\\', pendingBackslashes * 2);
                            pendingBackslashes = 0;
                        }

                        builder.Append("\\\"");
                        break;

                    case '\\':
                        pendingBackslashes++;
                        break;

                    default:
                        if (pendingBackslashes != 0)
                        {
                            if (pendingBackslashes == 1)
                            {
                                builder.Append('\\');
                            }
                            else
                            {
                                builder.Append('\\', pendingBackslashes * 2);
                            }

                            pendingBackslashes = 0;
                        }

                        builder.Append(args[i][j]);
                        break;
                }
            }

            if (pendingBackslashes != 0)
            {
                builder.Append('\\', pendingBackslashes * 2);
            }

            builder.Append('"');
        }

        return builder.ToString();
    }
}
