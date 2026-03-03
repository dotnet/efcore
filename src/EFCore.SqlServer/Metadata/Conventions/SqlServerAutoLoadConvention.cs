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
public class SqlServerAutoLoadConvention : AutoLoadConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerAutoLoadConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public SqlServerAutoLoadConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override bool ShouldBeAutoLoaded(IConventionProperty property)
    {
        var typeMapping = property.FindTypeMapping();
        if (typeMapping is not null)
        {
            return typeMapping is not SqlServerVectorTypeMapping;
        }

        // Fall back to CLR type check when type mapping hasn't been resolved yet
        return property.GetValueConverter() == null
            && property.ClrType.TryGetElementType(typeof(SqlVector<>)) is null;
    }
}
