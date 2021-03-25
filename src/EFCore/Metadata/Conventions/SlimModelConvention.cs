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
    public class SlimModelConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SlimModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public SlimModelConvention(
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
        protected virtual SlimModel Create(IModel model)
        {
            var slimModel = new SlimModel(model.ModelDependencies!, ((IRuntimeModel)model).SkipDetectChanges);

            var entityTypes = Sort(model.GetEntityTypes());
            var entityTypePairs = new List<(IEntityType Source, SlimEntityType Target)>(entityTypes.Count);

            foreach (var entityType in entityTypes)
            {
                var slimEntityType = Create(entityType, slimModel);
                entityTypePairs.Add((entityType, slimEntityType));

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var slimProperty = Create(property, slimEntityType);
                    CreateAnnotations(property, slimProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessPropertyAnnotations(annotations, source, target, runtime));
                }

                foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
                {
                    var slimServiceProperty = Create(serviceProperty, slimEntityType);
                    CreateAnnotations(serviceProperty, slimServiceProperty, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessServicePropertyAnnotations(annotations, source, target, runtime));
                    slimServiceProperty.ParameterBinding =
                        (ServiceParameterBinding)Create(serviceProperty.ParameterBinding, slimEntityType);
                }

                foreach (var key in entityType.GetDeclaredKeys())
                {
                    var slimKey = Create(key, slimEntityType);
                    if (key.IsPrimaryKey())
                    {
                        slimEntityType.SetPrimaryKey(slimKey);
                    }

                    CreateAnnotations(key, slimKey, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessKeyAnnotations(annotations, source, target, runtime));
                }

                foreach (var index in entityType.GetDeclaredIndexes())
                {
                    var slimIndex = Create(index, slimEntityType);
                    CreateAnnotations(index, slimIndex, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessIndexAnnotations(annotations, source, target, runtime));
                }

                slimEntityType.ConstructorBinding = Create(entityType.ConstructorBinding, slimEntityType);
                slimEntityType.ServiceOnlyConstructorBinding =
                    Create(((IRuntimeEntityType)entityType).ServiceOnlyConstructorBinding, slimEntityType);
            }

            foreach (var (entityType, slimEntityType) in entityTypePairs)
            {
                foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                {
                    var slimForeignKey = Create(foreignKey, slimEntityType);

                    var navigation = foreignKey.DependentToPrincipal;
                    if (navigation != null)
                    {
                        var slimNavigation = Create(navigation, slimForeignKey);
                        CreateAnnotations(navigation, slimNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                    }

                    navigation = foreignKey.PrincipalToDependent;
                    if (navigation != null)
                    {
                        var slimNavigation = Create(navigation, slimForeignKey);
                        CreateAnnotations(navigation, slimNavigation, static (convention, annotations, source, target, runtime) =>
                            convention.ProcessNavigationAnnotations(annotations, source, target, runtime));
                    }

                    CreateAnnotations(foreignKey, slimForeignKey, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessForeignKeyAnnotations(annotations, source, target, runtime));
                }
            }

            foreach (var (entityType, slimEntityType) in entityTypePairs)
            {
                foreach (var navigation in entityType.GetDeclaredSkipNavigations())
                {
                    var slimNavigation = Create(navigation, slimEntityType);

                    var inverse = slimNavigation.TargetEntityType.FindSkipNavigation(navigation.Inverse.Name);
                    if (inverse != null)
                    {
                        slimNavigation.Inverse = inverse;
                        inverse.Inverse = slimNavigation;
                    }

                    CreateAnnotations(navigation, slimNavigation, static (convention, annotations, source, target, runtime) =>
                        convention.ProcessSkipNavigationAnnotations(annotations, source, target, runtime));
                }

                CreateAnnotations(entityType, slimEntityType, static (convention, annotations, source, target, runtime) =>
                    convention.ProcessEntityTypeAnnotations(annotations, source, target, runtime));
            }

            CreateAnnotations(model, slimModel, static (convention, annotations, source, target, runtime) =>
                convention.ProcessModelAnnotations(annotations, source, target, runtime));

            return slimModel;
        }

        private void CreateAnnotations<TSource, TTarget>(
            TSource source,
            TTarget target,
            Action<SlimModelConvention, Dictionary<string, object?>, TSource, TTarget, bool> process)
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
        /// <param name="slimModel"> The target model that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessModelAnnotations(
            Dictionary<string, object?> annotations,
            IModel model,
            SlimModel slimModel,
            bool runtime)
        {
            if (runtime)
            {
                annotations.Remove(CoreAnnotationNames.ModelDependencies);
                annotations[CoreAnnotationNames.ReadOnlyModel] = slimModel;
            }
            else
            {
                annotations.Remove(CoreAnnotationNames.OwnedTypes);
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }
        }

        private static IReadOnlyList<IEntityType> Sort(IEnumerable<IEntityType> entityTypes)
        {
            var entityTypeGraph = new Multigraph<IEntityType, int>();
            entityTypeGraph.AddVertices(entityTypes);
            foreach (var entityType in entityTypes.Where(et => et.BaseType != null))
            {
                entityTypeGraph.AddEdge(entityType.BaseType!, entityType, 0);
            }

            return entityTypeGraph.TopologicalSort();
        }

        private SlimEntityType Create(IEntityType entityType, SlimModel model)
            => model.AddEntityType(entityType.Name,
                entityType.ClrType,
                entityType.HasSharedClrType,
                entityType.BaseType == null ? null : model.FindEntityType(entityType.BaseType.Name)!,
                entityType.GetDiscriminatorPropertyName(),
                entityType.GetChangeTrackingStrategy(),
                entityType.FindIndexerPropertyInfo(),
                entityType.IsPropertyBag);

        private ParameterBinding Create(ParameterBinding parameterBinding, SlimEntityType entityType)
            => parameterBinding.With(parameterBinding.ConsumedProperties.Select(property =>
            (entityType.FindProperty(property.Name)
                ?? entityType.FindServiceProperty(property.Name)
                ?? entityType.FindNavigation(property.Name)
                ?? (IPropertyBase?)entityType.FindSkipNavigation(property.Name))!).ToArray());

        private InstantiationBinding? Create(InstantiationBinding? instantiationBinding, SlimEntityType entityType)
            => instantiationBinding?.With(instantiationBinding.ParameterBindings.Select(binding => Create(binding, entityType)).ToList());

        /// <summary>
        ///     Updates the entity type annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="entityType"> The source entity type. </param>
        /// <param name="slimEntityType"> The target entity type that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessEntityTypeAnnotations(
            IDictionary<string, object?> annotations,
            IEntityType entityType,
            SlimEntityType slimEntityType,
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
                        new QueryRootRewritingExpressionVisitor(slimEntityType.Model).Rewrite((Expression)queryFilter!);
                }

