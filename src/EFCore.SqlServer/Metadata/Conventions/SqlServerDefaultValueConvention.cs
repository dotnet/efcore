// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Conventions;

/// <summary>
/// TODO
/// </summary>
public class SqlServerDefaultValueConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerDefaultValueConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public SqlServerDefaultValueConvention(
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

    /// <inheritdoc />
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var explicitDefaultConstraintNames = new List<string>();

        // store all explicit names first - we don't want to change those in case they conflict with implicit names
        foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entity.GetDeclaredProperties())
            {
                if (property.FindAnnotation(SqlServerAnnotationNames.DefaultConstraintName) is IConventionAnnotation annotation
                    && annotation.Value is string explicitDefaultConstraintName
                    && annotation.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    if (property.GetMappedStoreObjects(StoreObjectType.Table).Count() > 1)
                    {
                        // for TPC and some entity splitting scenarios (specifically composite key) we end up with multiple tables
                        // having to define the constraint. Since constraint has to be unique, we can't keep the same name for all
                        // Disabling this scenario until we have better place to configure the constraint name
                        // see issue #27970
                        throw new InvalidOperationException("Can't use explicitly named default convention with TPC or entity splitting."); // TODO improve message and add some info
                    }

                    explicitDefaultConstraintNames.Add(explicitDefaultConstraintName);
                }
            }
        }

        var existingDefaultConstraintNames = new List<string>(explicitDefaultConstraintNames);
        var useNamedDefaultConstraints = modelBuilder.Metadata.AreNamedDefaultConstraintsUsed() == true;

        var suffixCounter = 1;
        foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entity.GetDeclaredProperties())
            {
                if (property.FindAnnotation(RelationalAnnotationNames.DefaultValue) is IConventionAnnotation defaultValueAnnotation
                    || property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql) is IConventionAnnotation defaultValueSqlAnnotation)
                {
                    var defaultConstraintNameAnnotation = property.FindAnnotation(SqlServerAnnotationNames.DefaultConstraintName);
                    if (defaultConstraintNameAnnotation != null && defaultConstraintNameAnnotation.GetConfigurationSource() != ConfigurationSource.Convention)
                    {
                        // explicit constraint name - we already stored those so nothing to do here
                        continue;
                    }

                    if (useNamedDefaultConstraints)
                    {
                        var mappedTables = property.GetMappedStoreObjects(StoreObjectType.Table);
                        var mappedTablesCount = mappedTables.Count();

                        if (mappedTablesCount == 0)
                        {
                            continue;
                        }

                        if (mappedTablesCount == 1)
                        {
                            var constraintNameCandidate = property.DefaultDefaultConstraintName(mappedTables.First());
                            if (!existingDefaultConstraintNames.Contains(constraintNameCandidate))
                            {
                                // name that we generate is unique - add it to the list of names but we don't need to store it as annotation
                                existingDefaultConstraintNames.Add(constraintNameCandidate);
                            }
                            else
                            {
                                // conflict - generate name that is unique and store is as annotation
                                while (existingDefaultConstraintNames.Contains(constraintNameCandidate + suffixCounter))
                                {
                                    suffixCounter++;
                                }

                                existingDefaultConstraintNames.Add(constraintNameCandidate + suffixCounter);
                                property.SetDefaultConstraintName(constraintNameCandidate + suffixCounter);
                            }

                            continue;
                        }

                        // TPC or entity splitting - when column is mapped to multiple tables, we can deal with them
                        // as long as there are no name clashes with some other constraints
                        // by the time we actually need to generate the constraint name (to put it in the annotation for the migration op)
                        // we will know which store object the property we are processing is mapped to, so can pick the right name based on that
                        // here though, where we want to uniquefy the name duplicates, we work on the model level so can't pick the right de-duped name
                        // so in case of conflict, we have to throw
                        // see issue #27970
                        var constraintNameCandidates = new List<string>();
                        foreach (var mappedTable in mappedTables)
                        {
                            var constraintNameCandidate = property.DefaultDefaultConstraintName(mappedTable);
                            if (constraintNameCandidate != null)
                            {
                                if (!existingDefaultConstraintNames.Contains(constraintNameCandidate))
                                {
                                    constraintNameCandidates.Add(constraintNameCandidate);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"Named default constraints can't be used with TPC or entity splitting if they result in non-unique constraint name. Constraint name: '{constraintNameCandidate}'.");
                                }
                            }
                        }

                        existingDefaultConstraintNames.AddRange(constraintNameCandidates);
                    }
                }
            }
        }
    }
}
