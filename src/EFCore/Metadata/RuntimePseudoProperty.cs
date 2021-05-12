// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a property mapped to the database, but fully accessed through an outer property.
    ///     For relational databases, pseudo-properties represent the columns in the database when a single
    ///     property maps to multiple columns.
    /// </summary>
    public class RuntimePseudoProperty : RuntimeProperty, IPseudoProperty
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimePseudoProperty(
            string name,
            Type clrType,
            PropertyInfo? propertyInfo,
            FieldInfo? fieldInfo,
            RuntimeProperty outerProperty,
            Func<object?, object?> valueExtractor,
            RuntimeEntityType declaringEntityType,
            PropertyAccessMode propertyAccessMode,
            bool nullable,
            bool concurrencyToken,
            ValueGenerated valueGenerated,
            PropertySaveBehavior beforeSaveBehavior,
            PropertySaveBehavior afterSaveBehavior,
            int? maxLength,
            bool? unicode,
            int? precision,
            int? scale,
            Type? providerClrType,
            Func<IProperty, IEntityType, ValueGenerator>? valueGeneratorFactory,
            ValueConverter? valueConverter,
            ValueComparer? valueComparer,
            ValueComparer? keyValueComparer,
            CoreTypeMapping? typeMapping)
            : base(
                name, clrType, propertyInfo, fieldInfo, declaringEntityType, propertyAccessMode, nullable, concurrencyToken, valueGenerated,
                beforeSaveBehavior, afterSaveBehavior, maxLength, unicode, precision, scale, providerClrType, valueGeneratorFactory,
                valueConverter, valueComparer, keyValueComparer, typeMapping)
        {
            OuterProperty = outerProperty;
            ValueExtractor = valueExtractor;
        }

        /// <summary>
        ///     The outer property used to access values of this pseudo-property.
        /// </summary>
        public virtual IProperty OuterProperty { get; }

        /// <summary>
        ///     A delegate that accepts a value from the <see cref="OuterProperty"/> and generates the value
        ///     for this pseudo-property.
        /// </summary>
        public virtual Func<object?, object?> ValueExtractor { get; }

        /// <summary>
        ///     Checks whether or not this property is an <see cref="IsPseudoProperty"/>.
        /// </summary>
        /// <returns> <see langword="true" />. </returns>
        public override bool IsPseudoProperty
            => true;
    }
}
