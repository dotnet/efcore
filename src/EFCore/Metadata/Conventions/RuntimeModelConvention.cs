// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates an optimized copy of the mutable model. This convention is typically
    ///     implemented by database providers to update provider annotations when creating a read-only model.
    /// </summary>
    public class RuntimeModelConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RuntimeModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public RuntimeModelConvention(
            ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is finalized and can no longer be mutated.
        /// </summary>
        /// <param name="model"> The model. </param>
        public virtual IModel ProcessModelFinalized(IModel model)
            => Create(model);

        /// <summary>
        ///     Creates an optimized model base on the supplied one.
        /// </summary>
        /// <param name="model"> The source model. </param>
        /// <returns> An optimized model. </returns>
        protected virtual RuntimeModel Create(IModel model)
        {
            var runtimeModel = new RuntimeModel();
            runtimeModel.SetSkipDetectChanges(((IRuntimeModel)model).SkipDetectChanges);
            ((IModel)slimModel).ModelDependencies = model.ModelDependencies!;

            var entityTypes = model.GetEntityTypesInHierarchicalOrder();
            var entityTypePairs = new List<(IEntityType Source, RuntimeEntityType Target)>(entityTypes.Count);

            foreach (var entityType in entityTypes)
            {
                var runtimeEntityType = Create(entityType, runtimeModel);
                entityTypePairs.Add((entityType, runtimeEntityType));

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var runtimeProperty = Create(property, runtimeEntityType);
                    CreateAnnotations(property, runtimeProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessPropertyAnnotations(annotations, source, target, runtime));
                }

                foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
                {
                    var runtimeServiceProperty = Create(serviceProperty, runtimeEntityType);
                    CreateAnnotations(serviceProperty, runtimeServiceProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessServicePropertyAnnotations(annotations, source, target, runtime));
                    runtimeServiceProperty.ParameterBinding =
                        (ServiceParameterBinding)Create(serviceProperty.ParameterBinding, runtimeEntityType);
                }

                foreach (var key in entityType.GetDeclaredKeys())
                {
                    var runtimeKey = Create(key, runtimeEntityType);
                    if (key.IsPrimaryKey())
                    {
                        runtimeEntityType.SetPrimaryKey(runtimeKey);
                    }

                    CreateAnnotations(key, runtimeKey, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessKeyAnnotations(annotations, source, target, runtime));
                }

                foreach (var index in entityType.GetDeclaredIndexes())
                {
                    var runtimeIndex = Create(index, runtimeEntityType);
                    CreateAnnotations(index, runtimeIndex, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessIndexAnnotations(annotations, source, target, runtime));
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
                        CreateAnnotations(navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                    }

                    navigation = foreignKey.PrincipalToDependent;
                    if (navigation != null)
                    {
                        var runtimeNavigation = Create(navigation, runtimeForeignKey);
                        CreateAnnotations(navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                    }

                    CreateAnnotations(foreignKey, runtimeForeignKey, static (convention, annotations, source, target, runtime) =>
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

                    CreateAnnotations(navigation, runtimeNavigation, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessSkipNavigationAnnotations(annotations, source, target, runtime));
                }

                CreateAnnotations(entityType, runtimeEntityType, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessEntityTypeAnnotations(annotations, source, target, runtime));
            }

            CreateAnnotations(model, runtimeModel, static (convention, annotations, source, target, runtime) =>
                convention.ProcessModelAnnotations(annotations, source, target, runtime));

            return runtimeModel;
        }

        private void CreateAnnotations<TSource, TTarget>(
            TSource source,
            TTarget target,
            Action<RuntimeModelConvention, Dictionary<string, object?>, TSource, TTarget, bool> process)
            where TSource : IAnnotatable
            where TTarget : AnnotatableBase
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
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="model"> The source model. </param>
        /// <param name="runtimeModel"> The target model that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessModelAnnotations(
            Dictionary<string, object?> annotations,
            IModel model,
            RuntimeModel runtimeModel,
            bool runtime)
        {
            if (runtime)
            {
                annotations.Remove(CoreAnnotationNames.ModelDependencies);
                annotations[CoreAnnotationNames.ReadOnlyModel] = runtimeModel;
            }
            else
            {
                annotations.Remove(CoreAnnotationNames.OwnedTypes);
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }
        }

        private RuntimeEntityType Create(IEntityType entityType, RuntimeModel model)
            => model.AddEntityType(entityType.Name,
                entityType.ClrType,
                entityType.BaseType == null ? null : model.FindEntityType(entityType.BaseType.Name)!,
                entityType.HasSharedClrType,
                entityType.GetDiscriminatorPropertyName(),
                entityType.GetChangeTrackingStrategy(),
                entityType.FindIndexerPropertyInfo(),
                entityType.IsPropertyBag);

        private ParameterBinding Create(ParameterBinding parameterBinding, RuntimeEntityType entityType)
            => parameterBinding.With(parameterBinding.ConsumedProperties.Select(property =>
            (entityType.FindProperty(property.Name)
                ?? entityType.FindServiceProperty(property.Name)
                ?? entityType.FindNavigation(property.Name)
                ?? (IPropertyBase?)entityType.FindSkipNavigation(property.Name))!).ToArray());

        private InstantiationBinding? Create(InstantiationBinding? instantiationBinding, RuntimeEntityType entityType)
            => instantiationBinding?.With(instantiationBinding.ParameterBindings.Select(binding => Create(binding, entityType)).ToList());

        /// <summary>
        ///     Updates the entity type annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="entityType"> The source entity type. </param>
        /// <param name="runtimeEntityType"> The target entity type that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessEntityTypeAnnotations(
            IDictionary<string, object?> annotations,
            IEntityType entityType,
            RuntimeEntityType runtimeEntityType,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.NavigationAccessMode);
                annotations.Remove(CoreAnnotationNames.DiscriminatorProperty);

                if (annotations.TryGetValue(CoreAnnotationNames.QueryFilter, out var queryFilter))
                {
                    annotations[CoreAnnotationNames.QueryFilter] =
                        new QueryRootRewritingExpressionVisitor(runtimeEntityType.Model).Rewrite((Expression)queryFilter!);
                }

