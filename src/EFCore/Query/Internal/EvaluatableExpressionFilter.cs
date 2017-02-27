// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
        private static readonly PropertyInfo _dateTimeNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));

        private static readonly PropertyInfo _dateTimeUtcNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow));

        private static readonly MethodInfo _guidNewGuid
            = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid));

        private static readonly List<MethodInfo> _randomNext
            = typeof(Random).GetTypeInfo().GetDeclaredMethods(nameof(Random.Next)).ToList();

        private HashSet<MethodInfo> _modelDbFunctions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EvaluatableExpressionFilter([NotNull] IEnumerable<IDbFunction> modelDbFunctions)
        {
            Check.NotNull(modelDbFunctions, nameof(modelDbFunctions));

            _modelDbFunctions = new HashSet<MethodInfo>(modelDbFunctions.Select(dbFunc => dbFunc.MethodInfo));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
        {
            if (_guidNewGuid.Equals(methodCallExpression.Method)
                || _randomNext.Contains(methodCallExpression.Method)
                || _modelDbFunctions.Contains(methodCallExpression.Method))
            {
                return false;
            }

            return base.IsEvaluatableMethodCall(methodCallExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool IsEvaluatableMember(MemberExpression memberExpression)
        {
            if (Equals(memberExpression.Member, _dateTimeNow)
                || Equals(memberExpression.Member, _dateTimeUtcNow))
            {
                return false;
            }

            return base.IsEvaluatableMember(memberExpression);
        }
    }
}
