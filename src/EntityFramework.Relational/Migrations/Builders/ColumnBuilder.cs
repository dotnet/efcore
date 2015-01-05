// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class ColumnBuilder
    {
        public virtual Column Binary(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] int? maxLength = null,
            [CanBeNull] bool? fixedLength = null,
            [CanBeNull] byte[] defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool timestamp = false,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(byte[]),
                nullable,
                defaultValue,
                defaultSql,
                maxLength,
                fixedLength: fixedLength,
                timestamp: timestamp,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Boolean(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] bool? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(bool),
                nullable,
                defaultValue,
                defaultSql,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Byte(
            [CanBeNull] bool? nullable = null,
            bool identity = false,
            [CanBeNull] byte? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(byte),
                nullable,
                defaultValue,
                defaultSql,
                identity: identity,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column DateTime(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] byte? precision = null,
            [CanBeNull] DateTime? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(DateTime),
                nullable,
                defaultValue,
                defaultSql,
                precision: precision,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Decimal(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] byte? precision = null,
            [CanBeNull] byte? scale = null,
            [CanBeNull] decimal? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null,
            bool identity = false)
        {
            return BuildColumn(
                typeof(decimal),
                nullable,
                defaultValue,
                defaultSql,
                precision: precision,
                scale: scale,
                computed: computed,
                name: name,
                dataType: dataType,
                identity: identity);
        }

        public virtual Column Double(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] double? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(double),
                nullable,
                defaultValue,
                defaultSql,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Guid(
            [CanBeNull] bool? nullable = null,
            bool identity = false,
            [CanBeNull] Guid? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(Guid),
                nullable,
                defaultValue,
                defaultSql,
                identity: identity,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Single(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] float? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(float),
                nullable,
                defaultValue,
                defaultSql,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Short(
            [CanBeNull] bool? nullable = null,
            bool identity = false,
            [CanBeNull] short? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(short),
                nullable,
                defaultValue,
                defaultSql,
                identity: identity,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Int(
            [CanBeNull] bool? nullable = null,
            bool identity = false,
            [CanBeNull] int? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(int),
                nullable,
                defaultValue,
                defaultSql,
                identity: identity,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Long(
            [CanBeNull] bool? nullable = null,
            bool identity = false,
            [CanBeNull] long? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(long),
                nullable,
                defaultValue,
                defaultSql,
                identity: identity,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column String(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] int? maxLength = null,
            [CanBeNull] bool? fixedLength = null,
            [CanBeNull] bool? unicode = null,
            [CanBeNull] string defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(string),
                nullable,
                defaultValue,
                defaultSql,
                maxLength,
                fixedLength: fixedLength,
                unicode: unicode,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column Time(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] byte? precision = null,
            [CanBeNull] TimeSpan? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(TimeSpan),
                nullable,
                defaultValue,
                defaultSql,
                precision: precision,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        public virtual Column DateTimeOffset(
            [CanBeNull] bool? nullable = null,
            [CanBeNull] byte? precision = null,
            [CanBeNull] DateTimeOffset? defaultValue = null,
            [CanBeNull] string defaultSql = null,
            bool computed = false,
            [CanBeNull] string name = null,
            [CanBeNull] string dataType = null)
        {
            return BuildColumn(
                typeof(DateTimeOffset),
                nullable,
                defaultValue,
                defaultSql,
                precision: precision,
                computed: computed,
                name: name,
                dataType: dataType);
        }

        private static Column BuildColumn(
            Type clrType,
            bool? nullable,
            object defaultValue,
            string defaultSql = null,
            int? maxLength = null,
            byte? precision = null,
            byte? scale = null,
            bool? unicode = null,
            bool? fixedLength = null,
            bool identity = false,
            bool timestamp = false,
            bool computed = false,
            string name = null,
            string dataType = null)
        {
            var column
                = new Column(name, clrType)
                    {
                        DataType = dataType,
                        DefaultValue = defaultValue,
                        DefaultSql = defaultSql,
                        IsIdentity =  identity,
                        IsTimestamp = timestamp,
                        IsComputed = computed,
                        MaxLength = maxLength,
                        Precision = precision,
                        Scale = scale,
                        IsFixedLength = fixedLength,
                        IsUnicode = unicode
                    };

            if (nullable.HasValue)
            {
                column.IsNullable = nullable.Value;
            }

            return column;
        }
    }
}
