// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     A type that represents the id of a store object
    /// </summary>
    public struct StoreObjectIdentifier : IComparable<StoreObjectIdentifier>, IEquatable<StoreObjectIdentifier>
    {
        /// <summary>
        ///     Creates a table id.
        /// </summary>
        /// <param name="name"> The table name. </param>
        /// <param name="schema"> The table schema. </param>
        /// <returns> The table id. </returns>
        public static StoreObjectIdentifier Table([NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotNull(name, nameof(name));

            return new StoreObjectIdentifier { StoreObjectType = StoreObjectType.Table, Name = name, Schema = schema };
        }

        /// <summary>
        ///     Creates a view id.
        /// </summary>
        /// <param name="name"> The view name. </param>
        /// <param name="schema"> The view schema. </param>
        /// <returns> The view id. </returns>
        public static StoreObjectIdentifier View([NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotNull(name, nameof(name));

            return new StoreObjectIdentifier { StoreObjectType = StoreObjectType.View, Name = name, Schema = schema };
        }

        /// <summary>
        ///     Creates an id for the SQL query mapped using <see cref="M:RelationalEntityTypeBuilderExtensions.ToQuerySql" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The SQL query id. </returns>
        public static StoreObjectIdentifier SqlQuery([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new StoreObjectIdentifier { StoreObjectType = StoreObjectType.SqlQuery, Name = entityType.GetDefaultSqlQueryName() };
        }

        /// <summary>
        ///     Creates a SQL query id.
        /// </summary>
        /// <param name="name"> The SQL query name. </param>
        /// <returns> The SQL query id. </returns>
        public static StoreObjectIdentifier SqlQuery([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            return new StoreObjectIdentifier { StoreObjectType = StoreObjectType.SqlQuery, Name = name };
        }

        /// <summary>
        ///     Creates a function id.
        /// </summary>
        /// <param name="modelName"> The function model name. </param>
        /// <returns> The function id. </returns>
        public static StoreObjectIdentifier DbFunction([NotNull] string modelName)
        {
            Check.NotNull(modelName, nameof(modelName));

            return new StoreObjectIdentifier { StoreObjectType = StoreObjectType.Function, Name = modelName };
        }

        /// <summary>
        ///     Gets the table-like store object type.
        /// </summary>
        public StoreObjectType StoreObjectType { get; private set; }

        /// <summary>
        ///     Gets the table-like store object name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the table-like store object schema.
        /// </summary>
        public string Schema { get; private set; }

        /// <inheritdoc />
        public int CompareTo(StoreObjectIdentifier other)
        {
            var result = StoreObjectType.CompareTo(other.StoreObjectType);
            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(Name, other.Name);
            if (result != 0)
            {
                return result;
            }

            return StringComparer.Ordinal.Compare(Schema, other.Schema);
        }

        /// <summary>
        ///     Gets the friendly display name for the store object.
        /// </summary>
        public string DisplayName() => Schema == null ? Name : Schema + "." + Name;

        /// <inheritdoc />
        public override string ToString() => StoreObjectType.ToString() + " " + DisplayName();

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(StoreObjectIdentifier other)
        {
            return Name == other.Name
                && Schema == other.Schema
                && StoreObjectType == other.StoreObjectType;
        }

        /// <summary>
        ///     Determines if this key is equivalent to a given object (i.e. if they are keys for the same query).
        /// </summary>
        /// <param name="obj">
        ///     The object to compare this key to.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the object is a <see cref="StoreObjectIdentifier" /> and is for the same query,
        ///     otherwise <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
            => obj is StoreObjectIdentifier other && Equals(other);

        /// <summary>
        ///     Gets the hash code for this object.
        /// </summary>
        /// <returns>
        ///     The hash code.
        /// </returns>
        public override int GetHashCode()
            => HashCode.Combine(Name, Schema, StoreObjectType);
    }
}
