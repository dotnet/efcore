// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
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
            StoreTypeModifierKind storeTypeModifier = StoreTypeModifierKind.None,
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
            : base(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    clrType, converter, comparer, keyComparer), storeType, storeTypeModifier, dbType, unicode, size, fixedLength, precision, scale))

        {
            UdtTypeName = udtTypeName ?? storeType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqlServerUdtTypeMapping(RelationalTypeMappingParameters parameters, [CanBeNull] string udtTypeName)
            : base(parameters)
        {
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
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new SqlServerUdtTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), UdtTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new SqlServerUdtTypeMapping(Parameters.WithComposedConverter(converter), UdtTypeName);

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
    }
}
