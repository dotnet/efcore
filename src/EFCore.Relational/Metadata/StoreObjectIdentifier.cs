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
    public readonly struct StoreObjectIdentifier : IComparable<StoreObjectIdentifier>
    {
        private StoreObjectIdentifier(StoreObjectType storeObjectType, string name, string schema = null)
        {
            StoreObjectType = storeObjectType;
            Name = name;
            Schema = schema;
        }

        /// <summary>
        ///     Creates a table id.
        /// </summary>
        /// <param name="name"> The table name. </param>
        /// <param name="schema"> The table schema. </param>
        /// <returns> The table id. </returns>
        public static StoreObjectIdentifier Table([NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotNull(name, nameof(name));

            return new StoreObjectIdentifier(StoreObjectType.Table, name, schema);
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

            return new StoreObjectIdentifier(StoreObjectType.View, name, schema);
        }

        /// <summary>
        ///     Creates an id for the SQL query mapped using <see cref="M:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The SQL query id. </returns>
        public static StoreObjectIdentifier SqlQuery([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new StoreObjectIdentifier(StoreObjectType.SqlQuery, entityType.GetDefaultSqlQueryName());
        }

        /// <summary>
        ///     Creates a SQL query id.
        /// </summary>
        /// <param name="name"> The SQL query name. </param>
        /// <returns> The SQL query id. </returns>
        public static StoreObjectIdentifier SqlQuery([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            return new StoreObjectIdentifier(StoreObjectType.SqlQuery, name);
        }

        /// <summary>
        ///     Creates a function id.
        /// </summary>
        /// <param name="modelName"> The function model name. </param>
        /// <returns> The function id. </returns>
        public static StoreObjectIdentifier DbFunction([NotNull] string modelName)
        {
            Check.NotNull(modelName, nameof(modelName));

            return new StoreObjectIdentifier(StoreObjectType.Function, modelName);
        }

        /// <summary>
        ///     Gets the table-like store object type.
        /// </summary>
        public StoreObjectType StoreObjectType { get; }

        /// <summary>
        ///     Gets the table-like store object name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the table-like store object schema.
        /// </summary>
        public string Schema { get; }

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
    }
}
