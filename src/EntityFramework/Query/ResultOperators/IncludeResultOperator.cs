// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class IncludeResultOperator : SequenceTypePreservingResultOperatorBase
    {
        private Expression _navigationPropertyPath;

        public IncludeResultOperator([NotNull] Expression navigationPropertyPath)
        {
            Check.NotNull(navigationPropertyPath, "navigationPropertyPath");

            _navigationPropertyPath = navigationPropertyPath;
        }

        public virtual Expression NavigationPropertyPath
        {
            get { return _navigationPropertyPath; }
        }

        public override string ToString()
        {
            return "Include("
                   + FormattingExpressionTreeVisitor.Format(NavigationPropertyPath)
                   + ")";
        }

        public override ResultOperatorBase Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            return new IncludeResultOperator(_navigationPropertyPath);
        }

        public override void TransformExpressions([NotNull] Func<Expression, Expression> transformation)
        {
            Check.NotNull(transformation, "transformation");

            _navigationPropertyPath = transformation(_navigationPropertyPath);
        }

        public override StreamedSequence ExecuteInMemory<T>([NotNull] StreamedSequence input)
        {
            Check.NotNull(input, "input");

            return input;
        }
    }
}
