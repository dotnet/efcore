// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public abstract class JoinExpressionBase : TableExpressionBase
    {
        protected JoinExpressionBase([NotNull] TableExpressionBase table)
            : base(null)
        {
            Check.NotNull(table, nameof(table));

            Table = table;
        }

        public virtual TableExpressionBase Table { get; }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is JoinExpressionBase joinExpressionBase
                    && Equals(joinExpressionBase));

        private bool Equals(JoinExpressionBase joinExpressionBase)
            => base.Equals(joinExpressionBase)
                && Table.Equals(joinExpressionBase.Table);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Table);
    }
}
