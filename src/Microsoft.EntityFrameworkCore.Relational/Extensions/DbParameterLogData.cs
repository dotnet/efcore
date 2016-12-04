// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Logging information about the parameters of a <see cref="DbCommand" /> that is being executed.
    ///     </para>
    ///     <para>
    ///         Instances of this class are typically created by Entity Framework and passed to loggers, it is not designed
    ///         to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class DbParameterLogData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbParameterLogData" /> class.
        /// </summary>
        /// <param name="name">
        ///     The name of the parameter.
        /// </param>
        /// <param name="value">
        ///     The value of the parameter.
        /// </param>
        /// <param name="hasValue">
        ///     A value indicating whether the parameter has a value (or is assigned null).
        /// </param>
        /// <param name="direction">
        ///     The direction of the parameter.
        /// </param>
        /// <param name="dbType">
        ///     The type of the parameter.
        /// </param>
        /// <param name="nullable">
        ///     A value indicating whether the parameter type is nullable.
        /// </param>
        /// <param name="size">
        ///     The size of the type of the parameter.
        /// </param>
        /// <param name="precision">
        ///     The precision of the type of the parameter.
        /// </param>
        /// <param name="scale">
        ///     The scale of the type of the parameter.
        /// </param>
        public DbParameterLogData(
            [NotNull] string name,
            [CanBeNull] object value,
            bool hasValue,
            ParameterDirection direction,
            DbType dbType,
            bool nullable,
            int size,
            byte precision,
            byte scale)
        {
            Name = name;
            Value = value;
            HasValue = hasValue;
            Direction = direction;
            DbType = dbType;
            IsNullable = nullable;
            Size = size;
            Precision = precision;
            Scale = scale;
        }

        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     Gets the value of the parameter.
        /// </summary>
        public virtual object Value { get; }

        /// <summary>
        ///     Gets a value indicating whether the parameter has a value (or is assigned null).
        /// </summary>
        public virtual bool HasValue { get; set; }

        /// <summary>
        ///     Gets the direction of the parameter.
        /// </summary>
        public virtual ParameterDirection Direction { get; set; }

        /// <summary>
        ///     Gets the type of the parameter.
        /// </summary>
        public virtual DbType DbType { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the parameter type is nullable.
        /// </summary>
        public virtual bool IsNullable { get; }

        /// <summary>
        ///     Gets the size of the type of the parameter.
        /// </summary>
        public virtual int Size { get; }

        /// <summary>
        ///     Gets the precision of the type of the parameter.
        /// </summary>
        public virtual byte Precision { get; }

        /// <summary>
        ///     Gets the scale of the type of the parameter.
        /// </summary>
        public virtual byte Scale { get; }
    }
}
