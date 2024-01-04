// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface ILiftableConstantFactory
{
    LiftableConstantExpression CreateLiftableConstant(
        Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
        string variableName,
        Type type);
}
