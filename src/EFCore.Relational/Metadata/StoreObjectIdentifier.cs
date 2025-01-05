// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A type that represents the id of a store object
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public readonly struct StoreObjectIdentifier : IComparable<StoreObjectIdentifier>, IEquatable<StoreObjectIdentifier>
{
    private StoreObjectIdentifier(StoreObjectType storeObjectType, string name, string? schema = null)
    {
        StoreObjectType = storeObjectType;
        Name = name;
        Schema = schema;
    }

    /// <summary>
    ///     Creates an id for the store object that the given entity type is mapped to.
    /// </summary>
    /// <param name="typeBase">The entity type.</param>
    /// <param name="type">The store object type.</param>
    /// <returns>The store object id.</returns>
    public static StoreObjectIdentifier? Create(IReadOnlyTypeBase typeBase, StoreObjectType type)
    {
        Check.NotNull(typeBase, nameof(typeBase));

        switch (type)
        {
            case StoreObjectType.Table:
                var tableName = typeBase.GetTableName();
                return tableName == null ? null : Table(tableName, typeBase.GetSchema());
            case StoreObjectType.View:
                var viewName = typeBase.GetViewName();
                return viewName == null ? null : View(viewName, typeBase.GetViewSchema());
            case StoreObjectType.SqlQuery:
                var query = typeBase.GetSqlQuery();
                return query == null ? null : SqlQuery(typeBase.ContainingEntityType);
            case StoreObjectType.Function:
                var functionName = typeBase.GetFunctionName();
                return functionName == null ? null : DbFunction(functionName);
            case StoreObjectType.InsertStoredProcedure:
                var insertStoredProcedure = typeBase.GetInsertStoredProcedure();
                return insertStoredProcedure == null || insertStoredProcedure.Name == null
                    ? null
                    : InsertStoredProcedure(insertStoredProcedure.Name, insertStoredProcedure.Schema);
            case StoreObjectType.DeleteStoredProcedure:
                var deleteStoredProcedure = typeBase.GetDeleteStoredProcedure();
                return deleteStoredProcedure == null || deleteStoredProcedure.Name == null
                    ? null
                    : DeleteStoredProcedure(deleteStoredProcedure.Name, deleteStoredProcedure.Schema);
            case StoreObjectType.UpdateStoredProcedure:
                var updateStoredProcedure = typeBase.GetUpdateStoredProcedure();
                return updateStoredProcedure == null || updateStoredProcedure.Name == null
                    ? null
                    : UpdateStoredProcedure(updateStoredProcedure.Name, updateStoredProcedure.Schema);
            default:
                return null;
        }
    }

    /// <summary>
    ///     Creates a table id.
    /// </summary>
    /// <param name="name">The table name.</param>
    /// <param name="schema">The table schema.</param>
    /// <returns>The table id.</returns>
    public static StoreObjectIdentifier Table(string name, string? schema = null)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.Table, name, schema);
    }

    /// <summary>
    ///     Creates a view id.
    /// </summary>
    /// <param name="name">The view name.</param>
    /// <param name="schema">The view schema.</param>
    /// <returns>The view id.</returns>
    public static StoreObjectIdentifier View(string name, string? schema = null)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.View, name, schema);
    }

    /// <summary>
    ///     Creates an id for the SQL query mapped using <see cref="O:RelationalEntityTypeBuilderExtensions.ToSqlQuery" />.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The SQL query id.</returns>
    public static StoreObjectIdentifier SqlQuery(IReadOnlyEntityType entityType)
    {
        Check.NotNull(entityType, nameof(entityType));

        return new StoreObjectIdentifier(StoreObjectType.SqlQuery, entityType.GetDefaultSqlQueryName());
    }

    /// <summary>
    ///     Creates a SQL query id.
    /// </summary>
    /// <param name="name">The SQL query name.</param>
    /// <returns>The SQL query id.</returns>
    public static StoreObjectIdentifier SqlQuery(string name)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.SqlQuery, name);
    }

    /// <summary>
    ///     Creates a function id.
    /// </summary>
    /// <param name="modelName">The function model name.</param>
    /// <returns>The function id.</returns>
    public static StoreObjectIdentifier DbFunction(string modelName)
    {
        Check.NotNull(modelName, nameof(modelName));

        return new StoreObjectIdentifier(StoreObjectType.Function, modelName);
    }

    /// <summary>
    ///     Creates an insert stored procedure id.
    /// </summary>
    /// <param name="name">The stored procedure name.</param>
    /// <param name="schema">The stored procedure schema.</param>
    /// <returns>The stored procedure id.</returns>
    public static StoreObjectIdentifier InsertStoredProcedure(string name, string? schema = null)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.InsertStoredProcedure, name, schema);
    }

    /// <summary>
    ///     Creates a delete stored procedure id.
    /// </summary>
    /// <param name="name">The stored procedure name.</param>
    /// <param name="schema">The stored procedure schema.</param>
    /// <returns>The stored procedure id.</returns>
    public static StoreObjectIdentifier DeleteStoredProcedure(string name, string? schema = null)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.DeleteStoredProcedure, name, schema);
    }

    /// <summary>
    ///     Creates an update stored procedure id.
    /// </summary>
    /// <param name="name">The stored procedure name.</param>
    /// <param name="schema">The stored procedure  schema.</param>
    /// <returns>The stored procedure id.</returns>
    public static StoreObjectIdentifier UpdateStoredProcedure(string name, string? schema = null)
    {
        Check.NotNull(name, nameof(name));

        return new StoreObjectIdentifier(StoreObjectType.UpdateStoredProcedure, name, schema);
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
    public string? Schema { get; }

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
    public string DisplayName()
        => Schema == null ? Name : Schema + "." + Name;

    /// <inheritdoc />
    public override string ToString()
        => StoreObjectType + " " + DisplayName();

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is StoreObjectIdentifier identifier && Equals(identifier);

    /// <inheritdoc />
    public bool Equals(StoreObjectIdentifier other)
        => StoreObjectType == other.StoreObjectType && Name == other.Name && Schema == other.Schema;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(StoreObjectType, Name, Schema);

    /// <summary>
    ///     Compares one id to another id to see if they represent the same store object.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="true" /> if they represent the same store object; <see langword="false" /> otherwise.</returns>
    public static bool operator ==(StoreObjectIdentifier left, StoreObjectIdentifier right)
        => left.Equals(right);

    /// <summary>
    ///     Compares one id to another id to see if they represent the same store object.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="false" /> if they represent the same store object; <see langword="true" /> otherwise.</returns>
    public static bool operator !=(StoreObjectIdentifier left, StoreObjectIdentifier right)
        => !(left == right);
}
