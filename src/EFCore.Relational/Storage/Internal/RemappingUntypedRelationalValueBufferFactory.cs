// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Obsolete("Use TypedRelationalValueBufferFactory instead.")]
    public class RemappingUntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly IReadOnlyList<TypeMaterializationInfo> _mappingInfo;
        private readonly Action<object[]> _processValuesAction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RemappingUntypedRelationalValueBufferFactory(
            [NotNull] RelationalValueBufferFactoryDependencies dependencies,
            [NotNull] IReadOnlyList<TypeMaterializationInfo> mappingInfo,
            [CanBeNull] Action<object[]> processValuesAction)
        {
            _mappingInfo = mappingInfo;
            _processValuesAction = processValuesAction;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueBuffer Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path
            Debug.Assert(dataReader.FieldCount >= _mappingInfo.Count);

            if (_mappingInfo.Count == 0)
            {
                return ValueBuffer.Empty;
            }

            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            var remappedValues = new object[_mappingInfo.Count];

            for (var i = 0; i < _mappingInfo.Count; i++)
            {
                remappedValues[i] = values[_mappingInfo[i].Index];
            }

            values = remappedValues;

            _processValuesAction?.Invoke(values);

            for (var i = 0; i < _mappingInfo.Count; i++)
            {
                if (ReferenceEquals(values[i], DBNull.Value))
                {
                    values[i] = null;
                }
            }

            return new ValueBuffer(values);
        }
    }
}
