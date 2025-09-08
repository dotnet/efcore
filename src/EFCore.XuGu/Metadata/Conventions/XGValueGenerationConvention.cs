// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures store value generation as <see cref="ValueGenerated.OnAdd"/> on properties that are
    ///     part of the primary key and not part of any foreign keys, were configured to have a database default value
    ///     or were configured to use a <see cref="XGValueGenerationStrategy"/>.
    ///     It also configures properties as <see cref="ValueGenerated.OnAddOrUpdate"/> if they were configured as computed columns.
    /// </summary>
    public class XGValueGenerationConvention : RelationalValueGenerationConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="XGValueGenerationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public XGValueGenerationConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
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
        public override void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == XGAnnotationNames.ValueGenerationStrategy)
            {
                propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
                return;
            }

            base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
        }

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The store value generation strategy to set for the given property. </returns>
        protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
        {
            var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
            if (declaringTable.Name == null)
            {
                return null;
            }

            // If the first mapping can be value generated then we'll consider all mappings to be value generated
            // as this is a client-side configuration and can't be specified per-table.
            return GetValueGenerated(property, declaringTable, Dependencies.TypeMappingSource);
        }

        /// <summary>
        ///     Returns the store value generation strategy to set for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The store value generation strategy to set for the given property. </returns>
        public static new ValueGenerated? GetValueGenerated(
            [NotNull] IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var valueGenerated = RelationalValueGenerationConvention.GetValueGenerated(property, storeObject);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            return property.GetValueGenerationStrategy(storeObject) switch
            {
                XGValueGenerationStrategy.IdentityColumn => ValueGenerated.OnAdd,
                XGValueGenerationStrategy.ComputedColumn => ValueGenerated.OnAddOrUpdate,
                _ => null
            };
        }

        private ValueGenerated? GetValueGenerated(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource typeMappingSource)
        {
            var valueGenerated = RelationalValueGenerationConvention.GetValueGenerated(property, storeObject);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            return property.GetValueGenerationStrategy(storeObject, typeMappingSource) switch
            {
                XGValueGenerationStrategy.IdentityColumn => ValueGenerated.OnAdd,
                XGValueGenerationStrategy.ComputedColumn => ValueGenerated.OnAddOrUpdate,
                _ => null
            };
        }
    }
}
