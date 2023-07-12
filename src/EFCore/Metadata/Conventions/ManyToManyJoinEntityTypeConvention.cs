// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention which looks for matching skip navigations and automatically creates
///     a many-to-many join entity with suitable foreign keys, sets the two
///     matching skip navigations to use those foreign keys.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ManyToManyJoinEntityTypeConvention :
    ISkipNavigationAddedConvention,
    ISkipNavigationInverseChangedConvention,
    ISkipNavigationForeignKeyChangedConvention,
    ISkipNavigationRemovedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ManyToManyJoinEntityTypeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ManyToManyJoinEntityTypeConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionContext<IConventionSkipNavigationBuilder> context)
        => TryCreateJoinEntityType(skipNavigationBuilder);

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationInverseChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionSkipNavigation? inverse,
        IConventionSkipNavigation? oldInverse,
        IConventionContext<IConventionSkipNavigation> context)
        => TryCreateJoinEntityType(skipNavigationBuilder);

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        var joinEntityType = oldForeignKey?.DeclaringEntityType;
        var navigation = skipNavigationBuilder.Metadata;
        if (joinEntityType is not null
            && joinEntityType.IsInModel
            && navigation.IsCollection
            && navigation.ForeignKey?.DeclaringEntityType != joinEntityType)
        {
            ((InternalModelBuilder)joinEntityType.Model.Builder).RemoveImplicitJoinEntity((EntityType)joinEntityType);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation,
        IConventionContext<IConventionSkipNavigation> context)
    {
        var joinEntityType = navigation.ForeignKey?.DeclaringEntityType;
        if (joinEntityType is not null
            && joinEntityType.IsInModel
            && navigation.IsCollection)
        {
            ((InternalModelBuilder)joinEntityType.Model.Builder).RemoveImplicitJoinEntity((EntityType)joinEntityType);
        }
    }

    private void TryCreateJoinEntityType(IConventionSkipNavigationBuilder skipNavigationBuilder)
    {
        var skipNavigation = skipNavigationBuilder.Metadata;
        if (ShouldCreateJoinType(skipNavigation))
        {
            CreateJoinEntityType(GenerateJoinTypeName(skipNavigation), skipNavigation);
        }
    }

    /// <summary>
    ///     Checks whether a new join entity type is needed.
    /// </summary>
    /// <param name="skipNavigation">The target skip navigation.</param>
    /// <returns>A value indicating whether a new join entity type is needed.</returns>
    protected virtual bool ShouldCreateJoinType(IConventionSkipNavigation skipNavigation)
    {
        var inverseSkipNavigation = skipNavigation.Inverse;
        return skipNavigation.ForeignKey == null
            && skipNavigation.IsCollection
            && inverseSkipNavigation is { ForeignKey: null, IsCollection: true };
    }

    /// <summary>
    ///     Generates a unique name for the new joint entity type.
    /// </summary>
    /// <param name="skipNavigation">The target skip navigation.</param>
    /// <returns>A unique entity type name.</returns>
    protected virtual string GenerateJoinTypeName(IConventionSkipNavigation skipNavigation)
    {
        var inverseSkipNavigation = skipNavigation.Inverse;
        Check.DebugAssert(
            inverseSkipNavigation?.Inverse == skipNavigation,
            "Inverse's inverse should be the original skip navigation");

        var declaringEntityType = skipNavigation.DeclaringEntityType;
        var inverseEntityType = inverseSkipNavigation.DeclaringEntityType;
        var model = declaringEntityType.Model;
        var joinEntityTypeName = !declaringEntityType.HasSharedClrType
            ? declaringEntityType.ClrType.ShortDisplayName()
            : declaringEntityType.ShortName();
        var inverseName = !inverseEntityType.HasSharedClrType
            ? inverseEntityType.ClrType.ShortDisplayName()
            : inverseEntityType.ShortName();
        joinEntityTypeName = StringComparer.Ordinal.Compare(joinEntityTypeName, inverseName) < 0
            ? joinEntityTypeName + inverseName
            : inverseName + joinEntityTypeName;

        if (model.FindEntityType(joinEntityTypeName) != null)
        {
            var otherIdentifiers = model.GetEntityTypes().ToDictionary(et => et.Name, _ => 0);
            joinEntityTypeName = Uniquifier.Uniquify(
                joinEntityTypeName,
                otherIdentifiers,
                int.MaxValue);
        }

        return joinEntityTypeName;
    }

    /// <summary>
    ///     Create a join entity type and configures the corresponding foreign keys.
    /// </summary>
    /// <param name="joinEntityTypeName">The name for the new entity type.</param>
    /// <param name="skipNavigation">The target skip navigation.</param>
    protected virtual void CreateJoinEntityType(
        string joinEntityTypeName,
        IConventionSkipNavigation skipNavigation)
    {
        var model = skipNavigation.DeclaringEntityType.Model;

        var joinEntityTypeBuilder = model.Builder.SharedTypeEntity(joinEntityTypeName, Model.DefaultPropertyBagType)!;

        var inverseSkipNavigation = skipNavigation.Inverse!;
        CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
        CreateSkipNavigationForeignKey(inverseSkipNavigation, joinEntityTypeBuilder);
    }

    /// <summary>
    ///     Creates a foreign key on the given entity type to be used by the given skip navigation.
    /// </summary>
    /// <param name="skipNavigation">The target skip navigation.</param>
    /// <param name="joinEntityTypeBuilder">The join entity type.</param>
    /// <returns>The created foreign key.</returns>
    protected virtual IConventionForeignKey CreateSkipNavigationForeignKey(
        IConventionSkipNavigation skipNavigation,
        IConventionEntityTypeBuilder joinEntityTypeBuilder)
    {
        var foreignKey = ((InternalEntityTypeBuilder)joinEntityTypeBuilder)
            .HasRelationship(
                (EntityType)skipNavigation.DeclaringEntityType,
                ConfigurationSource.Convention,
                required: true,
                skipNavigation.Inverse!.Name)!
            .IsUnique(false, ConfigurationSource.Convention)!
            .Metadata;

        skipNavigation.Builder.HasForeignKey(foreignKey);

        return foreignKey;
    }
}
