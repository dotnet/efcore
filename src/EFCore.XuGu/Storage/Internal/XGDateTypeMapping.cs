// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="DateTime" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class XGDateTypeMapping : RelationalTypeMapping, IDefaultValueCompatibilityAware
    {
        private readonly bool _isDefaultValueCompatible;

        public static XGDateTypeMapping Default { get; } = new("date", typeof(DateOnly));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGDateTypeMapping([NotNull] string storeType, Type clrType, bool isDefaultValueCompatible = false)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        jsonValueReaderWriter: JsonDateOnlyReaderWriter.Instance),
                    storeType,
                    dbType: System.Data.DbType.Date),
                isDefaultValueCompatible)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected XGDateTypeMapping(RelationalTypeMappingParameters parameters, bool isDefaultValueCompatible)
            : base(parameters)
        {
            _isDefaultValueCompatible = isDefaultValueCompatible;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGDateTypeMapping(parameters, _isDefaultValueCompatible);

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="isDefaultValueCompatible"> Use a default value compatible syntax, or not. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping Clone(bool isDefaultValueCompatible = false)
            => new XGDateTypeMapping(Parameters, isDefaultValueCompatible);

        /// <summary>
        ///     Gets the string format to be used to generate SQL literals of this type.
        /// </summary>
        protected override string SqlLiteralFormatString => $@"{(_isDefaultValueCompatible ? null : "DATE ")}'{{0:yyyy-MM-dd}}'";
    }
}
