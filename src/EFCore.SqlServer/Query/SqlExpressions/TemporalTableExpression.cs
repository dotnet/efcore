// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.SqlExpressions
{
    /// <summary>
    ///     Fill in later
    /// </summary>
    public class TemporalTableExpression : TableExpressionBase, ICloneable
    {
        /// <summary>
        ///     Fill in later
        /// </summary>
        public TemporalTableExpression(ITableBase table)
            : base(table.Name.Substring(0, 1).ToLowerInvariant())
        {
            Name = table.Name;
            Schema = table.Schema;
            TemporalOperationType = TemporalOperationType.All;
        }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public TemporalTableExpression(ITableBase table, DateTime pointInTime)
            : base(table.Name.Substring(0, 1).ToLowerInvariant())
        {
            Name = table.Name;
            Schema = table.Schema;
            PointInTime = pointInTime;
            TemporalOperationType = TemporalOperationType.AsOf;
        }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public TemporalTableExpression(ITableBase table, DateTime from, DateTime to, TemporalOperationType temporalOperationType)
            : base(table.Name.Substring(0, 1).ToLowerInvariant())
        {
            Name = table.Name;
            Schema = table.Schema;
            From = from;
            To = to;
            TemporalOperationType = temporalOperationType;
        }

        private TemporalTableExpression(
            string name,
            string? schema,
            string? alias,
            DateTime? pointInTime,
            DateTime? from,
            DateTime? to,
            TemporalOperationType temporalOperationType)
            : base(alias)
        {
            Name = name;
            Schema = schema;
            PointInTime = pointInTime;
            From = from;
            To = to;
            TemporalOperationType = temporalOperationType;
        }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual string? Schema { get; }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual DateTime? PointInTime { get; }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual DateTime? From { get; }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual DateTime? To { get; }

        /// <summary>
        ///     Fill in later
        /// </summary>
        public virtual TemporalOperationType TemporalOperationType { get; }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter
                .Append(Name)
                .Append(" FOR SYSTEM_TIME ");

            switch (TemporalOperationType)
            {
                case TemporalOperationType.AsOf:
                    expressionPrinter
                        .Append("AS OF ")
                        .Append(PointInTime.ToString()!);
                    break;

                case TemporalOperationType.FromTo:
                    expressionPrinter
                        .Append("FROM ")
                        .Append(From.ToString()!)
                        .Append(" TO ")
                        .Append(To.ToString()!);
                    break;

                case TemporalOperationType.Between:
                    expressionPrinter
                        .Append("BETWEEN ")
                        .Append(From.ToString()!)
                        .Append(" AND ")
                        .Append(To.ToString()!);
                    break;

                case TemporalOperationType.ContainedIn:
                    expressionPrinter
                        .Append("CONTAINED IN (")
                        .Append(From.ToString()!)
                        .Append(", ")
                        .Append(To.ToString()!)
                        .Append(")");
                    break;

                default:
                    // TemporalOperationType.All
                    expressionPrinter
                        .Append("ALL");
                    break;

            }

            if (Alias != null)
            {
                expressionPrinter.Append(" AS ").Append(Alias);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            // This should be reference equal only.
            => obj != null && ReferenceEquals(this, obj);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Name, Schema, PointInTime, From, To, TemporalOperationType);

            /// <inheritdoc />
        public virtual object Clone()
            => new TemporalTableExpression(Name, Schema, Alias, PointInTime, From, To, TemporalOperationType);
    }
}
