namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class ModelValidator
{
    protected virtual void ValidateEntityType(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateEntityClrType(entityType, logger);
        ValidateChangeTrackingStrategy(entityType, logger);
        ValidateIgnoredMembers(entityType, logger);
        ValidatePropertyMapping(entityType, logger);
        ValidateOwnership(entityType, logger);
        ValidateNonNullPrimaryKey(entityType, logger);
        ValidateInheritanceMapping(entityType, logger);
        ValidateFieldMapping(entityType, logger);
        ValidateQueryFilters(entityType, logger);

        foreach (var property in entityType.GetDeclaredProperties())
        {
            ValidateProperty(property, entityType, logger);
        }

        foreach (var complexProperty in entityType.GetDeclaredComplexProperties())
        {
            ValidateComplexProperty(complexProperty, logger);
        }

        LogShadowProperties(entityType, logger);
    }

    protected virtual void ValidateProperty(
        IProperty property,
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateTypeMapping(property, logger);
        ValidatePrimitiveCollection(property, logger);
        ValidateAutoLoaded(property, structuralType, logger);
    }

    /// <summary>
    ///     Validates that a property configured as not auto-loaded is not a key, foreign key, concurrency token or discriminator.
    /// </summary>
    protected virtual void ValidateAutoLoaded(
        IProperty property,
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (property.IsAutoLoaded)
        {
            return;
        }

        var typeName = structuralType.DisplayName();

        if (property.IsKey())
        {
            throw new InvalidOperationException(CoreStrings.AutoLoadedKeyProperty(property.Name, typeName));
        }

        if (property.IsForeignKey())
        {
            throw new InvalidOperationException(CoreStrings.AutoLoadedForeignKeyProperty(property.Name, typeName));
        }

        if (property.IsConcurrencyToken)
        {
            throw new InvalidOperationException(CoreStrings.AutoLoadedConcurrencyTokenProperty(property.Name, typeName));
        }

        if (structuralType is IEntityType entityType
            && entityType.FindDiscriminatorProperty() == property)
        {
            throw new InvalidOperationException(CoreStrings.AutoLoadedDiscriminatorProperty(property.Name, typeName));
        }

        if (structuralType.ConstructorBinding is not null
            && structuralType.ConstructorBinding.ParameterBindings
                .SelectMany(p => p.ConsumedProperties)
                .Contains(property))
        {
            throw new InvalidOperationException(
                CoreStrings.AutoLoadedConstructorProperty(property.Name, typeName));
        }
    }

    protected virtual void ValidateComplexProperty(
        IComplexProperty complexProperty,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var complexType = complexProperty.ComplexType;
        ValidateChangeTrackingStrategy(complexType, logger);

        foreach (var property in complexType.GetDeclaredProperties())
        {
            ValidateProperty(property, complexType, logger);
        }

        foreach (var nestedComplexProperty in complexType.GetDeclaredComplexProperties())
        {
            ValidateComplexProperty(nestedComplexProperty, logger);
        }
    }
}