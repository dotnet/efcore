// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         An expected column in the relational data reader.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ReaderColumn
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _constructors = new();

        /// <summary>
        ///     Creates a new instance of the <see cref="ReaderColumn" /> class.
        /// </summary>
        /// <param name="type"> The CLR type of the column. </param>
        /// <param name="nullable"> A value indicating if the column is nullable. </param>
        /// <param name="name"> The name of the column. </param>
        [Obsolete("Use constructor which also takes IPropertyBase.")]
        protected ReaderColumn(Type type, bool nullable, string? name)
            : this(type, nullable, name, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ReaderColumn" /> class.
        /// </summary>
        /// <param name="type"> The CLR type of the column. </param>
        /// <param name="nullable"> A value indicating if the column is nullable. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="property"> The property being read if any, null otherwise. </param>
        protected ReaderColumn(Type type, bool nullable, string? name, IPropertyBase? property)
        {
            Type = type;
            IsNullable = nullable;
            Name = name;
            Property = property;
        }

        /// <summary>
        ///     The CLR type of the column.
        /// </summary>
        public virtual Type Type { get; }

        /// <summary>
        ///     A value indicating if the column is nullable.
        /// </summary>
        public virtual bool IsNullable { get; }

        /// <summary>
        ///     The name of the column.
        /// </summary>
        public virtual string? Name { get; }

        /// <summary>
        ///     The property being read if any, null otherwise.
        /// </summary>
        public virtual IPropertyBase? Property { get; }

        /// <summary>
        ///     Creates an instance of <see cref="ReaderColumn{T}" />.
        /// </summary>
        /// <param name="type"> The type of the column. </param>
        /// <param name="nullable"> Whether the column can contain <see langword="null" /> values. </param>
        /// <param name="columnName"> The column name if it is used to access the column values, <see langword="null" /> otherwise.</param>
        /// <param name="readFunc">
        ///     A <see cref="T:System.Func{DbDataReader, Int32[], T}" /> used to get the field value for this column.
        /// </param>
        /// <returns> An instance of <see cref="ReaderColumn{T}" />.</returns>
        [Obsolete("Use method which also takes IPropertyBase.")]
        public static ReaderColumn Create(Type type, bool nullable, string? columnName, object readFunc)
            => (ReaderColumn)GetConstructor(type).Invoke(new[] { nullable, columnName, readFunc });

        /// <summary>
        ///     Creates an instance of <see cref="ReaderColumn{T}" />.
        /// </summary>
        /// <param name="type"> The type of the column. </param>
        /// <param name="nullable"> Whether the column can contain <see langword="null" /> values. </param>
        /// <param name="columnName"> The column name if it is used to access the column values, <see langword="null" /> otherwise.</param>
        /// <param name="property"> The property being read if any, null otherwise. </param>
        /// <param name="readFunc">
        ///     A <see cref="T:System.Func{DbDataReader, Int32[], T}" /> used to get the field value for this column.
        /// </param>
        /// <returns> An instance of <see cref="ReaderColumn{T}" />.</returns>
        public static ReaderColumn Create(
            Type type,
            bool nullable,
            string? columnName,
            IPropertyBase? property,
            object readFunc)
            => (ReaderColumn)GetConstructor(type).Invoke(new[] { nullable, columnName, property, readFunc });

        private static ConstructorInfo GetConstructor(Type type)
            => _constructors.GetOrAdd(
                type, t => typeof(ReaderColumn<>).MakeGenericType(t).GetConstructors().First(ci=> ci.GetParameters().Length == 4));
    }
}
