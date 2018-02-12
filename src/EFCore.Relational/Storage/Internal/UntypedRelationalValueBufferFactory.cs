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
    public class UntypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly Action<object[]> _processValuesAction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public UntypedRelationalValueBufferFactory(
            [NotNull] RelationalValueBufferFactoryDependencies dependencies,
            [NotNull] IReadOnlyList<TypeMaterializationInfo> mappingInfo,
            [CanBeNull] Action<object[]> processValuesAction)
        {
            _processValuesAction = processValuesAction;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueBuffer Create(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var fieldCount = dataReader.FieldCount;

            if (fieldCount == 0)
            {
                return ValueBuffer.Empty;
            }

            var values = new object[fieldCount];

            dataReader.GetValues(values);

            _processValuesAction?.Invoke(values);

            for (var i = 0; i < fieldCount; i++)
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
