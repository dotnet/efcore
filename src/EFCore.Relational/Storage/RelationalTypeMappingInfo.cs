// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Describes metadata needed to decide on a relational type mapping for
    ///     a property, type, or provider-specific relational type name.
    /// </summary>
    public abstract class RelationalTypeMappingInfo : TypeMappingInfo
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="property"> The property for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] IProperty property)
            : base(property)
        {
            StoreTypeName = property
                .FindPrincipals()
                .Select(p => (string)p[RelationalAnnotationNames.ColumnType])
                .FirstOrDefault(t => t != null);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] Type type)
            : base(type)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="storeTypeName"> The provider-specific relational type name for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] string storeTypeName)
        {
            Check.NotEmpty(storeTypeName, nameof(storeTypeName));

            StoreTypeName = storeTypeName;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        protected RelationalTypeMappingInfo([NotNull] MemberInfo member)
            : base(member)
        {
            Check.NotNull(member, nameof(member));

            var attribute = member.GetCustomAttributes<ColumnAttribute>(true)?.FirstOrDefault();
            if (attribute != null)
            {
                StoreTypeName = attribute.TypeName;
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
        /// </summary>
        /// <param name="source"> The source info. </param>
        /// <param name="builtInConverter"> The converter to apply. </param>
        protected RelationalTypeMappingInfo(
            [NotNull] RelationalTypeMappingInfo source,
            ValueConverterInfo builtInConverter)
            : base(source, builtInConverter)
        {
            StoreTypeName = source.StoreTypeName;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or Ansi mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        protected RelationalTypeMappingInfo(
            [NotNull] Type type,
            bool keyOrIndex,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
            : base(type, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale)
        {
        }

        /// <summary>
        ///     The provider-specific relational type name for which mapping is needed.
        /// </summary>
        public virtual string StoreTypeName { get; }

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        protected virtual bool Equals([NotNull] RelationalTypeMappingInfo other)
            => Equals((TypeMappingInfo)other)
               && StoreTypeName == other.StoreTypeName;

        /// <summary>
        ///     Compares this <see cref="RelationalTypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((RelationalTypeMappingInfo)obj));

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
            => (base.GetHashCode() * 397) ^ (StoreTypeName?.GetHashCode() ?? 0);
    }
}
