// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DatabaseDropCommand : ICommand
    {
        public static void Configure([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions commonOptions)
        {
            command.Description = "Drop the database for specific environment";
            command.HelpOption();

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var force = command.Option(
                "-f|--force",
                "Drop without confirmation");
            var dryRun = command.Option(
                "--dry-run",
                "Do not actually drop the database");
            var json = command.JsonOption();

            command.OnExecute(() =>
                {
                    commonOptions.Command = new DatabaseDropCommand(
                        context.Value(),
                        force.HasValue(),
                        dryRun.HasValue(),
                        json.HasValue());
                });
        }

        private readonly bool _force;
        private readonly string _context;
        private readonly bool _dryRun;
        private readonly bool _json;

        public DatabaseDropCommand(string context, bool force, bool dryRun, bool json)
        {
            _context = context;
            _force = force;
            _dryRun = dryRun;
            _json = json;
        }

        public void Run(IOperationExecutor executor)
        {
            if (!_force)
            {
                var result = executor.GetContextInfo(_context);
                var reporter = _json
                    ? (Action<IDictionary>)ReportJsonDatabaseDiscovered
                    : ReportDatabaseDiscovered;

                reporter.Invoke(result);
                if (result == null)
                {
                    return;
                }
            }

            if (_dryRun)
            {
                return;
            }

            if (!_force)
            {
                Reporter.Output("Are you sure you want to proceed? (y/N)".MaybeColor(s => s.Bold()));
                var readedKey = Console.ReadLine().Trim();
                var confirmed = (readedKey == "y") || (readedKey == "Y");
                if (!confirmed)
                {
                    Reporter.Output("Cancelled");
                    return;
                }
            }

            executor.DropDatabase(_context);
        }

        private static void ReportDatabaseDiscovered(IDictionary result)
        {
            if (result == null)
            {
                Reporter.Output("Could not find database to drop");
                return;
            }

            Reporter.Output("This command will " + "permanently".MaybeColor(s => s.Bold()) + " drop the database:");
            Reporter.Output("");
            Reporter.Output($"    {"Database name".MaybeColor(s => s.Bold().Green())} : {result["DatabaseName"]}");
            Reporter.Output($"    {"Data source  ".MaybeColor(s => s.Bold().Green())} : {result["DataSource"]}");
            Reporter.Output("");
        }

        private static void ReportJsonDatabaseDiscovered(IDictionary result)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Reporter.JsonPrefix);

            if (result == null)
            {
                sb.AppendLine("{ }");
            }
            else
            {
                sb
                    .AppendLine("{")
                    .AppendLine("    \"databaseName\": " + SerializeString(result["DatabaseName"] as string) + ",")
                    .AppendLine("    \"dataSource\": " + SerializeString(result["DataSource"] as string))
                    .AppendLine("}");
            }

            sb.AppendLine(Reporter.JsonSuffix);

            Reporter.Output(sb.ToString());
        }

        private static readonly IDictionary<char, string> s_replaces
            = new Dictionary<char, string>
            {
                { '\n', @"\n" },
                { '\t', @"\t" },
                { '\r', @"\r" },
                { '\f', @"\f" },
                { '\b', @"\b" },
                { '"', @"\""" },
                { '\\', @"\\" }
            };

        private const string NullString = "null";

        private static string SerializeString(string raw)
        {
            if (raw == null)
            {
                return NullString;
            }

            var sb = new StringBuilder(raw.Length + 2);
            sb.Append('"');
            foreach (var c in raw)
            {
                if (s_replaces.ContainsKey(c))
                {
                    sb.Append(s_replaces[c]);
                }
                else
                {
                    sb.Append(c);
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
