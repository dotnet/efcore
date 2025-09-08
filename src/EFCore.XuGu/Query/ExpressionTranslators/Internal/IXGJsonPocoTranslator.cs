// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    public interface IXGJsonPocoTranslator : IMemberTranslator
    {
        SqlExpression TranslateMemberAccess([NotNull] SqlExpression instance, [NotNull] SqlExpression member, [NotNull] Type returnType);
        SqlExpression TranslateArrayLength([NotNull] SqlExpression expression);
        string GetJsonPropertyName([CanBeNull] MemberInfo member);
    }
}
