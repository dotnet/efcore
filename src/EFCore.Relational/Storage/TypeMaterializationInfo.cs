// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Associates a <see cref="RelationalTypeMapping" /> with an optional <see cref="IProperty" />
    ///     and an index into the data reader for use when reading and converting values from the database.
    /// </summary>
    public class TypeMaterializationInfo
    {
        /// <summary>
        ///     Creates a new <see cref="TypeMaterializationInfo" /> instance.
        /// </summary>
        /// <param name="modelType"> The type that is needed in the model after conversion. </param>
        /// <param name="property"> The property associated with the type, or <c>null</c> if none. </param>
        /// <param name="typeMapper"> The type mapper to use to find a mapping if the property does not have one already bound. </param>
        /// <param name="index">
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </param>
        public TypeMaterializationInfo(
            [NotNull] Type modelType,
            [CanBeNull] IProperty property,
            [CanBeNull] IRelationalTypeMapper typeMapper, 
            int index = -1)
        {
            Check.NotNull(modelType, nameof(modelType));

            var mapping = property?.FindRelationalMapping()
                      ?? typeMapper?.GetMapping(modelType);

            StoreType = mapping?.Converter?.StoreType
                        ?? modelType.UnwrapNullableType().UnwrapEnumType();

            ModelType = modelType;
            Mapping = mapping;
            Property = property;
            Index = index;
        }

        /// <summary>
        ///     The type that will be read from the store.
        /// </summary>
        public virtual Type StoreType { get; }

        /// <summary>
        ///     The type that is needed in the model after conversion.
        /// </summary>
        public virtual Type ModelType { get; }

        /// <summary>
        ///     The type mapping for the value to be read.
        /// </summary>
        public virtual RelationalTypeMapping Mapping { get; }

        /// <summary>
        ///     The property associated with the type, or <c>null</c> if none.
        /// </summary>
        public virtual IProperty Property { get; }

        /// <summary>
        ///     The index of the underlying result set that should be used for this type,
        ///     or -1 if no index mapping is needed.
        /// </summary>
        public virtual int Index { get; }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object. </param>
        /// <returns> <c>True</c> if the specified object is equal to the current object; otherwise, <c>false</c>. </returns>
        protected virtual bool Equals([NotNull] TypeMaterializationInfo other)
            => StoreType == other.StoreType
               && ModelType == other.ModelType
               && Equals(Mapping, other.Mapping)
               && Equals(Property, other.Property)
               && Index == other.Index;

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <c>True</c> if the specified object is equal to the current object; otherwise, <c>false</c>. </returns>
        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((TypeMaterializationInfo)obj));

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StoreType.GetHashCode();
                hashCode = (hashCode * 397) ^ ModelType.GetHashCode();
                hashCode = (hashCode * 397) ^ (Mapping?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Property?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Index;
                return hashCode;
            }
        }
    }
}
