// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures model function mappings based on public static methods on the context marked with
///     <see cref="DbFunctionAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public class RelationalDbFunctionAttributeConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalDbFunctionAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalDbFunctionAttributeConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after a model is initialized.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var contextType = Dependencies.ContextType;
        while (contextType != null
               && contextType != typeof(DbContext))
        {
            var functions = contextType.GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(mi => mi.IsDefined(typeof(DbFunctionAttribute)));

            foreach (var function in functions)
            {
                modelBuilder.HasDbFunction(function);
            }

            contextType = contextType.BaseType;
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var function in modelBuilder.Metadata.GetDbFunctions())
        {
            ProcessDbFunctionAdded(function.Builder, context);
        }
    }

    /// <summary>
    ///     Called when an <see cref="IConventionDbFunction" /> is added to the model.
    /// </summary>
    /// <param name="dbFunctionBuilder">The builder for the <see cref="IConventionDbFunction" />.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void ProcessDbFunctionAdded(
        IConventionDbFunctionBuilder dbFunctionBuilder,
        IConventionContext context)
    {
        var methodInfo = dbFunctionBuilder.Metadata.MethodInfo;
        var dbFunctionAttribute = methodInfo?.GetCustomAttributes<DbFunctionAttribute>().SingleOrDefault();
        if (dbFunctionAttribute != null)
        {
            dbFunctionBuilder.HasName(dbFunctionAttribute.Name, fromDataAnnotation: true);
            if (dbFunctionAttribute.Schema != null)
            {
                dbFunctionBuilder.HasSchema(dbFunctionAttribute.Schema, fromDataAnnotation: true);
            }

            if (dbFunctionAttribute.IsBuiltIn)
            {
                dbFunctionBuilder.IsBuiltIn(dbFunctionAttribute.IsBuiltIn, fromDataAnnotation: true);
            }

            if (dbFunctionAttribute.IsNullableHasValue)
            {
                dbFunctionBuilder.IsNullable(dbFunctionAttribute.IsNullable, fromDataAnnotation: true);
            }
        }
    }
}
