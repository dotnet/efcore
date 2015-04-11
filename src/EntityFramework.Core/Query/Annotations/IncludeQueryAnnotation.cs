// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;

namespace Microsoft.Data.Entity.Query.Annotations
{
    public class IncludeQueryAnnotation : QueryAnnotation
    {
        private List<PropertyInfo> _chainedNavigationProperties;

        public virtual Expression NavigationPropertyPath { get; }

        public virtual IReadOnlyList<PropertyInfo> ChainedNavigationProperties => _chainedNavigationProperties;

        public IncludeQueryAnnotation([NotNull] Expression navigationPropertyPath)
        {
            Check.NotNull(navigationPropertyPath, nameof(navigationPropertyPath));

            NavigationPropertyPath = navigationPropertyPath;
            _chainedNavigationProperties = new List<PropertyInfo>();
        }

        public virtual void AppendToNavigationPath([NotNull] IReadOnlyList<PropertyInfo> propertyInfos)
        {
            Check.NotNull(propertyInfos, nameof(propertyInfos));

            _chainedNavigationProperties.AddRange(propertyInfos);
        }

        public override string ToString()
            => "Include("
                + FormattingExpressionTreeVisitor.Format(NavigationPropertyPath)
                    + (_chainedNavigationProperties.Count > 0
                        ? _chainedNavigationProperties.Select(p => p.Name).Join(".")
                        : string.Empty)
                + ")";
    }
}
