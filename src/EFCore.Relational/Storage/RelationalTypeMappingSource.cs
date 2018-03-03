// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The base class for non-relational type mapping starting with version 2.1. Non-relational providers
    ///         should derive from this class and override <see cref="FindMapping(RelationalTypeMappingInfo)" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class RelationalTypeMappingSource : TypeMappingSource, IRelationalTypeMappingSource
    {
        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational-specific dependencies for this service. </param>
        protected RelationalTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     <para>
        ///         Overridden by relational database providers to find a type mapping for the given info.
        ///     </para>
        ///     <para>
        ///         The mapping info is populated with as much information about the required type mapping as
        ///         is available. Use all the information necessary to create the best mapping. Return <c>null</c>
        ///         if no mapping is available.
        ///     </para>
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <c>null</c> if none could be found. </returns>
        protected abstract RelationalTypeMapping FindMapping([NotNull] RelationalTypeMappingInfo mappingInfo);

        /// <summary>
        ///     Overridden to call <see cref="FindMapping(RelationalTypeMappingInfo)" />
        /// </summary>
        /// <param name="mappingInfo"> The mapping info to use to create the mapping. </param>
        /// <returns> The type mapping, or <c>null</c> if none could be found. </returns>
        protected override CoreTypeMapping FindMapping(TypeMappingInfo mappingInfo)
            => FindMapping((RelationalTypeMappingInfo)mappingInfo);

        /// <summary>
        ///     Dependencies used to create this <see cref="RelationalTypeMappingSource" />
        /// </summary>
        protected virtual RelationalTypeMappingSourceDependencies RelationalDependencies { get; }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="IProperty" />.
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(IProperty property)
            => property.FindRelationalMapping()
               ?? FindMappingWithConversion(new ConcreteRelationalTypeMappingInfo(property));

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" />.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" />
        ///         or <see cref="MemberInfo" /> available, otherwise call <see cref="FindMapping(IProperty)" />
        ///         or <see cref="FindMapping(MemberInfo)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(Type type)
            => FindMappingWithConversion(new ConcreteRelationalTypeMappingInfo(type));

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="MemberInfo" /> representing
        ///         a field or a property of a CLR type.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="member"> The field or property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public override CoreTypeMapping FindMapping(MemberInfo member)
            => FindMappingWithConversion(new ConcreteRelationalTypeMappingInfo(member));

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given database type name.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public virtual RelationalTypeMapping FindMapping(string storeTypeName)
            => (RelationalTypeMapping)FindMappingWithConversion(new ConcreteRelationalTypeMappingInfo(storeTypeName));

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" /> and additional facets.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        ///     <para>
        ///         Note: providers should typically not need to override this method.
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or Ansi mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        public virtual RelationalTypeMapping FindMapping(
            Type type,
            bool keyOrIndex,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
            => (RelationalTypeMapping)FindMappingWithConversion(
                new ConcreteRelationalTypeMappingInfo(
                    type, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale));

        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(IProperty property)
            => (RelationalTypeMapping)FindMapping(property);

        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(Type type)
            => (RelationalTypeMapping)FindMapping(type);

        RelationalTypeMapping IRelationalTypeMappingSource.FindMapping(MemberInfo member)
            => (RelationalTypeMapping)FindMapping(member);

        private sealed class ConcreteRelationalTypeMappingInfo : RelationalTypeMappingInfo
        {
            public ConcreteRelationalTypeMappingInfo([NotNull] IProperty property)
                : base(property)
            {
            }

            public ConcreteRelationalTypeMappingInfo([NotNull] Type type)
                : base(type)
            {
            }

            public ConcreteRelationalTypeMappingInfo([NotNull] MemberInfo member)
                : base(member)
            {
            }

            public ConcreteRelationalTypeMappingInfo([NotNull] string storeTypeName)
                : base(storeTypeName)
            {
            }

            private ConcreteRelationalTypeMappingInfo(ConcreteRelationalTypeMappingInfo source, ValueConverterInfo builtInConverter)
                : base(source, builtInConverter)
            {
            }

            public ConcreteRelationalTypeMappingInfo(
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

            public override TypeMappingInfo WithBuiltInConverter(ValueConverterInfo converterInfo)
                => new ConcreteRelationalTypeMappingInfo(this, converterInfo);
        }
    }
}
