// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class QueryAnnotationResultOperator : SequenceTypePreservingResultOperatorBase
    {
        private readonly ConstantExpression _annotationExpression;

        public QueryAnnotationResultOperator([NotNull] ConstantExpression annotationExpression)
        {
            Check.NotNull(annotationExpression, nameof(annotationExpression));

            _annotationExpression = annotationExpression;
            Annotation = (QueryAnnotationBase)annotationExpression.Value;
        }

        public virtual QueryAnnotationBase Annotation { get; }

        public override string ToString()
            => "AnnotateQuery(" + _annotationExpression + ")";

        public override ResultOperatorBase Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, nameof(cloneContext));

            return new QueryAnnotationResultOperator(_annotationExpression);
        }

        public override void TransformExpressions([NotNull] Func<Expression, Expression> transformation)
        {
        }

        public override StreamedSequence ExecuteInMemory<T>([NotNull] StreamedSequence input) => input;
    }
}
