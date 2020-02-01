// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd" /> on properties that are
    ///     part of the primary key and not part of any foreign keys or were configured to have a database default value.
    ///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate" /> if they were configured as computed columns.
    /// </summary>
    public class RelationalValueGenerationConvention :
        ValueGenerationConvention,
        IPropertyAnnotationChangedConvention,
        IEntityTypeAnnotationChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalValueGenerationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalValueGenerationConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after an annotation is changed on a property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
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
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == RelationalAnnotationNames.TableName)
            {
                ProcessTableChanged(
                    entityTypeBuilder,
                    (string)oldAnnotation?.Value ?? entityTypeBuilder.Metadata.GetDefaultTableName(),
                    entityTypeBuilder.Metadata.GetSchema());
            }
            else if (name == RelationalAnnotationNames.Schema)
            {
                ProcessTableChanged(
                    entityTypeBuilder,
                    entityTypeBuilder.Metadata.GetTableName(),
                    (string)oldAnnotation?.Value ?? entityTypeBuilder.Metadata.GetDefaultSchema());
            }
        }

        private void ProcessTableChanged(IConventionEntityTypeBuilder entityTypeBuilder, string oldTable, string oldSchema)
        {
            var pk = entityTypeBuilder.Metadata.FindPrimaryKey();
            if (pk == null)
            {
                return;
            }

            var oldLink = pk.Properties.First().FindSharedTableLink(oldTable, oldSchema);
            var newLink = pk.Properties.First().FindSharedTableLink();

            if (oldLink == null
                && newLink == null)
            {
                return;
            }

            foreach (var property in pk.Properties)
            {
                property.Builder.ValueGenerated(GetValueGenerated(property));
            }
        }

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The store value generation strategy to set for the given property. </returns>
        protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
            => GetValueGenerated(property);

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The store value generation strategy to set for the given property. </returns>
        public static new ValueGenerated? GetValueGenerated([NotNull] IProperty property)
        {
            var valueGenerated = ValueGenerationConvention.GetValueGenerated(property);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            return property.GetComputedColumnSql() != null
                ? ValueGenerated.OnAddOrUpdate
                : property.GetDefaultValue() != null || property.GetDefaultValueSql() != null
                    ? ValueGenerated.OnAdd
                    : (ValueGenerated?)null;
        }
    }
}
