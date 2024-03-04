// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that creates an optimized copy of the mutable model. This convention is typically
///     implemented by database providers to update provider annotations when creating a read-only model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RuntimeModelConvention : IModelFinalizedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RuntimeModelConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public RuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IModel ProcessModelFinalized(IModel model)
        => Create(model).FinalizeModel();

    /// <summary>
    ///     Creates an optimized model base on the supplied one.
    /// </summary>
    /// <param name="model">The source model.</param>
    /// <returns>An optimized model.</returns>
    protected virtual RuntimeModel Create(IModel model)
    {
        var runtimeModel = new RuntimeModel(
            skipDetectChanges: ((IRuntimeModel)model).SkipDetectChanges,
            modelId: model.ModelId,
            entityTypeCount: model.GetEntityTypes().Count(),
            typeConfigurationCount: model.GetTypeMappingConfigurations().Count());
        ((IModel)runtimeModel).ModelDependencies = model.ModelDependencies!;

        var entityTypes = model.GetEntityTypesInHierarchicalOrder();
        var entityTypePairs = new List<(IEntityType Source, RuntimeEntityType Target)>();

        foreach (var entityType in entityTypes)
        {
            var runtimeEntityType = Create(entityType, runtimeModel);
            entityTypePairs.Add((entityType, runtimeEntityType));

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var runtimeProperty = Create(property, runtimeEntityType);
                CreateAnnotations(
                    property, runtimeProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessPropertyAnnotations(annotations, source, target, runtime));

                var elementType = property.GetElementType();
                if (elementType != null)
                {
                    var runtimeElementType = Create(runtimeProperty, elementType, property.IsPrimitiveCollection);
                    CreateAnnotations(
                        elementType, runtimeElementType, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessElementTypeAnnotations(annotations, source, target, runtime));
                }
            }

            foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
            {
                var runtimeServiceProperty = Create(serviceProperty, runtimeEntityType);
                CreateAnnotations(
                    serviceProperty, runtimeServiceProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessServicePropertyAnnotations(annotations, source, target, runtime));
                runtimeServiceProperty.ParameterBinding =
                    (ServiceParameterBinding)Create(serviceProperty.ParameterBinding, runtimeEntityType);
            }

            foreach (var property in entityType.GetDeclaredComplexProperties())
            {
                var runtimeProperty = Create(property, runtimeEntityType);
                CreateAnnotations(
                    property, runtimeProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessComplexPropertyAnnotations(annotations, source, target, runtime));
            }

            foreach (var key in entityType.GetDeclaredKeys())
            {
                var runtimeKey = Create(key, runtimeEntityType);
                if (key.IsPrimaryKey())
                {
                    runtimeEntityType.SetPrimaryKey(runtimeKey);
                }

                CreateAnnotations(
                    key, runtimeKey, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessKeyAnnotations(annotations, source, target, runtime));
            }

            foreach (var index in entityType.GetDeclaredIndexes())
            {
                var runtimeIndex = Create(index, runtimeEntityType);
                CreateAnnotations(
                    index, runtimeIndex, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessIndexAnnotations(annotations, source, target, runtime));
            }

            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                var runtimeTrigger = Create(trigger, runtimeEntityType);
                CreateAnnotations(
                    trigger, runtimeTrigger, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessTriggerAnnotations(annotations, source, target, runtime));
            }

            runtimeEntityType.ConstructorBinding = Create(entityType.ConstructorBinding, runtimeEntityType);
            runtimeEntityType.ServiceOnlyConstructorBinding =
                Create(((IRuntimeEntityType)entityType).ServiceOnlyConstructorBinding, runtimeEntityType);
        }

        foreach (var (entityType, runtimeEntityType) in entityTypePairs)
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
            {
                var runtimeForeignKey = Create(foreignKey, runtimeEntityType);

                var navigation = foreignKey.DependentToPrincipal;
                if (navigation != null)
                {
                    var runtimeNavigation = Create(navigation, runtimeForeignKey);
                    CreateAnnotations(
                        navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                }

                navigation = foreignKey.PrincipalToDependent;
                if (navigation != null)
                {
                    var runtimeNavigation = Create(navigation, runtimeForeignKey);
                    CreateAnnotations(
                        navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                }

                CreateAnnotations(
                    foreignKey, runtimeForeignKey, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessForeignKeyAnnotations(annotations, source, target, runtime));
            }
        }

        foreach (var (entityType, runtimeEntityType) in entityTypePairs)
        {
            foreach (var navigation in entityType.GetDeclaredSkipNavigations())
            {
                var runtimeNavigation = Create(navigation, runtimeEntityType);

                var inverse = runtimeNavigation.TargetEntityType.FindSkipNavigation(navigation.Inverse.Name);
                if (inverse != null)
                {
                    runtimeNavigation.Inverse = inverse;
                    inverse.Inverse = runtimeNavigation;
                }

                CreateAnnotations(
                    navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessSkipNavigationAnnotations(annotations, source, target, runtime));
            }

            CreateAnnotations(
                entityType, runtimeEntityType, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessEntityTypeAnnotations(annotations, source, target, runtime));
        }

        foreach (var typeConfiguration in model.GetTypeMappingConfigurations())
        {
            var runtimeTypeConfiguration = Create(typeConfiguration, runtimeModel);
            CreateAnnotations(
                typeConfiguration, runtimeTypeConfiguration, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessTypeMappingConfigurationAnnotations(annotations, source, target, runtime));
        }

        CreateAnnotations(
            model, runtimeModel, static (convention, annotations, source, target, runtime) =>
                convention.ProcessModelAnnotations(annotations, source, target, runtime));

        return runtimeModel;
    }

    private void CreateAnnotations<TSource, TTarget>(
        TSource source,
        TTarget target,
        Action<RuntimeModelConvention, Dictionary<string, object?>, TSource, TTarget, bool> process)
        where TSource : IAnnotatable
        where TTarget : RuntimeAnnotatableBase
    {
        var annotations = source.GetAnnotations().ToDictionary(a => a.Name, a => a.Value);
        process(this, annotations, source, target, false);
        target.AddAnnotations(annotations);

        annotations = source.GetRuntimeAnnotations().ToDictionary(a => a.Name, a => a.Value);
        process(this, annotations, source, target, true);
        target.AddRuntimeAnnotations(annotations);
    }

    /// <summary>
    ///     Updates the model annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="model">The source model.</param>
    /// <param name="runtimeModel">The target model that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessModelAnnotations(
        Dictionary<string, object?> annotations,
        IModel model,
        RuntimeModel runtimeModel,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.ProductVersion
                    && key != CoreAnnotationNames.FullChangeTrackingNotificationsRequired)
                {
                    annotations.Remove(key);
                }
            }
        }
        else
        {
            annotations.Remove(CoreAnnotationNames.ModelDependencies);
            annotations[CoreAnnotationNames.ReadOnlyModel] = runtimeModel;
        }
    }

    private static RuntimeEntityType Create(IEntityType entityType, RuntimeModel model)
        => model.AddEntityType(
            entityType.Name,
            entityType.ClrType,
            entityType.BaseType == null ? null : model.FindEntityType(entityType.BaseType.Name)!,
            entityType.HasSharedClrType,
            entityType.GetDiscriminatorPropertyName(),
            entityType.GetChangeTrackingStrategy(),
            entityType.FindIndexerPropertyInfo(),
            entityType.IsPropertyBag,
            entityType.GetDiscriminatorValue(),
            derivedTypesCount: entityType.GetDirectlyDerivedTypes().Count(),
            propertyCount: entityType.GetDeclaredProperties().Count(),
            complexPropertyCount: entityType.GetDeclaredComplexProperties().Count(),
            navigationCount: entityType.GetDeclaredNavigations().Count(),
            skipNavigationCount: entityType.GetDeclaredSkipNavigations().Count(),
            servicePropertyCount: entityType.GetDeclaredServiceProperties().Count(),
            foreignKeyCount: entityType.GetDeclaredForeignKeys().Count(),
            unnamedIndexCount: entityType.GetDeclaredIndexes().Count(i => i.Name == null),
            namedIndexCount: entityType.GetDeclaredProperties().Count(i => i.Name != null),
            keyCount: entityType.GetDeclaredKeys().Count(),
            triggerCount: entityType.GetDeclaredTriggers().Count());

    private static ParameterBinding Create(ParameterBinding parameterBinding, RuntimeEntityType entityType)
        => parameterBinding.With(
            parameterBinding.ConsumedProperties.Select(
                property =>
                    (entityType.FindProperty(property.Name)
                        ?? entityType.FindServiceProperty(property.Name)
                        ?? entityType.FindNavigation(property.Name)
                        ?? (IPropertyBase?)entityType.FindSkipNavigation(property.Name))!).ToArray());

    private static InstantiationBinding? Create(InstantiationBinding? instantiationBinding, RuntimeEntityType entityType)
        => instantiationBinding?.With(instantiationBinding.ParameterBindings.Select(binding => Create(binding, entityType)).ToList());

    /// <summary>
    ///     Updates the entity type annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="entityType">The source entity type.</param>
    /// <param name="runtimeEntityType">The target entity type that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessEntityTypeAnnotations(
        Dictionary<string, object?> annotations,
        IEntityType entityType,
        RuntimeEntityType runtimeEntityType,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key)
                    && key != CoreAnnotationNames.QueryFilter
