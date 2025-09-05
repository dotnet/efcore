// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the SQLite value generation strategy for properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqliteValueGenerationConvention : ValueGenerationConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqliteValueGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqliteValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Returns the store value generation strategy to set for the given property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The strategy to set for the property.</returns>
    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var declaringType = property.DeclaringType;
        
        var strategy = GetValueGenerationStrategy(property);
        if (strategy == SqliteValueGenerationStrategy.Autoincrement)
        {
            return ValueGenerated.OnAdd;
        }

        return base.GetValueGenerated(property);
    }

    private static SqliteValueGenerationStrategy GetValueGenerationStrategy(IConventionProperty property)
    {
        var entityType = (IConventionEntityType)property.DeclaringType;
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is { Properties.Count: 1 }
            && primaryKey.Properties[0] == property
            && property.ClrType.UnwrapNullableType().IsInteger()
            && property.FindTypeMapping()?.Converter == null)
        {
            return SqliteValueGenerationStrategy.Autoincrement;
        }

        return SqliteValueGenerationStrategy.None;
    }
}