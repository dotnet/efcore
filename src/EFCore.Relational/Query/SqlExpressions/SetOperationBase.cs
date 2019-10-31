// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public abstract class SetOperationBase : TableExpressionBase
    {
        protected SetOperationBase([NotNull] string alias, SelectExpression source1, SelectExpression source2, bool distinct)
            : base(Check.NotEmpty(alias, nameof(alias)))
        {
            IsDistinct = distinct;
            Source1 = source1;
            Source2 = source2;
        }

        public virtual bool IsDistinct { get; }
        public virtual SelectExpression Source1 { get; }
        public virtual SelectExpression Source2 { get; }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SetOperationBase setOperationBase
                    && Equals(setOperationBase));

        private bool Equals(SetOperationBase setOperationBase)
            => IsDistinct == setOperationBase.IsDistinct
                && Source1.Equals(setOperationBase.Source1)
                && Source2.Equals(setOperationBase.Source2);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), IsDistinct, Source1, Source2);
    }
}
