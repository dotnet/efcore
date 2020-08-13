// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

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

        protected ReaderColumn([NotNull] Type type, bool nullable, [CanBeNull] string name)
        {
            Type = type;
            IsNullable = nullable;
            Name = name;
        }

        public virtual Type Type { get; }
        public virtual bool IsNullable { get; }
        public virtual string Name { get; }

        /// <summary>
        ///     Creates an instance of <see cref="ReaderColumn{T}"/>.
        /// </summary>
        /// <param name="type"> The type of the column. </param>
        /// <param name="nullable"> Whether the column can contain <c>null</c> values. </param>
        /// <param name="columnName"> The column name if it is used to access the column values, <c>null</c> otherwise.</param>
        /// <param name="readFunc">
        ///     A <see cref="T:System.Func{DbDataReader, Int32[], T}"/> used to get the field value for this column.
        /// </param>
        /// <returns> An instance of <see cref="ReaderColumn{T}"/>.</returns>
        public static ReaderColumn Create([NotNull] Type type, bool nullable, [CanBeNull] string columnName, [NotNull] object readFunc)
            => (ReaderColumn)GetConstructor(type).Invoke(new[] { nullable, columnName, readFunc });

        private static ConstructorInfo GetConstructor(Type type)
            => _constructors.GetOrAdd(type, t => typeof(ReaderColumn<>).MakeGenericType(t).GetConstructors()[0]);
    }
}
