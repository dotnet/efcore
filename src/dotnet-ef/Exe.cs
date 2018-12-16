// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal static class Exe
    {
        public static int Run(
            string executable,
            IReadOnlyList<string> args,
            string workingDirectory = null,
            bool interceptOutput = false)
        {
            var arguments = ToArguments(args);

            Reporter.WriteVerbose(executable + " " + arguments);

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = interceptOutput
            };
            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            var process = Process.Start(startInfo);

            if (interceptOutput)
            {
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    Reporter.WriteVerbose(line);
                }
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
                    builder.Append(" ");
                }

                if (args[i].IndexOf(' ') == -1)
                {
                    builder.Append(args[i]);

                    continue;
                }

                builder.Append("\"");

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
                                    builder.Append("\\");
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

                builder.Append("\"");
            }

            return builder.ToString();
        }
    }
}
