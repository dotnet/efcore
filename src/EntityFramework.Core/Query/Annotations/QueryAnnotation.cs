// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Annotations
{
    public class QueryAnnotation : QueryAnnotationBase
    {
        private readonly MethodInfo _methodInfo;
        private readonly object[] _arguments;

        public QueryAnnotation(
            [NotNull] MethodInfo methodInfo,
            [NotNull] object[] arguments)
        {
            _methodInfo = Check.NotNull(methodInfo, nameof(methodInfo));
            _arguments = Check.NotNull(arguments, nameof(arguments));
        }

        public virtual MethodInfo MethodInfo => _methodInfo;

        public virtual IReadOnlyList<object> Arguments => _arguments;

        public virtual bool IsCallTo([NotNull] MethodInfo methodInfo)
            => Check.NotNull(methodInfo, nameof(methodInfo)).IsGenericMethod
                ? MethodInfo.GetGenericMethodDefinition().Equals(methodInfo)
                : MethodInfo.Equals(methodInfo);

        public override string ToString()
            => $"QueryAnnotation({_methodInfo.Name}({Arguments.Select(FormatArgument).Join()}))";

        private static string FormatArgument(object argument)
            => (argument?.GetType().IsArray ?? false)
                ? $"[{((IEnumerable)argument).Cast<object>().Select(FormatArgument).Join()}]"
                : Expression.Constant(argument).ToString();
    }
}
