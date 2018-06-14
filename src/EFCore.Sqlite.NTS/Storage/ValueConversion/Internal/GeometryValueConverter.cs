// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GeometryValueConverter : ValueConverter
    {
        private static readonly GaiaGeoWriter _writer = new GaiaGeoWriter();

        private Func<object, object> _convertToProvider;
        private Func<object, object> _convertFromProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public GeometryValueConverter(Type type, GaiaGeoReader reader)
            : base(
                  (Expression<Func<IGeometry, byte[]>>)(g => _writer.Write(g)),
                  GetConvertFromProviderExpression(type, reader))
        {
            ModelClrType = type;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Func<object, object> ConvertToProvider
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _convertToProvider,
                this,
                x => Compile(x.ConvertToProviderExpression));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Func<object, object> ConvertFromProvider
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _convertFromProvider,
                this,
                x => Compile(x.ConvertFromProviderExpression));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ModelClrType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ProviderClrType
            => typeof(byte[]);

        private static LambdaExpression GetConvertFromProviderExpression(Type type, GaiaGeoReader reader)
        {
            var bytes = Expression.Parameter(typeof(byte[]), "blob");

            Expression body = Expression.Call(
                Expression.Constant(reader),
                typeof(GaiaGeoReader).GetRuntimeMethod(nameof(GaiaGeoReader.Read), new[] { typeof(byte[]) }),
                bytes);
            if (!type.IsAssignableFrom(typeof(IGeometry)))
            {
                body = Expression.Convert(body, type);
            }

            return Expression.Lambda(body, bytes);
        }

        private static Func<object, object> Compile(LambdaExpression convertExpression)
        {
            var compiled = convertExpression.Compile();

            return x => x != null
                ? compiled.DynamicInvoke(x)
                : null;
        }
    }
}