#pragma warning disable CS0612 // Type or member is obsolete
                    && key != CoreAnnotationNames.DefiningQuery
#pragma warning restore CS0612 // Type or member is obsolete
                    && key != CoreAnnotationNames.DiscriminatorMappingComplete)
                {
                    annotations.Remove(key);
                }
            }

            if (annotations.TryGetValue(CoreAnnotationNames.QueryFilter, out var queryFilter))
            {
                annotations[CoreAnnotationNames.QueryFilter] =
                    new QueryRootRewritingExpressionVisitor(runtimeEntityType.Model).Rewrite((Expression)queryFilter!);
            }

#pragma warning disable CS0612 // Type or member is obsolete
            if (annotations.TryGetValue(CoreAnnotationNames.DefiningQuery, out var definingQuery))
            {
                annotations[CoreAnnotationNames.DefiningQuery] =
#pragma warning restore CS0612 // Type or member is obsolete
                    new QueryRootRewritingExpressionVisitor(runtimeEntityType.Model).Rewrite((Expression)definingQuery!);
            }
        }
    }

    private static RuntimeTypeMappingConfiguration Create(ITypeMappingConfiguration typeConfiguration, RuntimeModel model)
    {
        var valueConverterType = (Type?)typeConfiguration[CoreAnnotationNames.ValueConverterType];
        var valueConverter = valueConverterType == null
            ? null
            : (ValueConverter?)Activator.CreateInstance(valueConverterType);

        return model.AddTypeMappingConfiguration(
            typeConfiguration.ClrType,
            typeConfiguration.GetMaxLength(),
            typeConfiguration.IsUnicode(),
            typeConfiguration.GetPrecision(),
            typeConfiguration.GetScale(),
            typeConfiguration.GetProviderClrType(),
            valueConverter);
    }

    /// <summary>
    ///     Updates the property annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="typeConfiguration">The source property.</param>
    /// <param name="runtimeTypeConfiguration">The target property that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessTypeMappingConfigurationAnnotations(
        Dictionary<string, object?> annotations,
        ITypeMappingConfiguration typeConfiguration,
        RuntimeTypeMappingConfiguration runtimeTypeConfiguration,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private static RuntimeProperty Create(IProperty property, RuntimeTypeBase runtimeType)
        => runtimeType is RuntimeEntityType runtimeEntityType
            ? runtimeEntityType.AddProperty(
                property.Name,
                property.ClrType,
                property.PropertyInfo,
                property.FieldInfo,
                property.GetPropertyAccessMode(),
                nullable: property.IsNullable,
                concurrencyToken: property.IsConcurrencyToken,
                valueGenerated: property.ValueGenerated,
                beforeSaveBehavior: property.GetBeforeSaveBehavior(),
                afterSaveBehavior: property.GetAfterSaveBehavior(),
                maxLength: property.GetMaxLength(),
                unicode: property.IsUnicode(),
                precision: property.GetPrecision(),
                scale: property.GetScale(),
                providerPropertyType: property.GetProviderClrType(),
                valueGeneratorFactory: property.GetValueGeneratorFactory(),
                valueConverter: property.GetValueConverter(),
                valueComparer: property.GetValueComparer(),
                keyValueComparer: property.GetKeyValueComparer(),
                providerValueComparer: property.GetProviderValueComparer(),
                jsonValueReaderWriter: property.GetJsonValueReaderWriter(),
                typeMapping: property.GetTypeMapping(),
                sentinel: property.Sentinel)
            : ((RuntimeComplexType)runtimeType).AddProperty(
                property.Name,
                property.ClrType,
                property.PropertyInfo,
                property.FieldInfo,
                property.GetPropertyAccessMode(),
                nullable: property.IsNullable,
                concurrencyToken: property.IsConcurrencyToken,
                valueGenerated: property.ValueGenerated,
                beforeSaveBehavior: property.GetBeforeSaveBehavior(),
                afterSaveBehavior: property.GetAfterSaveBehavior(),
                maxLength: property.GetMaxLength(),
                unicode: property.IsUnicode(),
                precision: property.GetPrecision(),
                scale: property.GetScale(),
                providerPropertyType: property.GetProviderClrType(),
                valueGeneratorFactory: property.GetValueGeneratorFactory(),
                valueConverter: property.GetValueConverter(),
                valueComparer: property.GetValueComparer(),
                keyValueComparer: property.GetKeyValueComparer(),
                providerValueComparer: property.GetProviderValueComparer(),
                jsonValueReaderWriter: property.GetJsonValueReaderWriter(),
                typeMapping: property.GetTypeMapping(),
                sentinel: property.Sentinel);

    private static RuntimeElementType Create(RuntimeProperty runtimeProperty, IElementType element, bool primitiveCollection)
        => runtimeProperty.SetElementType(
            element.ClrType,
            element.IsNullable,
            element.GetMaxLength(),
            element.IsUnicode(),
            element.GetPrecision(),
            element.GetScale(),
            element.GetProviderClrType(),
            element.GetValueConverter(),
            element.GetValueComparer(),
            element.GetJsonValueReaderWriter(),
            element.GetTypeMapping(),
            primitiveCollection);

    /// <summary>
    ///     Updates the property annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="property">The source property.</param>
    /// <param name="runtimeProperty">The target property that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    /// <summary>
    ///     Updates the element type annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="element">The source element type.</param>
    /// <param name="runtimeElement">The target element type that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessElementTypeAnnotations(
        Dictionary<string, object?> annotations,
        IElementType element,
        RuntimeElementType runtimeElement,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private static RuntimeServiceProperty Create(IServiceProperty property, RuntimeEntityType runtimeEntityType)
        => runtimeEntityType.AddServiceProperty(
            property.Name,
            property.PropertyInfo,
            property.FieldInfo,
            property.ClrType,
            property.GetPropertyAccessMode());

    /// <summary>
    ///     Updates the service property annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="property">The source service property.</param>
    /// <param name="runtimeProperty">The target service property that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessServicePropertyAnnotations(
        Dictionary<string, object?> annotations,
        IServiceProperty property,
        RuntimeServiceProperty runtimeProperty,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private RuntimeComplexProperty Create(IComplexProperty complexProperty, RuntimeTypeBase runtimeStructuralType)
    {
        var runtimeComplexProperty = runtimeStructuralType.AddComplexProperty(
            complexProperty.Name,
            complexProperty.ClrType,
            complexProperty.ComplexType.Name,
            complexProperty.ComplexType.ClrType,
            complexProperty.PropertyInfo,
            complexProperty.FieldInfo,
            complexProperty.GetPropertyAccessMode(),
            complexProperty.IsNullable,
            complexProperty.IsCollection,
            complexProperty.ComplexType.GetChangeTrackingStrategy(),
            complexProperty.ComplexType.FindIndexerPropertyInfo(),
            complexProperty.ComplexType.IsPropertyBag,
            propertyCount: complexProperty.ComplexType.GetDeclaredProperties().Count(),
            complexPropertyCount: complexProperty.ComplexType.GetDeclaredComplexProperties().Count());

        var complexType = complexProperty.ComplexType;
        var runtimeComplexType = runtimeComplexProperty.ComplexType;

        foreach (var property in complexType.GetProperties())
        {
            var runtimeProperty = Create(property, runtimeComplexType);
            CreateAnnotations(
                property, runtimeProperty, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessPropertyAnnotations(annotations, source, target, runtime));

            var elementType = property.GetElementType();
            if (elementType != null)
            {
                var runtimeElementType = Create(runtimeProperty, elementType, property.IsPrimitiveCollection);
                CreateAnnotations(
                    elementType, runtimeElementType, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessElementTypeAnnotations(annotations, source, target, runtime));
            }
        }

        foreach (var property in complexType.GetComplexProperties())
        {
            var runtimeProperty = Create(property, runtimeComplexType);
            CreateAnnotations(
                property, runtimeProperty, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessComplexPropertyAnnotations(annotations, source, target, runtime));
        }

        CreateAnnotations(
            complexType, runtimeComplexType, static (convention, annotations, source, target, runtime) =>
                convention.ProcessComplexTypeAnnotations(annotations, source, target, runtime));
        return runtimeComplexProperty;
    }

    /// <summary>
    ///     Updates the property annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="property">The source property.</param>
    /// <param name="runtimeProperty">The target property that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessComplexPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IComplexProperty property,
        RuntimeComplexProperty runtimeProperty,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    /// <summary>
    ///     Updates the complex type annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="complexType">The source complex type.</param>
    /// <param name="runtimeComplexType">The target complex type that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessComplexTypeAnnotations(
        Dictionary<string, object?> annotations,
        IComplexType complexType,
        RuntimeComplexType runtimeComplexType,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private static RuntimeKey Create(IKey key, RuntimeEntityType runtimeEntityType)
        => runtimeEntityType.AddKey(runtimeEntityType.FindProperties(key.Properties.Select(p => p.Name))!);

    /// <summary>
    ///     Updates the key annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="key">The source key.</param>
    /// <param name="runtimeKey">The target key that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessKeyAnnotations(
        Dictionary<string, object?> annotations,
        IKey key,
        RuntimeKey runtimeKey,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (s, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(s))
                {
                    annotations.Remove(s);
                }
            }
        }
    }

    private static RuntimeIndex Create(IIndex index, RuntimeEntityType runtimeEntityType)
        => runtimeEntityType.AddIndex(
            runtimeEntityType.FindProperties(index.Properties.Select(p => p.Name))!,
            index.Name,
            index.IsUnique);

    /// <summary>
    ///     Updates the index annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="index">The source index.</param>
    /// <param name="runtimeIndex">The target index that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessIndexAnnotations(
        Dictionary<string, object?> annotations,
        IIndex index,
        RuntimeIndex runtimeIndex,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private RuntimeForeignKey Create(IForeignKey foreignKey, RuntimeEntityType runtimeEntityType)
    {
        var principalEntityType = runtimeEntityType.Model.FindEntityType(foreignKey.PrincipalEntityType.Name)!;
        return runtimeEntityType.AddForeignKey(
            runtimeEntityType.FindProperties(foreignKey.Properties.Select(p => p.Name))!,
            GetKey(foreignKey.PrincipalKey, principalEntityType),
            principalEntityType,
            foreignKey.DeleteBehavior,
            foreignKey.IsUnique,
            foreignKey.IsRequired,
            foreignKey.IsRequiredDependent,
            foreignKey.IsOwnership);
    }

    private static RuntimeTrigger Create(ITrigger trigger, RuntimeEntityType runtimeEntityType)
        => runtimeEntityType.AddTrigger(trigger.ModelName);

    /// <summary>
    ///     Updates the trigger annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="trigger">The source trigger.</param>
    /// <param name="runtimeTrigger">The target trigger that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessTriggerAnnotations(
        Dictionary<string, object?> annotations,
        ITrigger trigger,
        RuntimeTrigger runtimeTrigger,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    /// <summary>
    ///     Updates the foreign key annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="foreignKey">The source foreign key.</param>
    /// <param name="runtimeForeignKey">The target foreign key that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessForeignKeyAnnotations(
        Dictionary<string, object?> annotations,
        IForeignKey foreignKey,
        RuntimeForeignKey runtimeForeignKey,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private static RuntimeNavigation Create(INavigation navigation, RuntimeForeignKey runtimeForeignKey)
        => (navigation.IsOnDependent ? runtimeForeignKey.DeclaringEntityType : runtimeForeignKey.PrincipalEntityType)
            .AddNavigation(
                navigation.Name,
                runtimeForeignKey,
                navigation.IsOnDependent,
                navigation.ClrType,
                navigation.PropertyInfo,
                navigation.FieldInfo,
                navigation.GetPropertyAccessMode(),
                navigation.IsEagerLoaded,
                navigation.LazyLoadingEnabled);

    /// <summary>
    ///     Updates the navigation annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="navigation">The source navigation.</param>
    /// <param name="runtimeNavigation">The target navigation that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessNavigationAnnotations(
        Dictionary<string, object?> annotations,
        INavigation navigation,
        RuntimeNavigation runtimeNavigation,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    private RuntimeSkipNavigation Create(ISkipNavigation navigation, RuntimeEntityType runtimeEntityType)
        => runtimeEntityType.AddSkipNavigation(
            navigation.Name,
            runtimeEntityType.Model.FindEntityType(navigation.TargetEntityType.Name)!,
            GetForeignKey(
                navigation.ForeignKey, runtimeEntityType.Model.FindEntityType(navigation.ForeignKey.DeclaringEntityType.Name)!),
            navigation.IsCollection,
            navigation.IsOnDependent,
            navigation.ClrType,
            navigation.PropertyInfo,
            navigation.FieldInfo,
            navigation.GetPropertyAccessMode(),
            navigation.IsEagerLoaded,
            navigation.LazyLoadingEnabled);

    /// <summary>
    ///     Gets the corresponding foreign key in the read-optimized model.
    /// </summary>
    /// <param name="foreignKey">The original foreign key.</param>
    /// <param name="entityType">The declaring entity type.</param>
    /// <returns>The corresponding read-optimized foreign key.</returns>
    protected virtual RuntimeForeignKey GetForeignKey(IForeignKey foreignKey, RuntimeEntityType entityType)
        => entityType.FindDeclaredForeignKeys(
                entityType.FindProperties(foreignKey.Properties.Select(p => p.Name))!)
            .Single(
                fk => fk.PrincipalEntityType.Name == foreignKey.PrincipalEntityType.Name
                    && fk.PrincipalKey.Properties.Select(p => p.Name).SequenceEqual(
                        foreignKey.PrincipalKey.Properties.Select(p => p.Name)));

    /// <summary>
    ///     Gets the corresponding key in the read-optimized model.
    /// </summary>
    /// <param name="key">The original key.</param>
    /// <param name="entityType">The declaring entity type.</param>
    /// <returns>The corresponding read-optimized key.</returns>
    protected virtual RuntimeKey GetKey(IKey key, RuntimeEntityType entityType)
        => entityType.FindKey(entityType.FindProperties(key.Properties.Select(p => p.Name))!)!;

    /// <summary>
    ///     Gets the corresponding index in the read-optimized model.
    /// </summary>
    /// <param name="index">The original index.</param>
    /// <param name="entityType">The declaring entity type.</param>
    /// <returns>The corresponding read-optimized index.</returns>
    protected virtual RuntimeIndex GetIndex(IIndex index, RuntimeEntityType entityType)
        => index.Name == null
            ? entityType.FindIndex(entityType.FindProperties(index.Properties.Select(p => p.Name))!)!
            : entityType.FindIndex(index.Name)!;

    /// <summary>
    ///     Updates the skip navigation annotations that will be set on the read-only object.
    /// </summary>
    /// <param name="annotations">The annotations to be processed.</param>
    /// <param name="skipNavigation">The source skip navigation.</param>
    /// <param name="runtimeSkipNavigation">The target skip navigation that will contain the annotations.</param>
    /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
    protected virtual void ProcessSkipNavigationAnnotations(
        Dictionary<string, object?> annotations,
        ISkipNavigation skipNavigation,
        RuntimeSkipNavigation runtimeSkipNavigation,
        bool runtime)
    {
        if (!runtime)
        {
            foreach (var (key, _) in annotations)
            {
                if (CoreAnnotationNames.AllNames.Contains(key))
                {
                    annotations.Remove(key);
                }
            }
        }
    }

    /// <summary>
    ///     A visitor that rewrites <see cref="EntityQueryRootExpression" /> encountered in an expression to use a different entity type.
    /// </summary>
    protected class QueryRootRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;

        /// <summary>
        ///     Creates a new instance of <see cref="QueryRootRewritingExpressionVisitor" />.
        /// </summary>
        /// <param name="model">The model to look for entity types.</param>
        public QueryRootRewritingExpressionVisitor(IModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     Rewrites <see cref="EntityQueryRootExpression" /> encountered in an expression to use a different entity type.
        /// </summary>
        /// <param name="expression">The query expression to rewrite.</param>
        public Expression Rewrite(Expression expression)
            => Visit(expression);

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityQueryRootExpression entityQueryRootExpression
                ? entityQueryRootExpression.UpdateEntityType(_model.FindEntityType(entityQueryRootExpression.EntityType.Name)!)
                : base.VisitExtension(extensionExpression);
    }
}
