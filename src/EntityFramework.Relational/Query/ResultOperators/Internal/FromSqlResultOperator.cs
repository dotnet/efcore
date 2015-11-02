// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query.ResultOperators.Internal
{
    public class FromSqlResultOperator : SequenceTypePreservingResultOperatorBase, IQueryAnnotation
    {
        public FromSqlResultOperator([NotNull] Expression sql, [NotNull] string argumentsParameterName)
        {
            Sql = sql;
            ArgumentsParameterName = argumentsParameterName;
        }

        public virtual IQuerySource QuerySource { get; [NotNull] set; }
        public virtual QueryModel QueryModel { get; set; }

        public virtual Expression Sql { get; }

        public virtual string ArgumentsParameterName { get; }

        public override string ToString() => $"FromSql('{Sql}')";

        public override ResultOperatorBase Clone([NotNull] CloneContext cloneContext)
            => new FromSqlResultOperator(Sql, ArgumentsParameterName);

        public override void TransformExpressions([NotNull] Func<Expression, Expression> transformation)
        {
        }

        public override StreamedSequence ExecuteInMemory<T>([NotNull] StreamedSequence input) => input;
    }
}
