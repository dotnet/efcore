// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class TypedRelationalValueBufferFactory : IRelationalValueBufferFactory
    {
        private readonly Func<DbDataReader, object[]> _valueFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TypedRelationalValueBufferFactory(
            RelationalValueBufferFactoryDependencies dependencies,
            Func<DbDataReader, object[]> valueFactory)
        {
            _valueFactory = valueFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueBuffer Create(DbDataReader dataReader)
        {
            Check.DebugAssert(dataReader != null, "dataReader != null"); // hot path

            var values = _valueFactory(dataReader);

            return values.Length == 0
                ? ValueBuffer.Empty
                : new ValueBuffer(values);
        }
    }
}
