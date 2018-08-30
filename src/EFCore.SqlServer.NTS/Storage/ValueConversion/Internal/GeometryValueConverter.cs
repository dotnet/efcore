// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GeometryValueConverter : ValueConverter
    {
        private static readonly SqlServerSpatialWriter _writer
            = new SqlServerSpatialWriter { HandleOrdinates = Ordinates.XYZM };

        private Func<object, object> _convertToProvider;
        private Func<object, object> _convertFromProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public GeometryValueConverter(Type type, SqlServerSpatialReader reader)
            : base(
                  (Expression<Func<IGeometry, SqlBytes>>)(g => new SqlBytes(_writer.Write(g))),
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
            => typeof(SqlBytes);

        private static LambdaExpression GetConvertFromProviderExpression(Type type, SqlServerSpatialReader reader)
        {
            var bytes = Expression.Parameter(typeof(SqlBytes), "bytes");

            Expression body = Expression.Call(
                Expression.Constant(reader),
                typeof(SqlServerSpatialReader).GetRuntimeMethod(nameof(SqlServerSpatialReader.Read), new[] { typeof(byte[]) }),
                new[]
                {
                    Expression.Property(bytes, nameof(SqlBytes.Value))
                });
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
