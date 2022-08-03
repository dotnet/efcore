// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableStoredProcedure" /> that an entity type is mapped to.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class StoredProcedureBuilder<TEntity> : StoredProcedureBuilder, IInfrastructure<EntityTypeBuilder<TEntity>>
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public StoredProcedureBuilder(IMutableStoredProcedure sproc, EntityTypeBuilder<TEntity> entityTypeBuilder)
        : base(sproc, entityTypeBuilder)
    {
    }

    private EntityTypeBuilder<TEntity> EntityTypeBuilder
        => (EntityTypeBuilder<TEntity>)((IInfrastructure<EntityTypeBuilder>)this).Instance;

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> HasParameter(string propertyName)
        => (StoredProcedureBuilder<TEntity>)base.HasParameter(propertyName);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> HasParameter(
        string propertyName, Action<StoredProcedureParameterBuilder> buildAction)
        => (StoredProcedureBuilder<TEntity>)base.HasParameter(propertyName, buildAction);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasParameter<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
        => HasParameter<TEntity, TProperty>(propertyExpression);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TDerivedEntity">The same or a derived entity type.</genericparam>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
        where TDerivedEntity : class, TEntity
    {
        Builder.HasParameter(propertyExpression, ConfigurationSource.Explicit);
        return this;
    }
    
    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasParameter<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Action<StoredProcedureParameterBuilder> buildAction)
        => HasParameter<TEntity, TProperty>(propertyExpression, buildAction);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TDerivedEntity">The same or a derived entity type.</genericparam>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasParameter<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
        Action<StoredProcedureParameterBuilder> buildAction)
        where TDerivedEntity : class, TEntity
    {
        Builder.HasParameter(propertyExpression, ConfigurationSource.Explicit);
        buildAction(new(Metadata.GetStoreIdentifier()!.Value, CreatePropertyBuilder(propertyExpression)));
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> HasResultColumn(string propertyName)
        => (StoredProcedureBuilder<TEntity>)base.HasResultColumn(propertyName);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> HasResultColumn(
        string propertyName, Action<StoredProcedureResultColumnBuilder> buildAction)
        => (StoredProcedureBuilder<TEntity>)base.HasResultColumn(propertyName, buildAction);
    
    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
        => HasResultColumn<TEntity, TProperty>(propertyExpression);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <genericparam name="TDerivedEntity">The same or a derived entity type.</genericparam>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
        where TDerivedEntity : class, TEntity
    {
        Builder.HasResultColumn(propertyExpression, ConfigurationSource.Explicit);
        return this;
    }
    
    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasResultColumn<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Action<StoredProcedureResultColumnBuilder> buildAction)
        => HasResultColumn<TEntity, TProperty>(propertyExpression, buildAction);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <genericparam name="TDerivedEntity">The same or a derived entity type.</genericparam>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder<TEntity> HasResultColumn<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression,
        Action<StoredProcedureResultColumnBuilder> buildAction)
        where TDerivedEntity : class, TEntity
    {
        Builder.HasResultColumn(propertyExpression, ConfigurationSource.Explicit);
        buildAction(new(Metadata.GetStoreIdentifier()!.Value, CreatePropertyBuilder(propertyExpression)));
        return this;
    }

    /// <summary>
    ///     Prevents automatically creating a transaction when executing this stored procedure.
    /// </summary>
    /// <param name="suppress">A value indicating whether the automatic transactions should be prevented.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> SuppressTransactions(bool suppress = true)
        => (StoredProcedureBuilder<TEntity>)base.SuppressTransactions(suppress);

    /// <summary>
    ///     Adds or updates an annotation on the stored procedure. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual StoredProcedureBuilder<TEntity> HasAnnotation(string annotation, object? value)
        => (StoredProcedureBuilder<TEntity>)base.HasAnnotation(annotation, value);

    EntityTypeBuilder<TEntity> IInfrastructure<EntityTypeBuilder<TEntity>>.Instance => EntityTypeBuilder;
}
