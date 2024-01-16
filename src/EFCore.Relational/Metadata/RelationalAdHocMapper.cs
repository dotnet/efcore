// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Creates ad-hoc mappings of CLR types to entity types after the model has been built.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class RelationalAdHocMapper : AdHocMapper
{
    /// <summary>
    ///     Do not call this constructor directly from either provider or application code as it may change
    ///     as new dependencies are added. Instead, use this type in your constructor so that an instance
    ///     will be created and injected automatically by the dependency injection container. To create
    ///     an instance with some dependent services replaced, first resolve the object from the dependency
    ///     injection container, then replace selected services using the C# 'with' operator. Do not call
    ///     the constructor at any point in this process.
    /// </summary>
    public RelationalAdHocMapper(AdHocMapperDependencies dependencies, RelationalAdHocMapperDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalAdHocMapperDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override ConventionSet BuildConventionSet()
    {
        var conventionSet = base.BuildConventionSet();
        conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));
        conventionSet.Remove(typeof(TableNameFromDbSetConvention));
        conventionSet.Remove(typeof(TableValuedDbFunctionConvention));
        return conventionSet;
    }
}
