// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class LiftableConstantFactory : ILiftableConstantFactory
{
    public virtual LiftableConstantExpression CreateLiftableConstant(
        Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
        string variableName,
        Type type)
        => new(resolverExpression, variableName, type);
}
