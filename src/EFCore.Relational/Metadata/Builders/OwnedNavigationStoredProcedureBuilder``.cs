// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableStoredProcedure" /> that an entity type is mapped to.
/// </summary>
/// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
/// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
public class OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> :
    OwnedNavigationStoredProcedureBuilder,
    IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>
    where TOwnerEntity : class
    where TDependentEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationStoredProcedureBuilder(
        IMutableStoredProcedure sproc,
        OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder)
        : base(sproc, ownedNavigationBuilder)
    {
    }

    private OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> OwnedNavigationBuilder
        => (OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>)((IInfrastructure<OwnedNavigationBuilder>)this)
            .GetInfrastructure();

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(string propertyName)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasParameter(propertyName);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter(
        string propertyName,
        Action<StoredProcedureParameterBuilder> buildAction)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasParameter(propertyName, buildAction);

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression)
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
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasParameter<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression,
        Action<StoredProcedureParameterBuilder> buildAction)
    {
        var parameterBuilder = Builder.HasParameter(propertyExpression, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureParameterBuilder(parameterBuilder, CreatePropertyBuilder(propertyExpression)));
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
        string propertyName)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasOriginalValueParameter(propertyName);

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter(
        string propertyName,
        Action<StoredProcedureParameterBuilder> buildAction)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasOriginalValueParameter(propertyName, buildAction);

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression)
    {
        Builder.HasOriginalValueParameter(propertyExpression, ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasOriginalValueParameter<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression,
        Action<StoredProcedureParameterBuilder> buildAction)
    {
        var parameterBuilder = Builder.HasOriginalValueParameter(propertyExpression, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureParameterBuilder(parameterBuilder, CreatePropertyBuilder(propertyExpression)));
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that returns the rows affected if no such parameter exists.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter()
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasRowsAffectedParameter();

    /// <summary>
    ///     Configures a new parameter that returns the rows affected if no such parameter exists.
    /// </summary>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedParameter(
        Action<StoredProcedureParameterBuilder> buildAction)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasRowsAffectedParameter(buildAction);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(string propertyName)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasResultColumn(propertyName);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn(
        string propertyName,
        Action<StoredProcedureResultColumnBuilder> buildAction)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasResultColumn(propertyName, buildAction);

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <genericparam name="TProperty">The property type.</genericparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (<c>blog => blog.Url</c>).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression)
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
    public virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasResultColumn<TProperty>(
        Expression<Func<TDependentEntity, TProperty>> propertyExpression,
        Action<StoredProcedureResultColumnBuilder> buildAction)
    {
        var resultColumnBuilder = Builder.HasResultColumn(propertyExpression, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureResultColumnBuilder(resultColumnBuilder, CreatePropertyBuilder(propertyExpression)));
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result that returns the rows affected for this stored procedure
    ///     if no such column exists.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn()
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasRowsAffectedResultColumn();

    /// <summary>
    ///     Configures a new column of the result that returns the rows affected for this stored procedure
    ///     if no such column exists.
    /// </summary>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedResultColumn(
        Action<StoredProcedureResultColumnBuilder> buildAction)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasRowsAffectedResultColumn(buildAction);

    /// <summary>
    ///     Configures the result of this stored procedure to be the number of rows affected.
    /// </summary>
    /// <param name="rowsAffectedReturned">
    ///     A value indicating whether this stored procedure returns the number of rows affected.
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasRowsAffectedReturnValue(
        bool rowsAffectedReturned = true)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasRowsAffectedReturnValue(rowsAffectedReturned);

    /// <summary>
    ///     Adds or updates an annotation on the stored procedure. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity> HasAnnotation(
        string annotation,
        object? value)
        => (OwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>)base.HasAnnotation(annotation, value);

    OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>.Instance
        => OwnedNavigationBuilder;
}
