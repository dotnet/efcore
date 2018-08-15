// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IncludeResultOperator : SequenceTypePreservingResultOperatorBase, IQueryAnnotation
    {
        private readonly List<INavigation[]> _navigationPaths;

        private List<string> _navigationPropertyPaths;
        private IQuerySource _querySource;
        private Expression _pathFromQuerySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeResultOperator(
            [NotNull] INavigation[] navigationPath, [NotNull] Expression pathFromQuerySource, bool implicitLoad = false)
        {
            _navigationPaths = new List<INavigation[]>
            {
                navigationPath
            };
            _navigationPropertyPaths = new List<string>();
            _pathFromQuerySource = pathFromQuerySource;

            IsImplicitLoad = implicitLoad;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeResultOperator(
            [NotNull] IEnumerable<string> navigationPropertyPaths,
            [NotNull] Expression pathFromQuerySource)
        {
            _navigationPropertyPaths = new List<string>(navigationPropertyPaths);
            _pathFromQuerySource = pathFromQuerySource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IQuerySource QuerySource
        {
            get => _querySource ?? (_querySource = GetQuerySource(PathFromQuerySource));
            set => _querySource = value;
        }

        private static IQuerySource GetQuerySource(Expression expression)
            => expression.TryGetReferencedQuerySource()
               ?? (expression is MemberExpression memberExpression
                   ? GetQuerySource(memberExpression.Expression.RemoveConvert())
                   : null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsImplicitLoad { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual QueryModel QueryModel { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<string> NavigationPropertyPaths => _navigationPropertyPaths;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<INavigation[]> NavigationPaths => _navigationPaths;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression PathFromQuerySource
        {
            get => _pathFromQuerySource;
            [param: NotNull] set => _pathFromQuerySource = value;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AppendToNavigationPath([NotNull] IReadOnlyList<PropertyInfo> propertyInfos)
        {
            if (_navigationPropertyPaths == null)
            {
                _navigationPropertyPaths = new List<string>();
            }

            _navigationPropertyPaths.AddRange(propertyInfos.Select(pi => pi.Name));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
            => $@"Include(""{NavigationPropertyPaths.Join(".")}"")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string DisplayString()
            => $"{PathFromQuerySource}.{_navigationPropertyPaths.Join(".")}";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ResultOperatorBase Clone(CloneContext cloneContext)
            => new IncludeResultOperator(NavigationPropertyPaths, PathFromQuerySource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input) => input;
    }
}
