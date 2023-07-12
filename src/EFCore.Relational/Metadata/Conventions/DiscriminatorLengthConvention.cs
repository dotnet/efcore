// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that sets the maximum length for string discriminator properties.
/// </summary>
/// <remarks>
///     <para>
///         The maximum length is set to a value large enough to cover all discriminator values in the hierarchy.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///         <see href="https://aka.ms/efcore-docs-inheritance">TPH mapping of inheritance hierarchies</see> for more information
///         and examples.
///     </para>
/// </remarks>
public class DiscriminatorLengthConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="DiscriminatorLengthConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public DiscriminatorLengthConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes()
                     .Where(entityType => entityType.BaseType == null))
        {
            var discriminatorProperty = entityType.FindDiscriminatorProperty();
            if (discriminatorProperty != null
                && discriminatorProperty.ClrType == typeof(string)
                && !discriminatorProperty.IsKey()
                && !discriminatorProperty.IsForeignKey())
            {
                var maxDiscriminatorValueLength =
                    entityType.GetDerivedTypesInclusive().Select(e => ((string)e.GetDiscriminatorValue()!).Length).Max();

                var previous = 1;
                var current = 1;
                while (maxDiscriminatorValueLength > current)
                {
                    var next = current + previous;
                    previous = current;
                    current = next;
                }

                discriminatorProperty.Builder.HasMaxLength(current);
            }
        }
    }
}
