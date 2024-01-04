// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public class RelationalLiftableConstantFactory : LiftableConstantFactory, IRelationalLiftableConstantFactory
{
    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalLiftableConstantFactory(
#pragma warning disable EF1001 // Internal EF Core API usage.
        LiftableConstantExpressionDependencies dependencies,
#pragma warning restore EF1001 // Internal EF Core API usage.
        RelationalLiftableConstantExpressionDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalLiftableConstantExpressionDependencies RelationalDependencies { get; }

    /// <summary>
    ///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual LiftableConstantExpression CreateLiftableConstant(
        ConstantExpression originalExpression,
        Expression<Func<RelationalMaterializerLiftableConstantContext, object>> resolverExpression,
        string variableName,
        Type type)
        => new(originalExpression, resolverExpression, variableName, type);
}
