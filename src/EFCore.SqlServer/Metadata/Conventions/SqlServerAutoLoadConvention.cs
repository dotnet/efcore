// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures SQL Server vector properties as not auto-loaded.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
/// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
public class SqlServerAutoLoadConvention(ProviderConventionSetBuilderDependencies dependencies) : AutoLoadConvention(dependencies)
{
    /// <inheritdoc />
    protected override bool ShouldBeAutoLoaded(IConventionProperty property)
    {
        var typeMapping = property.FindTypeMapping();
        if (typeMapping is not null)
        {
            return typeMapping is not SqlServerVectorTypeMapping;
        }

        // Fall back to CLR type check when type mapping hasn't been resolved yet.
        // If there's a value converter, the CLR type may not reflect the store type,
        // so we can only check for SqlVector<> when there's no converter.
        return property.GetValueConverter() is not null
            || property.ClrType.UnwrapNullableType().TryGetElementType(typeof(SqlVector<>)) is null;
    }
}
