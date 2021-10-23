// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
    public class InMemoryTypeMappingSource : TypeMappingSource
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryTypeMappingSource(TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            Check.DebugAssert(clrType != null, "ClrType is null");

            if (clrType.IsValueType
                || clrType == typeof(string)
                || clrType == typeof(byte[]))
            {
                return new InMemoryTypeMapping(clrType);
            }

            if (clrType.FullName == "NetTopologySuite.Geometries.Geometry"
                || clrType.GetBaseTypes().Any(t => t.FullName == "NetTopologySuite.Geometries.Geometry"))
            {
                var comparer = (ValueComparer)Activator.CreateInstance(typeof(GeometryValueComparer<>).MakeGenericType(clrType))!;

                return new InMemoryTypeMapping(
                    clrType,
                    comparer,
                    comparer);
            }

            return base.FindMapping(mappingInfo);
        }
    }
}
