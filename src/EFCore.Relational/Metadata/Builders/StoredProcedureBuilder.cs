// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableStoredProcedure" /> that an entity type is mapped to.
/// </summary>
public class StoredProcedureBuilder : IInfrastructure<EntityTypeBuilder>, IInfrastructure<IConventionStoredProcedureBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public StoredProcedureBuilder(IMutableStoredProcedure sproc, EntityTypeBuilder entityTypeBuilder)
    {
        Builder = ((StoredProcedure)sproc).Builder;
        EntityTypeBuilder = entityTypeBuilder;
    }

    private EntityTypeBuilder EntityTypeBuilder { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalStoredProcedureBuilder Builder { [DebuggerStepThrough] get; }

    /// <inheritdoc />
    IConventionStoredProcedureBuilder IInfrastructure<IConventionStoredProcedureBuilder>.Instance
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     The stored procedure being configured.
    /// </summary>
    public virtual IMutableStoredProcedure Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasParameter(string propertyName)
    {
        Builder.HasParameter(propertyName, ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new parameter if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasParameter(
        string propertyName,
        Action<StoredProcedureParameterBuilder> buildAction)
    {
        var parameterBuilder = Builder.HasParameter(propertyName, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureParameterBuilder(parameterBuilder, CreatePropertyBuilder(propertyName)));
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasOriginalValueParameter(string propertyName)
    {
        Builder.HasOriginalValueParameter(propertyName, ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that holds the original value if no parameter mapped to the given property exists.
    /// </summary>
    /// <param name="propertyName">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasOriginalValueParameter(
        string propertyName,
        Action<StoredProcedureParameterBuilder> buildAction)
    {
        var parameterBuilder = Builder.HasOriginalValueParameter(propertyName, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureParameterBuilder(parameterBuilder, CreatePropertyBuilder(propertyName)));
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that returns the rows affected if no such parameter exists.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasRowsAffectedParameter()
    {
        Builder.HasRowsAffectedParameter(ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new parameter that returns the rows affected if no such parameter exists.
    /// </summary>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasRowsAffectedParameter(
        Action<StoredProcedureParameterBuilder> buildAction)
    {
        var parameterBuilder = Builder.HasRowsAffectedParameter(ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureParameterBuilder(parameterBuilder, null));
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasResultColumn(string propertyName)
    {
        Builder.HasResultColumn(propertyName, ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result for this stored procedure. This is used for database generated columns.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="buildAction">An action that performs configuration of the column.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasResultColumn(
        string propertyName,
        Action<StoredProcedureResultColumnBuilder> buildAction)
    {
        var resultColumnBuilder = Builder.HasResultColumn(propertyName, ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureResultColumnBuilder(resultColumnBuilder, CreatePropertyBuilder(propertyName)));
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result that returns the rows affected for this stored procedure
    ///     if no such column exists.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasRowsAffectedResultColumn()
    {
        Builder.HasRowsAffectedResultColumn(ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a new column of the result that returns the rows affected for this stored procedure
    ///     if no such column exists.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasRowsAffectedResultColumn(
        Action<StoredProcedureResultColumnBuilder> buildAction)
    {
        var resultColumnBuilder = Builder.HasRowsAffectedResultColumn(ConfigurationSource.Explicit)!;
        buildAction(new StoredProcedureResultColumnBuilder(resultColumnBuilder, null));
        return this;
    }

    /// <summary>
    ///     Configures the result of this stored procedure to be the number of rows affected.
    /// </summary>
    /// <param name="rowsAffectedReturned">
    ///     A value indicating whether this stored procedure returns the number of rows affected.
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasRowsAffectedReturnValue(bool rowsAffectedReturned = true)
    {
        Builder.HasRowsAffectedReturn(rowsAffectedReturned, ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Adds or updates an annotation on the stored procedure. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual StoredProcedureBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual PropertyBuilder CreatePropertyBuilder(string propertyName)
    {
        var entityType = EntityTypeBuilder.Metadata;
        var property = entityType.FindProperty(propertyName);
        if (property == null)
        {
            property = entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties())
                .FirstOrDefault(p => p.Name == propertyName);
        }

        if (property == null)
        {
            throw new InvalidOperationException(CoreStrings.PropertyNotFound(propertyName, entityType.DisplayName()));
        }

#pragma warning disable EF1001 // Internal EF Core API usage.
        return new ModelBuilder(entityType.Model)
#pragma warning restore EF1001 // Internal EF Core API usage.
            .Entity(property.DeclaringType.Name)
            .Property(property.ClrType, propertyName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual PropertyBuilder CreatePropertyBuilder<TDerivedEntity, TProperty>(
        Expression<Func<TDerivedEntity, TProperty>> propertyExpression)
        where TDerivedEntity : class
    {
        var memberInfo = propertyExpression.GetMemberAccess();
        var entityType = EntityTypeBuilder.Metadata;
        var entityTypeBuilder = entityType.ClrType == typeof(TDerivedEntity)
            ? EntityTypeBuilder
#pragma warning disable EF1001 // Internal EF Core API usage.
            : new ModelBuilder(entityType.Model).Entity(typeof(TDerivedEntity));
#pragma warning restore EF1001 // Internal EF Core API usage.

        return entityTypeBuilder.Property(memberInfo.GetMemberType(), memberInfo.Name);
    }

    EntityTypeBuilder IInfrastructure<EntityTypeBuilder>.Instance
        => EntityTypeBuilder;
}
