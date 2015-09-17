// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Builders;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    internal static class SqliteDmlParser
    {
        private static readonly ISet<string> _constraintKeyWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CONSTRAINT",
            "PRIMARY",
            "UNIQUE",
            "CHECK",
            "FOREIGN"
        };

        public static void ParseTableDefinition(ModelBuilder modelBuilder, string tableName, string sql)
        {
            var entityBuilder = modelBuilder.Entity(tableName);
            var statements = ParseStatements(sql).ToList();
            var i = 0;
            for (; i < statements.Count; i++)
            {
                var firstWord = statements[i].Split(' ', '(')[0];
                if (_constraintKeyWords.Contains(firstWord))
                {
                    break; // once we see the first constraint, stop looking for params
                }
                ParseColumnDefinition(entityBuilder, statements[i]);
            }
            for (; i < statements.Count; i++)
            {
                ParseConstraints(entityBuilder, statements[i]);
            }
        }

        // Extract all column definitions and constraints
        // Splits on commas, accounting for commas with parenthesis and quotes
        public static IEnumerable<string> ParseStatements(string sql)
        {
            char c;
            var start = 0;
            while ((c = sql[start++]) != '(')
            {
                if (c == '"')
                {
                    while (sql[start++] != '"')
                    {
                        ;
                    }
                }
                if (c == '\'')
                {
                    while (sql[start++] != '\'')
                    {
                        ;
                    }
                }
            }
            var statementsChunk = sql.Substring(start, sql.LastIndexOf(')') - start);

            return SafeSplit(statementsChunk, ',').Select(s => s.Trim());
        }

        public static void ParseColumnDefinition(EntityTypeBuilder entityBuilder, string statement)
        {
            var paramName = UnescapeString(SafeSplit(statement, ' ').First());
            var prop = entityBuilder.Metadata.FindProperty(paramName);
            if (prop == null)
            {
                return;
            }

            if (statement.IndexOf(" UNIQUE", StringComparison.OrdinalIgnoreCase) > 0)
            {
                entityBuilder.Index(prop.Name).Unique();
            }
        }

        public static void ParseConstraints(EntityTypeBuilder entityBuilder, string statement)
        {
            var constraint = statement.Split(' ', '(')[0];
            if (constraint.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                ParseInlineUniqueConstraint(entityBuilder, statement);
            }
        }

        public static void ParseInlineUniqueConstraint(EntityTypeBuilder entityBuilder, string statement)
        {
            var start = statement.IndexOf('(') + 1;
            var paramChunk = statement.Substring(start, statement.LastIndexOf(')') - start);
            var props = SafeSplit(paramChunk, ',')
                .Select(UnescapeString)
                .ToList();
            var index = entityBuilder.Metadata.Indexes.FirstOrDefault(i =>
                {
                    if (!i.Sqlite().Name.StartsWith("sqlite_autoindex"))
                    {
                        return false;
                    }
                    foreach (var prop in props)
                    {
                        if (!i.Properties.Any(p => p.Sqlite().ColumnName.Equals(prop, StringComparison.OrdinalIgnoreCase)))
                        {
                            return false;
                        }
                    }
                    return true;
                });
            if (index != null)
            {
                index.IsUnique = true;
            }
        }

        public static string UnescapeString(string identifier)
        {
            identifier = identifier.Trim();
            var firstChar = identifier[0];
            char quote;
            if (firstChar != identifier[identifier.Length - 1])
            {
                return identifier;
            }

            if (firstChar == '"')
            {
                quote = '"';
            }
            else if (firstChar == '\'')
            {
                quote = '\'';
            }
            else
            {
                return identifier;
            }

            return identifier.Substring(1, identifier.Length - 2).Replace($"{quote}{quote}", quote.ToString());
        }

        // For internal use. Only designed to split a valid SQLite statements.
        internal static IEnumerable<string> SafeSplit(string str, char separator)
        {
            // ReSharper disable EmptyEmbeddedStatement
            var idx = -1;
            var lastStart = 0;
            char c;
            while (++idx < str.Length)
            {
                c = str[idx];
                if (c == separator)
                {
                    if (idx > lastStart)
                    {
                        yield return str.Substring(lastStart, idx - lastStart).Trim(separator);
                    }
                    lastStart = idx + 1;
                }
                else if (c == '(')
                {
                    var nestedLevel = 1;
                    while (nestedLevel > 0
                           && ++idx < str.Length)
                    {
                        switch (str[idx])
                        {
                            case ')':
                                nestedLevel--;
                                break;
                            case '(':
                                nestedLevel++;
                                break;
                            case '"':
                                while (++idx < str.Length
                                       && str[idx] != '"')
                                {
                                    ;
                                }
                                break;
                            case '\'':
                                while (++idx < str.Length
                                       && str[idx] != '\'')
                                {
                                    ;
                                }
                                break;
                        }
                    }
                }
                else if (c == '"')
                {
                    while (++idx < str.Length
                           && str[idx] != '"')
                    {
                        ;
                    }
                }
                else if (c == '\'')
                {
                    while (++idx < str.Length
                           && str[idx] != '\'')
                    {
                        ;
                    }
                }
            }
            if (idx > lastStart)
            {
                yield return str.Substring(lastStart, idx - lastStart).Trim(separator);
            }
        }
    }
}
