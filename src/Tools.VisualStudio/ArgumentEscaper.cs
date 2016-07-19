// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Tools.VisualStudio
{
    public class ArgumentEscaper
    {
        public static string Escape(IEnumerable<string> args)
        {
            if (args == null)
            {
                return null;
            }

            return string.Join(" ", args.Where(a => a != null).Select(EscapeArg));
        }

        private static string EscapeArg(string arg)
        {
            var sb = new StringBuilder();
            var needsQuotes = NeedsQuotation(arg);
            var isQuoted = needsQuotes || IsQuoted(arg);
            if (needsQuotes)
            {
                sb.Append("\"");
            }

            for (var i = 0; i < arg.Length; i++)
            {
                var backslashes = 0;
                while (i < arg.Length
                       && arg[i] == '\\')
                {
                    i++;
                    backslashes++;
                }

                if (i == arg.Length && isQuoted)
                {
                    // if quoted
                    sb.Append('\\', backslashes * 2);
                }
                else if (i == arg.Length)
                {
                    // if not quoted, no need to escape backslash
                    sb.Append('\\', backslashes);
                }
                else if (arg[i] == '"')
                {
                    sb.Append('\\', backslashes * 2 + 1)
                        .Append('"');
                }
                else
                {
                    sb.Append('\\', backslashes);
                    sb.Append(arg[i]);
                }
            }

            if (needsQuotes)
            {
                sb.Append("\"");
            }
            return sb.ToString();
        }

        private static bool IsQuoted(string arg)
            => arg.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && arg.EndsWith("\"", StringComparison.OrdinalIgnoreCase);

        private static bool NeedsQuotation(string arg)
        {
            if (IsQuoted(arg))
            {
                return false;
            }

            return arg.Any(c => c == ' ' || c == '\t' || c == '\n');
        }
    }
}
