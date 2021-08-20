// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TemporalFromToTableExpression : TemporalRangeTableExpression
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TemporalFromToTableExpression(ITableBase table, DateTime from, DateTime to)
            : base(table, from, to)
        {
        }

        private TemporalFromToTableExpression(string name, string? schema, string? alias, DateTime from, DateTime to)
            : base(name, schema, alias, from, to)
        {
        }

        /// <inheritdoc />
        public override TableExpressionBase Clone() => new TemporalFromToTableExpression(Name, Schema, Alias, From, To);

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter
                .Append(Name)
                .Append(" FOR SYSTEM_TIME ")
                .Append("FROM ")
                .Append(From.ToString())
                .Append(" TO ")
                .Append(To.ToString());

            if (Alias != null)
            {
                expressionPrinter.Append(" AS ").Append(Alias);
            }
        }
    }
}
