// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     An exception that is thrown when a concurrency violation is encountered while saving to the database. A concurrency violation
    ///     occurs when an unexpected number of rows are affected during save. This is usually because the data in the database has
    ///     been modified since it was loaded into memory.
    /// </summary>
    [Serializable]
    public class DbUpdateConcurrencyException : DbUpdateException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        /// </summary>
        public DbUpdateConcurrencyException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        public DbUpdateConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception. </param>
        public DbUpdateConcurrencyException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception. </param>
        /// <param name="entries"> The entries that were involved in the error. </param>
        public DbUpdateConcurrencyException(
            string message,
            Exception? innerException,
            IReadOnlyList<IUpdateEntry> entries)
            : base(message, innerException, entries)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateConcurrencyException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="entries"> The entries that were involved in the concurrency violation. </param>
        public DbUpdateConcurrencyException(
            string message,
            IReadOnlyList<IUpdateEntry> entries)
            : base(message, entries)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class from a serialized form.
        /// </summary>
        /// <param name="info"> The serialization info. </param>
        /// <param name="context"> The streaming context being used. </param>
        public DbUpdateConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
