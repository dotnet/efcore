// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ThenIncludeExpressionNode : IncludeExpressionNodeBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[] { EntityFrameworkQueryableExtensions.ThenIncludeAfterEnumerableMethodInfo, EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ThenIncludeExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, navigationPropertyPathLambda)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ApplyNodeSpecificSemantics(
            QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
        {
            var includeResultOperator
                = (IncludeResultOperator)clauseGenerationContext.GetContextInfo(Source);

            if (!NavigationPropertyPathLambda.TryGetComplexPropertyAccess(out var propertyPath))
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidIncludeLambdaExpression(
                        nameof(EntityFrameworkQueryableExtensions.ThenInclude),
                        NavigationPropertyPathLambda));
            }

            includeResultOperator.AppendToNavigationPath(propertyPath);

            clauseGenerationContext.AddContextInfo(this, includeResultOperator);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
            => null;
    }
}
