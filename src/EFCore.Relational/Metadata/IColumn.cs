// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a column in a table.
    /// </summary>
    public interface IColumn : IColumnBase
    {
        /// <summary>
        ///     Gets the containing table.
        /// </summary>
        new ITable Table { get; }

        /// <summary>
        ///     Gets the property mappings.
        /// </summary>
        new IEnumerable<IColumnMapping> PropertyMappings { get; }

        /// <summary>
        ///     Gets the maximum length of data that is allowed in this column. For example, if the property is a <see cref="string" /> '
        ///     then this is the maximum number of characters.
        /// </summary>
        int? MaxLength
            => PropertyMappings.First().Property.GetMaxLength(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Gets the precision of data that is allowed in this column. For example, if the property is a <see cref="decimal" /> '
        ///     then this is the maximum number of digits.
        /// </summary>
        int? Precision
            => PropertyMappings.First().Property.GetPrecision(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Gets the scale of data that is allowed in this column. For example, if the property is a <see cref="decimal" /> '
        ///     then this is the maximum number of decimal places.
        /// </summary>
        int? Scale
            => PropertyMappings.First().Property.GetScale(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        bool? IsUnicode
            => PropertyMappings.First().Property.IsUnicode(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Returns a flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        bool? IsFixedLength
            => PropertyMappings.First().Property.IsFixedLength(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Indicates whether or not this column acts as an automatic concurrency token by generating a different value
        ///     on every update in the same vein as 'rowversion'/'timestamp' columns on SQL Server.
        /// </summary>
        bool IsRowVersion
            => PropertyMappings.First().Property.IsConcurrencyToken
                && PropertyMappings.First().Property.ValueGenerated == ValueGenerated.OnAddOrUpdate;

        /// <summary>
        ///     Returns the object that is used as the default value for this column.
        /// </summary>
        public virtual object DefaultValue
        {
            get
            {
                var property = PropertyMappings.First().Property;
                var value = property.GetDefaultValue(StoreObjectIdentifier.Table(Table.Name, Table.Schema));
                var converter = property.GetValueConverter() ?? PropertyMappings.First().TypeMapping?.Converter;

                return converter != null
                    ? converter.ConvertToProvider(value)
                    : value;
            }
        }

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for this column.
        /// </summary>
        public virtual string DefaultValueSql
            => PropertyMappings.First().Property
                .GetDefaultValueSql(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Returns the SQL expression that is used as the computed value for this column.
        /// </summary>
        public virtual string ComputedColumnSql
            => PropertyMappings.First().Property
                .GetComputedColumnSql(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Returns whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        public virtual bool? IsStored
            => PropertyMappings.First().Property
                .GetIsStored(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Comment for this column
        /// </summary>
        public virtual string Comment
            => PropertyMappings.First().Property
                .GetComment(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

        /// <summary>
        ///     Collation for this column
        /// </summary>
        public virtual string Collation
            => PropertyMappings.First().Property
                .GetCollation(StoreObjectIdentifier.Table(Table.Name, Table.Schema));
    }
}
