// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
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
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] string udtTypeName = null,
            [CanBeNull] ValueConverter converter = null,
            [CanBeNull] ValueComparer comparer = null,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false)
            : base(storeType, clrType, converter, comparer, dbType, unicode, size, fixedLength)
        {
            UdtTypeName = udtTypeName ?? storeType;
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
            => new SqlServerUdtTypeMapping(storeType, ClrType, UdtTypeName, Converter, Comparer, DbType, IsUnicode, size, IsFixedLength);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new SqlServerUdtTypeMapping(StoreType, ClrType, UdtTypeName, ComposeConverter(converter), Comparer, DbType, IsUnicode, Size, IsFixedLength);

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

            if (parameter.Value != null && parameter.Value != DBNull.Value)
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
