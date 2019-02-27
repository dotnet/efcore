// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class ValueGeneratorConvention :
        IPrimaryKeyChangedConvention,
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IBaseTypeChangedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ValueGeneratorConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                property.Builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            foreach (var property in foreignKey.Properties)
            {
                var pk = property.PrimaryKey;
                if (pk == null)
                {
                    property.Builder?.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                }
                else
                {
                    foreach (Property keyProperty in pk.Properties)
                    {
                        keyProperty.Builder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
        {
            if (previousPrimaryKey != null)
            {
                foreach (var property in previousPrimaryKey.Properties)
                {
                    property.Builder?.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
                }
            }

            var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
            if (primaryKey != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    property.Builder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            foreach (var property in entityTypeBuilder.Metadata.GetProperties())
            {
                property.Builder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerated? GetValueGenerated([NotNull] Property property)
            => !property.IsForeignKey()
               && property.PrimaryKey?.Properties.Count(p => !p.IsForeignKey()) == 1
               && CanBeGenerated(property)
                ? ValueGenerated.OnAdd
                : (ValueGenerated?)null;

        /// <summary>
        ///     Indicates whether the specified property can have the value generated by the store or by a non-temporary value generator
        ///     when not set.
        /// </summary>
        /// <param name="property"> The key property that might be store generated. </param>
        /// <returns> A value indicating whether the specified property should have the value generated by the store. </returns>
        private static bool CanBeGenerated(Property property)
        {
            var propertyType = property.ClrType.UnwrapNullableType();
            return (propertyType.IsInteger()
                    && propertyType != typeof(byte))
                   || propertyType == typeof(Guid)
                ? true
                : false;
        }
    }
}
