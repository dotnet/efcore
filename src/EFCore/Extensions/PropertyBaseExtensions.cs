// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IPropertyBase" />.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> that should be used to
        ///         get or set a value for the given property.
        ///     </para>
        ///     <para>
        ///         Note that it is an error to call this method for a shadow property (<see cref="IsShadowProperty" />) since
        ///         such a property has no associated <see cref="MemberInfo" />.
        ///     </para>
        /// </summary>
        /// <param name="propertyBase"> The property. </param>
        /// <param name="forMaterialization"> If true, then the member to use for query materialization will be returned. </param>
        /// <param name="forSet">
        ///     If true, then the member to use for setting the property value will be returned, otherwise
        ///     the member to use for getting the property value will be returned.
        /// </param>
        /// <returns> The <see cref="MemberInfo" /> to use. </returns>
        public static MemberInfo GetMemberInfo(
            [NotNull] this IPropertyBase propertyBase,
            bool forMaterialization,
            bool forSet)
        {
            if (propertyBase.TryGetMemberInfo(forMaterialization, forSet, out var memberInfo, out var errorMessage))
            {
                return memberInfo;
            }

            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        ///     <para>
        ///         Gets a <see cref="IClrPropertyGetter" /> for reading the value of this property.
        ///     </para>
        ///     <para>
        ///         Note that it is an error to call this method for a shadow property (<see cref="IsShadowProperty" />) since
        ///         such a property has no associated <see cref="MemberInfo" />.
        ///     </para>
        /// </summary>
        /// <param name="propertyBase"> The property. </param>
        /// <returns> The accessor. </returns>
        public static IClrPropertyGetter GetGetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Getter;

        /// <summary>
        ///     Gets the name of the backing field for this property, or <c>null</c> if the backing field
        ///     is not known.
        /// </summary>
        /// <param name="propertyBase"> The property for which the backing field will be returned. </param>
        /// <returns> The name of the backing field, or <c>null</c>. </returns>
        public static string GetFieldName([NotNull] this IPropertyBase propertyBase)
            => propertyBase.FieldInfo?.GetSimpleMemberName();

        /// <summary>
        ///     Gets a value indicating whether this is a shadow property. A shadow property is one that does not have a
        ///     corresponding property in the entity class. The current value for the property is stored in
        ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>True</c> if the property is a shadow property, otherwise <c>false</c>.
        /// </returns>
        public static bool IsShadowProperty([NotNull] this IPropertyBase property)
            => Check.NotNull(property, nameof(property)).GetIdentifyingMemberInfo() == null;

        /// <summary>
        ///     Gets a value indicating whether this is an indexer property. An indexer property is one that is accessed through
        ///     an indexer on the entity class.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>True</c> if the property is an indexer property, otherwise <c>false</c>.
        /// </returns>
        public static bool IsIndexerProperty([NotNull] this IPropertyBase property)
            => Check.NotNull(property, nameof(property)).GetIdentifyingMemberInfo() is PropertyInfo propertyInfo
                && propertyInfo == property.DeclaringType.FindIndexerPropertyInfo();
    }
}
