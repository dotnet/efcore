// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public class TestCommand
    {
        protected string _command;
        private readonly ITestOutputHelper _output;

        public string WorkingDirectory { get; set; }

        public Dictionary<string, string> Environment { get; } = new Dictionary<string, string>();

        public TestCommand(string command, ITestOutputHelper output)
        {
            _command = command;
            _output = output;
        }

        private void Log(string message) => _output?.WriteLine(message);

        public virtual CommandResult Execute(string args = "")
        {
            var commandPath = _command;
            ResolveCommand(ref commandPath, ref args);

            Log($"Executing - {commandPath} {args}");

            var stdOut = new StreamForwarder();
            var stdErr = new StreamForwarder();

            stdOut.ForwardTo(Log);
            stdErr.ForwardTo(Log);

            return RunProcess(commandPath, args, stdOut, stdErr);
        }

        public virtual CommandResult ExecuteWithCapturedOutput(string args = "")
        {
            var command = _command;
            ResolveCommand(ref command, ref args);
            var commandPath = Env.GetCommandPath(command, ".exe", ".cmd", "") ??
                Env.GetCommandPathFromRootPath(AppContext.BaseDirectory, command, ".exe", ".cmd", "");

            Log($"Executing (Captured Output) - {commandPath} {args}");

            var stdOut = new StreamForwarder();
            var stdErr = new StreamForwarder();

            stdOut.Capture();
            stdErr.Capture();

            return RunProcess(commandPath, args, stdOut, stdErr);
        }

        private void ResolveCommand(ref string executable, ref string args)
        {
            if (executable.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var newArgs = ArgumentEscaper.EscapeSingleArg(executable);
                if (!string.IsNullOrEmpty(args))
                {
                    newArgs += " " + args;
                }
                args = newArgs;
                executable = "dotnet";
            }

            if (!Path.IsPathRooted(executable))
            {
                executable = Env.GetCommandPath(executable) ??
                           Env.GetCommandPathFromRootPath(AppContext.BaseDirectory, executable);
            }
        }

        private CommandResult RunProcess(string executable, string args, StreamForwarder stdOut, StreamForwarder stdErr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            foreach (var item in Environment)
            {
                psi.Environment[item.Key] = item.Value;
            }

            if (!string.IsNullOrWhiteSpace(WorkingDirectory))
            {
                psi.WorkingDirectory = WorkingDirectory;
                Log($"Working directory: {WorkingDirectory}");
            }

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            using (process)
            {
                process.Start();

                var threadOut = stdOut.BeginRead(process.StandardOutput);
                var threadErr = stdErr.BeginRead(process.StandardError);

                process.WaitForExit();
                Task.WaitAll(threadOut, threadErr);

                var result = new CommandResult(
                    process.StartInfo,
                    process.ExitCode,
                    stdOut.CapturedOutput,
                    stdErr.CapturedOutput);

                return result;
            }
        }
    }
}