#pragma warning disable CS0612 // Type or member is obsolete
                if (annotations.TryGetValue(CoreAnnotationNames.DefiningQuery, out var definingQuery))
                {
                    annotations[CoreAnnotationNames.DefiningQuery] =
                        new QueryRootRewritingExpressionVisitor(runtimeEntityType.Model).Rewrite((Expression)definingQuery!);
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        private RuntimeProperty Create(IProperty property, RuntimeEntityType runtimeEntityType)
            => runtimeEntityType.AddProperty(
                property.Name,
                property.ClrType,
                property.PropertyInfo,
                property.FieldInfo,
                property.GetPropertyAccessMode(),
                property.IsNullable,
                property.IsConcurrencyToken,
                property.ValueGenerated,
                property.GetBeforeSaveBehavior(),
                property.GetAfterSaveBehavior(),
                property.GetMaxLength(),
                property.IsUnicode(),
                property.GetPrecision(),
                property.GetScale(),
                property.GetProviderClrType(),
                property.GetValueGeneratorFactory(),
                property.GetValueConverter(),
                property.GetValueComparer(),
                property.GetKeyValueComparer(),
                property.GetTypeMapping());

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source property. </param>
        /// <param name="runtimeProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations,
            IProperty property,
            RuntimeProperty runtimeProperty,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.BeforeSaveBehavior);
                annotations.Remove(CoreAnnotationNames.AfterSaveBehavior);
                annotations.Remove(CoreAnnotationNames.MaxLength);
                annotations.Remove(CoreAnnotationNames.Unicode);
                annotations.Remove(CoreAnnotationNames.Precision);
                annotations.Remove(CoreAnnotationNames.Scale);
                annotations.Remove(CoreAnnotationNames.ProviderClrType);
                annotations.Remove(CoreAnnotationNames.ValueGeneratorFactory);
                annotations.Remove(CoreAnnotationNames.ValueGeneratorFactoryType);
                annotations.Remove(CoreAnnotationNames.ValueConverter);
                annotations.Remove(CoreAnnotationNames.ValueConverterType);
                annotations.Remove(CoreAnnotationNames.ValueComparer);
                annotations.Remove(CoreAnnotationNames.ValueComparerType);
            }
        }

        private RuntimeServiceProperty Create(IServiceProperty property, RuntimeEntityType runtimeEntityType)
            => runtimeEntityType.AddServiceProperty(
                property.Name,
                property.PropertyInfo,
                property.FieldInfo,
                property.GetPropertyAccessMode());

        /// <summary>
        ///     Updates the service property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source service property. </param>
        /// <param name="runtimeProperty"> The target service property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessServicePropertyAnnotations(
            Dictionary<string, object?> annotations,
            IServiceProperty property,
            RuntimeServiceProperty runtimeProperty,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }
        }

        private RuntimeKey Create(IKey key, RuntimeEntityType runtimeEntityType)
            => runtimeEntityType.AddKey(runtimeEntityType.FindProperties(key.Properties.Select(p => p.Name))!);

        /// <summary>
        ///     Updates the key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="key"> The source key. </param>
        /// <param name="runtimeKey"> The target key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
            IKey key,
            RuntimeKey runtimeKey,
            bool runtime)
        {
        }

        private RuntimeIndex Create(IIndex index, RuntimeEntityType runtimeEntityType)
            => runtimeEntityType.AddIndex(
                runtimeEntityType.FindProperties(index.Properties.Select(p => p.Name))!,
                index.Name,
                index.IsUnique);

        /// <summary>
        ///     Updates the index annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="index"> The source index. </param>
        /// <param name="runtimeIndex"> The target index that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessIndexAnnotations(
            Dictionary<string, object?> annotations,
            IIndex index,
            RuntimeIndex runtimeIndex,
            bool runtime)
        {
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

        /// <summary>
        ///     Updates the foreign key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="foreignKey"> The source foreign key. </param>
        /// <param name="runtimeForeignKey"> The target foreign key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessForeignKeyAnnotations(
            Dictionary<string, object?> annotations,
            IForeignKey foreignKey,
            RuntimeForeignKey runtimeForeignKey,
            bool runtime)
        {
        }

        private RuntimeNavigation Create(INavigation navigation, RuntimeForeignKey runtimeForeigKey)
            => (navigation.IsOnDependent ? runtimeForeigKey.DeclaringEntityType : runtimeForeigKey.PrincipalEntityType)
                .AddNavigation(
                    navigation.Name,
                    slimForeigKey,
                    navigation.IsOnDependent,
                    navigation.ClrType,
                    navigation.PropertyInfo,
                    navigation.FieldInfo,
                    navigation.GetPropertyAccessMode(),
                    navigation.IsEagerLoaded);

        /// <summary>
        ///     Updates the navigation annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="navigation"> The source navigation. </param>
        /// <param name="runtimeNavigation"> The target navigation that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessNavigationAnnotations(
            Dictionary<string, object?> annotations,
            INavigation navigation,
            RuntimeNavigation runtimeNavigation,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.EagerLoaded);
            }
        }

        private RuntimeSkipNavigation Create(ISkipNavigation navigation, RuntimeEntityType runtimeEntityType)
            => runtimeEntityType.AddSkipNavigation(
                navigation.Name,
                runtimeEntityType.Model.FindEntityType(navigation.TargetEntityType.Name)!,
                GetForeignKey(navigation.ForeignKey, runtimeEntityType.Model.FindEntityType(navigation.ForeignKey.DeclaringEntityType.Name)!),
                navigation.IsCollection,
                navigation.IsOnDependent,
                navigation.ClrType,
                navigation.PropertyInfo,
                navigation.FieldInfo,
                navigation.GetPropertyAccessMode(),
                navigation.IsEagerLoaded);

        /// <summary>
        ///     Gets the corresponding foreign key in the read-optimized model.
        /// </summary>
        /// <param name="foreignKey"> The original foreign key. </param>
        /// <param name="entityType"> The declaring entity type. </param>
        /// <returns> The corresponding read-optimized foreign key. </returns>
        protected virtual RuntimeForeignKey GetForeignKey(IForeignKey foreignKey, RuntimeEntityType entityType)
            => entityType.FindDeclaredForeignKeys(
                entityType.FindProperties(foreignKey.Properties.Select(p => p.Name))!)
                .Single(fk => fk.PrincipalEntityType.Name == foreignKey.PrincipalEntityType.Name
                && fk.PrincipalKey.Properties.Select(p => p.Name).SequenceEqual(
                    foreignKey.PrincipalKey.Properties.Select(p => p.Name)));

        /// <summary>
        ///     Gets the corresponding key in the read-optimized model.
        /// </summary>
        /// <param name="key"> The original key. </param>
        /// <param name="entityType"> The declaring entity type. </param>
        /// <returns> The corresponding read-optimized key. </returns>
        protected virtual RuntimeKey GetKey(IKey key, RuntimeEntityType entityType)
            => entityType.FindKey(entityType.FindProperties(key.Properties.Select(p => p.Name))!)!;

        /// <summary>
        ///     Gets the corresponding index in the read-optimized model.
        /// </summary>
        /// <param name="index"> The original index. </param>
        /// <param name="entityType"> The declaring entity type. </param>
        /// <returns> The corresponding read-optimized index. </returns>
        protected virtual RuntimeIndex GetIndex(IIndex index, RuntimeEntityType entityType)
            => index.Name == null
            ? entityType.FindIndex(entityType.FindProperties(index.Properties.Select(p => p.Name))!)!
            : entityType.FindIndex(index.Name)!;

        /// <summary>
        ///     Updates the skip navigation annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="skipNavigation"> The source skip navigation. </param>
        /// <param name="runtimeSkipNavigation"> The target skip navigation that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessSkipNavigationAnnotations(
            Dictionary<string, object?> annotations,
            ISkipNavigation skipNavigation,
            RuntimeSkipNavigation runtimeSkipNavigation,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.EagerLoaded);
            }
        }

        /// <summary>
        ///     A visitor that rewrites <see cref="QueryRootExpression" /> encountered in an expression to use a different entity type.
        /// </summary>
        protected class QueryRootRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly IModel _model;

            /// <summary>
            ///     Creates a new instance of <see cref="QueryRootRewritingExpressionVisitor" />.
            /// </summary>
            /// <param name="model"> The model to look for entity types. </param>
            public QueryRootRewritingExpressionVisitor(IModel model)
            {
                _model = model;
            }

            /// <summary>
            ///     Rewrites <see cref="QueryRootExpression" /> encountered in an expression to use a different entity type.
            /// </summary>
            /// <param name="expression"> The query expression to rewrite. </param>
            public Expression Rewrite(Expression expression)
                => Visit(expression);

            /// <inheritdoc />
            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is QueryRootExpression queryRootExpression
                    ? new QueryRootExpression(_model.FindEntityType(queryRootExpression.EntityType.Name)!)
                    : base.VisitExtension(extensionExpression);
        }
    }
}
