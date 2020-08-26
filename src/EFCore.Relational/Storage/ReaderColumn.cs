// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _constructors
            = new ConcurrentDictionary<Type, ConstructorInfo>();

        /// <summary>
        ///     Creates a new instance of the <see cref="ReaderColumn" /> class.
        /// </summary>
        /// <param name="type"> The CLR type of the column. </param>
        /// <param name="nullable"> A value indicating if the column is nullable. </param>
        /// <param name="name"> The name of the column. </param>
        [Obsolete("Use constructor which also takes IPropertyBase.")]
        protected ReaderColumn([NotNull] Type type, bool nullable, [CanBeNull] string name)
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
        protected ReaderColumn([NotNull] Type type, bool nullable, [CanBeNull] string name, [CanBeNull] IPropertyBase property)
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
        public virtual string Name { get; }

        /// <summary>
        ///     The property being read if any, null otherwise.
        /// </summary>
        public virtual IPropertyBase Property { get; }

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
        public static ReaderColumn Create([NotNull] Type type, bool nullable, [CanBeNull] string columnName, [NotNull] object readFunc)
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
            [NotNull] Type type,
            bool nullable,
            [CanBeNull] string columnName,
            [CanBeNull] IPropertyBase property,
            [NotNull] object readFunc)
            => (ReaderColumn)GetConstructor(type).Invoke(new[] { nullable, columnName, property, readFunc });

        private static ConstructorInfo GetConstructor(Type type)
            => _constructors.GetOrAdd(
                type, t => typeof(ReaderColumn<>).MakeGenericType(t).GetConstructors().First(ci=> ci.GetParameters().Length == 4));
    }
}
