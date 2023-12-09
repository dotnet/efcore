// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures property mappings have any ElementMapping discovered by the type mapper.
/// </summary>
/// <remarks>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class ElementMappingConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ElementMappingConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ElementMappingConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            Validate(entityType);
        }

        void Validate(IConventionTypeBase typeBase)
        {
            foreach (var property in typeBase.GetDeclaredProperties())
            {
                var typeMapping = Dependencies.TypeMappingSource.FindMapping((IProperty)property);
                if (typeMapping is { ElementTypeMapping: not null })
                {
                    property.Builder.SetElementType(property.ClrType.TryGetElementType(typeof(IEnumerable<>)));
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                Validate(complexProperty.ComplexType);
            }
        }
    }
}
