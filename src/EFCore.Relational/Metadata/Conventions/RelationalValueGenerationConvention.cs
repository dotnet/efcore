// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
    ///     part of the primary key and not part of any foreign keys or were configured to have a database default value.
    ///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
    ///     <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
    /// </remarks>
    public class RelationalValueGenerationConvention :
        ValueGenerationConvention,
        IPropertyAnnotationChangedConvention,
        IEntityTypeAnnotationChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalValueGenerationConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
        public RelationalValueGenerationConvention(
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
        ///     Called after an annotation is changed on a property.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property.</param>
        /// <param name="name">The annotation name.</param>
        /// <param name="annotation">The new annotation.</param>
        /// <param name="oldAnnotation">The old annotation.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            var property = propertyBuilder.Metadata;
            switch (name)
            {
                case RelationalAnnotationNames.DefaultValue:
                case RelationalAnnotationNames.DefaultValueSql:
                case RelationalAnnotationNames.ComputedColumnSql:
                    propertyBuilder.ValueGenerated(GetValueGenerated(property));
                    break;
            }
        }

        /// <summary>
        ///     Called after an annotation is changed on an entity type.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type.</param>
        /// <param name="name">The annotation name.</param>
        /// <param name="annotation">The new annotation.</param>
        /// <param name="oldAnnotation">The old annotation.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == RelationalAnnotationNames.TableName)
            {
                var schema = entityTypeBuilder.Metadata.GetSchema();
                ProcessTableChanged(
                    entityTypeBuilder,
                    (string?)oldAnnotation?.Value ?? entityTypeBuilder.Metadata.GetDefaultTableName(),
                    schema,
                    entityTypeBuilder.Metadata.GetTableName(),
                    schema);
            }
            else if (name == RelationalAnnotationNames.Schema)
            {
                var tableName = entityTypeBuilder.Metadata.GetTableName();
                ProcessTableChanged(
                    entityTypeBuilder,
                    tableName,
                    (string?)oldAnnotation?.Value ?? entityTypeBuilder.Metadata.GetDefaultSchema(),
                    tableName,
                    entityTypeBuilder.Metadata.GetSchema());
            }
        }

        private void ProcessTableChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string? oldTable,
            string? oldSchema,
            string? newTable,
            string? newSchema)
        {
            var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
            if (primaryKey == null)
            {
                return;
            }

            var oldLink = oldTable != null
                ? entityTypeBuilder.Metadata.FindRowInternalForeignKeys(StoreObjectIdentifier.Table(oldTable, oldSchema))
                : null;
            var newLink = newTable != null
                ? entityTypeBuilder.Metadata.FindRowInternalForeignKeys(StoreObjectIdentifier.Table(newTable, newSchema))
                : null;

            if ((oldLink?.Any() != true
                    && newLink?.Any() != true)
                || newLink == null)
            {
                return;
            }

            foreach (var property in primaryKey.Properties)
            {
                property.Builder.ValueGenerated(GetValueGenerated(property, StoreObjectIdentifier.Table(newTable!, newSchema)));
            }
        }

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The store value generation strategy to set for the given property.</returns>
        protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
        {
            var tableName = property.DeclaringEntityType.GetTableName();
            if (tableName == null)
            {
                return null;
            }

            return GetValueGenerated(property, StoreObjectIdentifier.Table(tableName, property.DeclaringEntityType.GetSchema()));
        }

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The new store value generation strategy to set for the given property.</returns>
        public static ValueGenerated? GetValueGenerated(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var valueGenerated = GetValueGenerated(property);
            return valueGenerated
                ?? (property.GetComputedColumnSql(storeObject) != null
                    ? ValueGenerated.OnAddOrUpdate
                    : property.TryGetDefaultValue(storeObject, out _) || property.GetDefaultValueSql(storeObject) != null
                        ? ValueGenerated.OnAdd
                        : null);
        }
    }
}
