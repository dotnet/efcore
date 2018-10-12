// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerUdtTypeMapping : RelationalTypeMapping
    {
        private static Action<DbParameter, string> _udtTypeNameSetter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerUdtTypeMapping(
            [NotNull] Type clrType,
            [NotNull] string storeType,
            [NotNull] Func<object, Expression> literalGenerator,
            StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
            [CanBeNull] string udtTypeName = null,
            [CanBeNull] ValueConverter converter = null,
            [CanBeNull] ValueComparer comparer = null,
            [CanBeNull] ValueComparer keyComparer = null,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            int? precision = null,
            int? scale = null)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType, converter, comparer, keyComparer), storeType, storeTypePostfix, dbType, unicode, size, fixedLength, precision, scale))

        {
            LiteralGenerator = literalGenerator;
            UdtTypeName = udtTypeName ?? storeType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqlServerUdtTypeMapping(
            RelationalTypeMappingParameters parameters,
            [NotNull] Func<object, Expression> literalGenerator,
            [CanBeNull] string udtTypeName)
            : base(parameters)
        {
            LiteralGenerator = literalGenerator;
            UdtTypeName = udtTypeName ?? parameters.StoreType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string UdtTypeName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<object, Expression> LiteralGenerator { get; }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerUdtTypeMapping(parameters, LiteralGenerator, UdtTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
            => SetUdtTypeName(parameter);

        private void SetUdtTypeName(DbParameter parameter)
        {
            NonCapturingLazyInitializer.EnsureInitialized(
                ref _udtTypeNameSetter,
                parameter.GetType(),
                CreateUdtTypeNameAccessor);

            if (parameter.Value != null
                && parameter.Value != DBNull.Value)
            {
                _udtTypeNameSetter(parameter, UdtTypeName);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression GenerateCodeLiteral(object value)
            => LiteralGenerator(value);

        private static Action<DbParameter, string> CreateUdtTypeNameAccessor(Type paramType)
        {
            var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
            var valueParam = Expression.Parameter(typeof(string), "value");

            return Expression.Lambda<Action<DbParameter, string>>(
                Expression.Call(
                    Expression.Convert(paramParam, paramType),
                    paramType.GetProperty("UdtTypeName").SetMethod,
                    valueParam),
                paramParam,
                valueParam).Compile();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerUdtTypeMapping CreateSqlHierarchyIdMapping(Type udtType)
            => new SqlServerUdtTypeMapping(
                udtType,
                "hierarchyid",
                v => Expression.Call(
                    v.GetType().GetMethod("Parse"),
                    Expression.New(
                        typeof(SqlString).GetConstructor(new[] { typeof(string) }),
                        Expression.Constant(v.ToString(), typeof(string)))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerUdtTypeMapping CreateSqlSpatialMapping(Type udtType, string storeName)
            => new SqlServerUdtTypeMapping(
                udtType,
                storeName,
                v =>
                {
                    var spatialType = v.GetType();
                    var noParams = new object[0];

                    var wkt = ((SqlChars)spatialType.GetMethod("AsTextZM").Invoke(v, noParams)).ToSqlString().ToString();
                    var srid = ((SqlInt32)spatialType.GetMethod("get_STSrid").Invoke(v, noParams)).Value;

                    return Expression.Call(
                        spatialType.GetMethod("STGeomFromText"),
                        Expression.New(
                            typeof(SqlChars).GetConstructor(
                                new[] { typeof(SqlString) }),
                            Expression.New(
                                typeof(SqlString).GetConstructor(
                                    new[] { typeof(string) }),
                                Expression.Constant(wkt, typeof(string)))),
                        Expression.Constant(srid, typeof(int)));
                });
    }
}
