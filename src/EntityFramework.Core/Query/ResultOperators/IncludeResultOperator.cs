// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        private List<PropertyInfo> _chainedNavigationProperties;

        public IncludeResultOperator([NotNull] Expression navigationPropertyPath)
        {
            Check.NotNull(navigationPropertyPath, "navigationPropertyPath");

            _navigationPropertyPath = navigationPropertyPath;
        }

        public virtual Expression NavigationPropertyPath => _navigationPropertyPath;

        public virtual IReadOnlyList<PropertyInfo> ChainedNavigationProperties => _chainedNavigationProperties;

        public override string ToString()
        {
            return "Include("
                   + FormattingExpressionTreeVisitor.Format(NavigationPropertyPath)
                   + (_chainedNavigationProperties != null
                       ? "." + _chainedNavigationProperties.Select(p => p.Name).Join(".")
                       : null)
                   + ")";
        }

        public override ResultOperatorBase Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            var includeResultOperator = new IncludeResultOperator(_navigationPropertyPath);

            if (_chainedNavigationProperties != null)
            {
                includeResultOperator.AppendToNavigationPath(_chainedNavigationProperties);
            }

            return includeResultOperator;
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

        public virtual void AppendToNavigationPath([NotNull] IReadOnlyList<PropertyInfo> propertyInfos)
        {
            Check.NotNull(propertyInfos, "propertyInfos");

            (_chainedNavigationProperties
             ?? (_chainedNavigationProperties = new List<PropertyInfo>()))
                .AddRange(propertyInfos);
        }
    }
}
