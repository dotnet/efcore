// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
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
    public class ReaderColumn<T> : ReaderColumn
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ReaderColumn{T}" /> class.
        /// </summary>
        /// <param name="nullable"> A value indicating if the column is nullable. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="getFieldValue"> A function to get field value for the column from the reader. </param>
        [Obsolete("Use constructor which also takes IPropertyBase.")]
        public ReaderColumn(bool nullable, [CanBeNull] string name, [NotNull] Func<DbDataReader, int[], T> getFieldValue)
            : this(nullable, name, property: null, getFieldValue)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ReaderColumn{T}" /> class.
        /// </summary>
        /// <param name="nullable"> A value indicating if the column is nullable. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="property"> The property being read if any, null otherwise. </param>
        /// <param name="getFieldValue"> A function to get field value for the column from the reader. </param>
        public ReaderColumn(
            bool nullable,
            [CanBeNull] string name,
            [CanBeNull] IPropertyBase property,
            [NotNull] Func<DbDataReader, int[], T> getFieldValue)
            : base(typeof(T), nullable, name, property)
        {
            GetFieldValue = getFieldValue;
        }

        /// <summary>
        ///     The function to get field value for the column from the reader.
        /// </summary>
        public virtual Func<DbDataReader, int[], T> GetFieldValue { get; }
    }
}