#pragma warning disable CS0612 // Type or member is obsolete
                if (annotations.TryGetValue(CoreAnnotationNames.DefiningQuery, out var definingQuery))
                {
                    annotations[CoreAnnotationNames.DefiningQuery] =
                        new QueryRootRewritingExpressionVisitor(slimEntityType.Model).Rewrite((Expression)definingQuery!);
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        private SlimProperty Create(IProperty property, SlimEntityType slimEntityType)
            => slimEntityType.AddProperty(
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
        /// <param name="slimProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations,
            IProperty property,
            SlimProperty slimProperty,
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
                annotations.Remove(CoreAnnotationNames.ValueConverter);
                annotations.Remove(CoreAnnotationNames.ValueComparer);
            }
        }

        private SlimServiceProperty Create(IServiceProperty property, SlimEntityType slimEntityType)
            => slimEntityType.AddServiceProperty(
                property.Name,
                property.PropertyInfo,
                property.FieldInfo,
                property.GetPropertyAccessMode());

        /// <summary>
        ///     Updates the service property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source service property. </param>
        /// <param name="slimProperty"> The target service property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessServicePropertyAnnotations(
            Dictionary<string, object?> annotations,
            IServiceProperty property,
            SlimServiceProperty slimProperty,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            }
        }

        private SlimKey Create(IKey key, SlimEntityType slimEntityType)
            => slimEntityType.AddKey(slimEntityType.FindProperties(key.Properties.Select(p => p.Name))!);

        /// <summary>
        ///     Updates the key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="key"> The source key. </param>
        /// <param name="slimKey"> The target key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
            IKey key,
            SlimKey slimKey,
            bool runtime)
        {
        }

        private SlimIndex Create(IIndex index, SlimEntityType slimEntityType)
            => slimEntityType.AddIndex(
                slimEntityType.FindProperties(index.Properties.Select(p => p.Name))!,
                index.Name,
                index.IsUnique);

        /// <summary>
        ///     Updates the index annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="index"> The source index. </param>
        /// <param name="slimIndex"> The target index that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessIndexAnnotations(
            Dictionary<string, object?> annotations,
            IIndex index,
            SlimIndex slimIndex,
            bool runtime)
        {
        }

        private SlimForeignKey Create(IForeignKey foreignKey, SlimEntityType slimEntityType)
        {
            var principalEntityType = slimEntityType.Model.FindEntityType(foreignKey.PrincipalEntityType.Name)!;
            return slimEntityType.AddForeignKey(
                slimEntityType.FindProperties(foreignKey.Properties.Select(p => p.Name))!,
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
        /// <param name="slimForeignKey"> The target foreign key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessForeignKeyAnnotations(
            Dictionary<string, object?> annotations,
            IForeignKey foreignKey,
            SlimForeignKey slimForeignKey,
            bool runtime)
        {
        }

        private SlimNavigation Create(INavigation navigation, SlimForeignKey slimForeigKey)
            => (navigation.IsOnDependent ? slimForeigKey.DeclaringEntityType : slimForeigKey.PrincipalEntityType)
                .AddNavigation(
                    navigation.Name,
                    navigation.ClrType,
                    navigation.PropertyInfo,
                    navigation.FieldInfo,
                    slimForeigKey,
                    navigation.IsOnDependent,
                    navigation.GetPropertyAccessMode(),
                    navigation.IsEagerLoaded);

        /// <summary>
        ///     Updates the navigation annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="navigation"> The source navigation. </param>
        /// <param name="slimNavigation"> The target navigation that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessNavigationAnnotations(
            Dictionary<string, object?> annotations,
            INavigation navigation,
            SlimNavigation slimNavigation,
            bool runtime)
        {
            if (!runtime)
            {
                annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
                annotations.Remove(CoreAnnotationNames.EagerLoaded);
            }
        }

        private SlimSkipNavigation Create(ISkipNavigation navigation, SlimEntityType slimEntityType)
            => slimEntityType.AddSkipNavigation(
                navigation.Name,
                navigation.ClrType,
                navigation.PropertyInfo,
                navigation.FieldInfo,
                slimEntityType.Model.FindEntityType(navigation.TargetEntityType.Name)!,
                GetForeignKey(navigation.ForeignKey, slimEntityType.Model.FindEntityType(navigation.ForeignKey.DeclaringEntityType.Name)!),
                navigation.IsCollection,
                navigation.IsOnDependent,
                navigation.GetPropertyAccessMode(),
                navigation.IsEagerLoaded);

        /// <summary>
        ///     Gets the corresponding foreign key in the read-optimized model.
        /// </summary>
        /// <param name="foreignKey"> The original foreign key. </param>
        /// <param name="entityType"> The declaring entity type. </param>
        /// <returns> The corresponding read-optimized foreign key. </returns>
        protected virtual SlimForeignKey GetForeignKey(IForeignKey foreignKey, SlimEntityType entityType)
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
        protected virtual SlimKey GetKey(IKey key, SlimEntityType entityType)
            => entityType.FindKey(entityType.FindProperties(key.Properties.Select(p => p.Name))!)!;

        /// <summary>
        ///     Gets the corresponding index in the read-optimized model.
        /// </summary>
        /// <param name="index"> The original index. </param>
        /// <param name="entityType"> The declaring entity type. </param>
        /// <returns> The corresponding read-optimized index. </returns>
        protected virtual SlimIndex GetIndex(IIndex index, SlimEntityType entityType)
            => index.Name == null
            ? entityType.FindIndex(entityType.FindProperties(index.Properties.Select(p => p.Name))!)!
            : entityType.FindIndex(index.Name)!;

        /// <summary>
        ///     Updates the skip navigation annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="skipNavigation"> The source skip navigation. </param>
        /// <param name="slimSkipNavigation"> The target skip navigation that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected virtual void ProcessSkipNavigationAnnotations(
            Dictionary<string, object?> annotations,
            ISkipNavigation skipNavigation,
            SlimSkipNavigation slimSkipNavigation,
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
