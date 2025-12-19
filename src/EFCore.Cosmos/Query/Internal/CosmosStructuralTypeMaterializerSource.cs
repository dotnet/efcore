// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // StructuralTypeMaterializerSource is pubternal

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosStructuralTypeMaterializerSource(StructuralTypeMaterializerSourceDependencies dependencies)
    : StructuralTypeMaterializerSource(dependencies)
{
    /// <summary>
    ///     Complex properties are not handled in the initial materialization expression,
    ///     so we can more easily generate the necessary nested materialization expressions later in CosmosShapedQueryCompilingExpressionVisitor.
    /// </summary>
    protected override bool ReadComplexTypeDirectly(IComplexType complexType)
        => false;
}
