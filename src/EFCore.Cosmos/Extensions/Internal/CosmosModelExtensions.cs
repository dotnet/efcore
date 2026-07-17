// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosModelExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosStructuralTypeSerializerProvider GetCosmosStructuralTypeSerializerProvider(this IModel model)
        => ((Lazy<CosmosStructuralTypeSerializerProvider>)(model.FindRuntimeAnnotation(CosmosAnnotationNames.StructuralTypeSerializerProvider)?.Value ?? throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetCosmosStructuralTypeSerializerProvider))))).Value;
}
