// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Tools.VisualStudio
{
    /// <summary>
    ///     <para>
    ///         Invokes a new process and pipes output through to the PowerShell host.
    ///         By default, this merges stdout and stderr into the 'Output' stream.
    ///         (By comparison, Invoke-Expression and the call operator redirect stderr into the Error stream).
    ///     </para>
    ///     <para>
    ///         When -RedirectByPrefix is specified, each line of stdout/stderr can be redirected to 'Debug', 'Verbose', etc. based on the prefix.
    ///         <cref see="RedirectByPrefix" />.
    ///         When -JsonOutput is specified, the cmdlet will only output JSON objects found in stdout/stderr that have been wrapped by '//BEGIN'
    ///         and '//END' <cref see="JsonOutput" />
    ///     </para>
    /// </summary>
    [Cmdlet("Invoke", "Process")]
    [OutputType(typeof(string))]
    public class InvokeProcessCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Executable { get; set; }

        [Parameter]
        public string[] Arguments { get; set; }

        [Parameter]
        public SwitchParameter RedirectByPrefix { get; set; }

        [Parameter]
        public SwitchParameter JsonOutput { get; set; }

        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        protected override void BeginProcessing()
        {
            WriteDebug("JsonOutput: " + JsonOutput.IsPresent);
            WriteDebug("RedirectByPrefix: " + JsonOutput.IsPresent);
        }

        protected override void ProcessRecord()
        {
            // TODO handle stdin
            try
            {
                if (!Path.IsPathRooted(Executable))
                {
                    Executable = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Executable);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = Executable,
                    Arguments = ArgumentEscaper.Escape(Arguments),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                WriteDebug($"Executing process: [{startInfo.FileName}]");
                WriteDebug($"Arguments: [{startInfo.Arguments}]");
                var process = new Process
                {
                    StartInfo = startInfo
                };
                process.Start();
                Forward(process.StandardOutput);
                Forward(process.StandardError);

                string line;
                while (_reading > 0
                       || _queue.Count > 0)
                {
                    if (_queue.TryDequeue(out line))
                    {
                        Write(line);
                    }
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    WriteError(new ErrorRecord(
                        new AggregateException("Process finished with non-zero exit code"),
                        $"[{Executable}] finished with exit code {process.ExitCode}",
                        ErrorCategory.CloseError,
                        Executable
                        ));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    ex.Message,
                    ErrorCategory.NotSpecified,
                    nameof(InvokeProcessCommand)
                    ));
            }
        }

        private const string JsonBeginPrefix = "//BEGIN";
        private const string JsonEndPrefix = "//END";
        private readonly List<string> _jsonOutputLines = new List<string>();
        private bool _collecting;

        private void WriteOutput(string line)
        {
            if (!JsonOutput.IsPresent)
            {
                WriteObject(line);
                return;
            }

            if (line.Equals(JsonBeginPrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteDebug(line);
                _collecting = true;
            }
            else if (line.Equals(JsonEndPrefix, StringComparison.OrdinalIgnoreCase) && _collecting)
            {
                WriteDebug(line);

                var obj = string.Join(Environment.NewLine, _jsonOutputLines);
                WriteObject(obj);
                _jsonOutputLines.Clear();
                _collecting = false;
            }
            else if (_collecting)
            {
                WriteDebug(line);

                _jsonOutputLines.Add(line);
            }
            else
            {
                // When JsonOutput is specified, do not push anything but json objects in to the output pipeline
                Host.UI.WriteLine(line);
            }
        }

        private const string VerbosePrefix = "VERBOSE : ";
        private const string DebugPrefix = "DEBUG   : ";
        private const string WarningPrefix = "WARNING : ";
        private const string ErrorPrefix = "ERROR   : ";
        private const string OutputPrefix = "OUTPUT  : ";

        private void Write(string line)
        {
            if (!RedirectByPrefix.IsPresent)
            {
                WriteOutput(line);
                return;
            }

            if (line.StartsWith(VerbosePrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteVerbose(line.Substring(VerbosePrefix.Length));
                return;
            }

            if (line.StartsWith(DebugPrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteDebug(line.Substring(DebugPrefix.Length));
                return;
            }

            if (line.StartsWith(WarningPrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteWarning(line.Substring(WarningPrefix.Length));
                return;
            }

            if (line.StartsWith(ErrorPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var message = line.Substring(ErrorPrefix.Length);
                WriteError(new ErrorRecord(
                    new InvalidOperationException(message),
                    message,
                    ErrorCategory.FromStdErr,
                    null
                    ));
                return;
            }

            if (line.StartsWith(OutputPrefix, StringComparison.OrdinalIgnoreCase))
            {
                WriteOutput(line.Substring(OutputPrefix.Length));
                return;
            }

            WriteOutput(line);
        }

        private long _reading;

        private void Forward(StreamReader reader)
        {
            Interlocked.Increment(ref _reading);
            Task.Run(() =>
                {
                    try
                    {
                        var buffer = new char[1];
                        var sb = new StringBuilder();
                        var lastChar = '\0';
                        while (reader.Read(buffer, 0, 1) > 0)
                        {
                            if (buffer[0] == '\n'
                                || buffer[0] == '\r')
                            {
                                if (lastChar != '\r')
                                {
                                    _queue.Enqueue(sb.ToString());
                                    sb.Clear();
                                }
                            }
                            else
                            {
                                sb.Append(buffer[0]);
                            }
                            lastChar = buffer[0];
                        }
                        if (sb.Length > 0)
                        {
                            _queue.Enqueue(sb.ToString());
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _reading);
                    }
                });
        }
    }
}
