// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public abstract class JoinExpressionBase : TableExpressionBase
    {
        protected TableExpressionBase _tableExpression;
        private Expression _predicate;

        protected JoinExpressionBase([NotNull] TableExpressionBase tableExpression)
            : base(
                Check.NotNull(tableExpression, nameof(tableExpression)).QuerySource,
                tableExpression.Alias)
        {
            _tableExpression = tableExpression;
        }

        public virtual TableExpressionBase TableExpression => _tableExpression;

        public virtual Expression Predicate
        {
            get { return _predicate; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _predicate = value;
            }
        }
    }
}
