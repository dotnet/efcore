// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseModel : Annotatable, IEquatable<DatabaseModel>
    {
        /// <summary>
        ///     The database name, or <c>null</c> if none is set.
        /// </summary>
        public virtual string DatabaseName { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The database schema, or <c>null</c> to use the default schema.
        /// </summary>
        public virtual string DefaultSchema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The list of tables in the database.
        /// </summary>
        public virtual IList<DatabaseTable> Tables { get; } = new List<DatabaseTable>();

        /// <summary>
        ///     The list of sequences in the database.
        /// </summary>
        public virtual IList<DatabaseSequence> Sequences { get; } = new List<DatabaseSequence>();

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(DatabaseModel other)
        {
            if (other == null
                || !base.Equals(other)
                || DatabaseName != other.DatabaseName
                || DefaultSchema != other.DefaultSchema
                || Tables.Count != other.Tables.Count
                || Sequences.Count != other.Sequences.Count)
            {
                return false;
            }

            for (var i = 0; i < Tables.Count; i++)
            {
                if (!Tables[i].Equals(other.Tables[i]))
                {
                    return false;
                }
            }

            for (var i = 0; i < Sequences.Count; i++)
            {
                if (!Sequences[i].Equals(other.Sequences[i]))
                {
                    return false;
                }
            }

            // TODO: Compare annotations too.
            // Can't implement Equals on Annotatable because that takes over equality for everything that derives
            // from it (e.g. EntityType).

            return true;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => obj is DatabaseModel other && Equals(other);

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => 0;   // TODO
    }
}
