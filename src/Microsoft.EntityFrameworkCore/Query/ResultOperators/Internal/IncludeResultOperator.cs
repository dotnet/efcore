// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query.ResultOperators.Internal
{
    public class IncludeResultOperator : SequenceTypePreservingResultOperatorBase, IQueryAnnotation
    {
        private List<PropertyInfo> _chainedNavigationProperties;

        public IncludeResultOperator([NotNull] MemberExpression navigationPropertyPath)
        {
            NavigationPropertyPath = navigationPropertyPath;
            QuerySource = GetQuerySource(navigationPropertyPath);
        }

        private static IQuerySource GetQuerySource(MemberExpression expression)
        {
            var expressionWithoutConvert = expression.Expression.RemoveConvert();

            return (expressionWithoutConvert as QuerySourceReferenceExpression)?.ReferencedQuerySource
                   ?? GetQuerySource((MemberExpression)expressionWithoutConvert);
        }

        public virtual IQuerySource QuerySource { get; set; }
        public virtual QueryModel QueryModel { get; set; }

        public virtual MemberExpression NavigationPropertyPath { get; }

        public virtual IReadOnlyList<PropertyInfo> ChainedNavigationProperties => _chainedNavigationProperties;

        public virtual void AppendToNavigationPath([NotNull] IReadOnlyList<PropertyInfo> propertyInfos)
        {
            if (_chainedNavigationProperties == null)
            {
                _chainedNavigationProperties = new List<PropertyInfo>();
            }

            _chainedNavigationProperties.AddRange(propertyInfos);
        }

        public override string ToString()
            => "Include("
               + NavigationPropertyPath
               + (_chainedNavigationProperties != null
                   ? "." + _chainedNavigationProperties.Select(p => p.Name).Join(".")
                   : string.Empty)
               + ")";

        public override ResultOperatorBase Clone(CloneContext cloneContext)
            => new IncludeResultOperator(NavigationPropertyPath)
            {
                _chainedNavigationProperties = _chainedNavigationProperties
            };

        public override void TransformExpressions([NotNull] Func<Expression, Expression> transformation)
        {
        }

        public override StreamedSequence ExecuteInMemory<T>([NotNull] StreamedSequence input) => input;
    }
}
