// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    // Based on: System.Data.Jet.JetCommandParser (EntityFrameworkCore.Jet)
    public class XGQueryStringFactory : IRelationalQueryStringFactory
    {
        private static readonly Lazy<Regex> _limitExpressionParameterRegex = new Lazy<Regex>(
            () => new Regex(
                $@"(?<=\W)LIMIT\s+(?:(?<leading_offset>@?\w+),\s*)?(?<row_count>@?\w+)(?:\s*OFFSET\s*(?<trailing_offset>@?\w+))?",
                RegexOptions.Singleline | RegexOptions.IgnoreCase));

        private static readonly Lazy<Regex> _extractParameterRegex = new Lazy<Regex>(() => new Regex(@"@\w+"));

        private readonly IRelationalTypeMappingSource _typeMapper;

        public XGQueryStringFactory([NotNull] IRelationalTypeMappingSource typeMapper)
        {
            _typeMapper = typeMapper;
        }

        public virtual string Create(DbCommand command)
        {
            if (command.Parameters.Count == 0)
            {
                return command.CommandText;
            }

            // For parameter in LIMIT clauses and for string parameters, we need to inline the parameter values directly into the SQL.
            PrepareCommand(command);

            var builder = new StringBuilder();
            foreach (DbParameter parameter in command.Parameters)
            {
                builder
                    .Append("SET ")
                    .Append(parameter.ParameterName)
                    .Append(" = ")
                    .Append(GetParameterValue(parameter))
                    .AppendLine(";");
            }

            return builder
                .AppendLine()
                .Append(command.CommandText).ToString();
        }

        private string GetParameterValue(DbParameter parameter)
        {
            var typeMapping = _typeMapper.FindMapping(parameter.Value.GetType());

            return (parameter.Value == DBNull.Value
                    || parameter.Value == null)
                ? "NULL"
                : typeMapping != null
                    ? typeMapping.GenerateSqlLiteral(parameter.Value)
                    : parameter.Value.ToString();
        }

        protected virtual void PrepareCommand(DbCommand command)
        {
            // MySQL does not support user variables in LIMIT statements.
            // (It does however support parameters in LIMIT statements since 2010. See https://bugs.mysql.com/bug.php?id=11918)
            //
            // Because of that, we need to inline the parameter values as constants into the SQL command, in cases where they appear in a
            // LIMIT clause.
            //
            // Also, the rules for applying collation from user variables are different than the once for parameters.
            // We therefore inline all parameter values of type string as well.

            var stringParameterNames = command.Parameters.Cast<DbParameter>()
                .Where(p => p.Value is string)
                .Select(p => p.ParameterName)
                .ToList();

            if (!command.CommandText.Contains("LIMIT", StringComparison.OrdinalIgnoreCase) &&
                !stringParameterNames.Any())
            {
                return;
            }

            var matches = _limitExpressionParameterRegex.Value.Matches(command.CommandText);
            if (matches.Count <= 0 &&
                !stringParameterNames.Any())
            {
                return;
            }

            var limitGroupsWithParameter = matches.SelectMany(m => m.Groups["row_count"].Captures)
                .Concat(matches.SelectMany(m => m.Groups["leading_offset"].Captures))
                .Concat(matches.SelectMany(m => m.Groups["trailing_offset"].Captures))
                .ToList();

            if (!limitGroupsWithParameter.Any() &&
                !stringParameterNames.Any())
            {
                return;
            }

            var parser = new XGCommandParser(command.CommandText);
            var parameterPositions = parser.GetStateIndices('@');
            var parameters = parameterPositions
                .Select(
                    i => new
                    {
                        Index = i,
                        ParameterName = _extractParameterRegex.Value.Match(command.CommandText.Substring(i)).Value,
                    })
                .Where(t => !string.IsNullOrEmpty(t.ParameterName))
                .ToList();

            var validParameters = (limitGroupsWithParameter
                    .Where(c => parameterPositions.Contains(c.Index) &&
                                command.Parameters.Contains(c.Value))
                    .Select(c => new {Index = c.Index, ParameterName = c.Value}))
                .Concat(stringParameterNames.SelectMany(s => parameters.Where(p => p.ParameterName == s)))
                .Distinct()
                .OrderByDescending(c => c.Index)
                .ToList();

            foreach (var validParameter in validParameters)
            {
                var parameterIndex = validParameter.Index;
                var parameterName = validParameter.ParameterName;

                parameters.RemoveAt(
                    parameters.FindIndex(
                        t => t.Index == parameterIndex &&
                             t.ParameterName == parameterName));

                var parameter = command.Parameters[parameterName];
                var parameterValue = GetParameterValue(parameter);

                command.CommandText = command.CommandText.Substring(0, parameterIndex) +
                                      parameterValue +
                                      command.CommandText.Substring(parameterIndex + validParameter.ParameterName.Length);
            }

            foreach (var parameterName in validParameters
                .Select(c => c.ParameterName)
                .Distinct()
                .Where(s => parameters.FindIndex(t => t.ParameterName == s) == -1))
            {
                command.Parameters.RemoveAt(parameterName);
            }
        }
    }
}
