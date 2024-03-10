// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class RelationalModelAsserter : ModelAsserter
{
    public new static RelationalModelAsserter Instance { get; } = new();

    public override void AssertEqual(
        IReadOnlyModel expected,
        IReadOnlyModel actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareMemberAnnotations);

        var designTime = expected is Model && actual is Model;

        Assert.Multiple(
            () => Assert.Equal(expected.GetDatabaseName(), actual.GetDatabaseName()),
            () => Assert.Equal(expected.GetDefaultSchema(), actual.GetDefaultSchema()),
            () => Assert.Equal(expected.GetMaxIdentifierLength(), actual.GetMaxIdentifierLength()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetCollation(), actual.GetCollation());
                }
            },
            () => Assert.Equal(expected.GetDbFunctions().Select(x => x),
                actual.GetDbFunctions(),
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.GetSequences().Select(x => x),
                actual.GetSequences(),
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () =>
            {
                var expectedRelationalModel = (IRelationalModel)((IModel)expected)
                    ?.FindRuntimeAnnotationValue(RelationalAnnotationNames.RelationalModel);
                var actualRelationalModel = (IRelationalModel)((IModel)actual)
                    ?.FindRuntimeAnnotationValue(RelationalAnnotationNames.RelationalModel);
                if (expectedRelationalModel != null)
                {
                    AssertEqual(expectedRelationalModel, actualRelationalModel, compareMemberAnnotations);
                }
            });
    }

    public virtual bool AssertEqual(
        IReadOnlyDbFunction expected,
        IReadOnlyDbFunction actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        Assert.Same(actual, actual.Model.FindDbFunction(actual.ModelName));

        Assert.Multiple(
            () => Assert.Equal(expected.ModelName, actual.ModelName),
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.Schema, actual.Schema),
            () => Assert.Equal(expected.ReturnType, actual.ReturnType),
            () => Assert.Equal(expected.StoreType, actual.StoreType),
            () => Assert.Equal(expected.IsBuiltIn, actual.IsBuiltIn),
            () => Assert.Equal(expected.IsAggregate, actual.IsAggregate),
            () => Assert.Equal(expected.IsScalar, actual.IsScalar),
            () => Assert.Equal(expected.IsNullable, actual.IsNullable),
            () => Assert.Equal(expected.MethodInfo, actual.MethodInfo),
            () => Assert.Equal(expected.Translation, actual.Translation),
            () => Assert.Equal(expected.TypeMapping?.StoreType, actual.TypeMapping?.StoreType),
            () => Assert.Equal(expected.Parameters.Select(x => x),
                actual.Parameters,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareBackreferences: false)),
            () => Assert.Equal(((IRuntimeDbFunction)expected).StoreFunction.SchemaQualifiedName,
                ((IRuntimeDbFunction)actual).StoreFunction.SchemaQualifiedName),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyDbFunctionParameter expected,
        IReadOnlyDbFunctionParameter actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.Function.ModelName, actual.Function.ModelName);
                }
            },
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.StoreType, actual.StoreType),
            () => Assert.Equal(expected.PropagatesNullability, actual.PropagatesNullability),
            () => Assert.Equal(expected.TypeMapping?.StoreType, actual.TypeMapping?.StoreType),
            () => Assert.Equal(((IRuntimeDbFunctionParameter)expected).StoreFunctionParameter.Name,
                ((IRuntimeDbFunctionParameter)actual).StoreFunctionParameter.Name),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlySequence expected,
        IReadOnlySequence actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        Assert.Same(actual, actual.Model.FindSequence(actual.Name, actual.ModelSchema));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.Schema, actual.Schema),
            () => Assert.Equal(expected.ModelSchema, actual.ModelSchema),
            () => Assert.Equal(expected.Type, actual.Type),
            () => Assert.Equal(expected.IncrementBy, actual.IncrementBy),
            () => Assert.Equal(expected.StartValue, actual.StartValue),
            () => Assert.Equal(expected.MaxValue, actual.MaxValue),
            () => Assert.Equal(expected.MinValue, actual.MinValue),
            () => Assert.Equal(expected.IsCyclic, actual.IsCyclic),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyEntityType expected,
        IReadOnlyEntityType actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareBackreferences,
            compareMemberAnnotations);

        if (expected is not ITypeBase expectedStructuralType
            || actual is not ITypeBase actualStructuralType)
        {
            return true;
        }

        var designTime = expected is EntityType && actual is EntityType;

        Assert.Multiple(
            () => Assert.Equal(expected.GetDbSetName(), actual.GetDbSetName()),
            () => Assert.Equal(expected.GetContainerColumnName(), actual.GetContainerColumnName()),
            () => Assert.Equal(expected.GetJsonPropertyName(), actual.GetJsonPropertyName()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetComment(), actual.GetComment());
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.IsTableExcludedFromMigrations(), actual.IsTableExcludedFromMigrations());
                }
            },
            () => Assert.Equal(expected.GetSchemaQualifiedTableName(), actual.GetSchemaQualifiedTableName()),
            () => Assert.Equal(expected.GetSchemaQualifiedViewName(), actual.GetSchemaQualifiedViewName()),
            () => Assert.Equal(expected.GetSqlQuery(), actual.GetSqlQuery()),
            () => Assert.Equal(expected.GetFunctionName(), actual.GetFunctionName()),
            () => AssertEqual(
                expected.GetInsertStoredProcedure(),
                actual.GetInsertStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => AssertEqual(
                expected.GetUpdateStoredProcedure(),
                actual.GetUpdateStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => AssertEqual(
                expected.GetDeleteStoredProcedure(),
                actual.GetDeleteStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => Assert.Equal(expected.GetMappingFragments().Select(x => x),
                actual.GetMappingFragments(),
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareBackreferences: false)),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetCheckConstraints().Select(x => x),
                        actual.GetCheckConstraints(),
                        (expected, actual) =>
                            AssertEqual(
                                expected,
                                actual,
                                compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                                compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                                compareBackreferences: false));
                }
            },
            () => Assert.Equal(expectedStructuralType.GetDefaultMappings().Select(x => x),
                actualStructuralType.GetDefaultMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetTableMappings().Select(x => x),
                actualStructuralType.GetTableMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetViewMappings().Select(x => x),
                actualStructuralType.GetViewMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetSqlQueryMappings().Select(x => x),
                actualStructuralType.GetSqlQueryMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetFunctionMappings().Select(x => x),
                actualStructuralType.GetFunctionMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetInsertStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetInsertStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetUpdateStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetUpdateStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetDeleteStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetDeleteStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyEntityTypeMappingFragment expected,
        IReadOnlyEntityTypeMappingFragment actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.IsTableExcludedFromMigrations, actual.IsTableExcludedFromMigrations),
            () => Assert.Equal(expected.StoreObject, actual.StoreObject),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.EntityType, actual.EntityType, EntityTypeFullNameComparer.Instance);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyStoredProcedure expected,
        IReadOnlyStoredProcedure actual,
        bool compareBackreferences = false,
        bool compareAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);
            return true;
        }

        var expectedAnnotations = compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>();
        var actualAnnotations = compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>();

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.GetStoreIdentifier(), actual.GetStoreIdentifier()),
            () => Assert.Equal(expected.IsRowsAffectedReturned, actual.IsRowsAffectedReturned),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.EntityType, actual.EntityType, EntityTypeFullNameComparer.Instance);
                }
            },
            () => Assert.Equal(expected.Parameters.Select(x => x), actual.Parameters,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareBackreferences: false,
                        compareAnnotations)),
            () => Assert.Equal(expected.ResultColumns.Select(x => x), actual.ResultColumns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareBackreferences: false,
                        compareAnnotations)),
            () => Assert.Equal(((IRuntimeStoredProcedure)expected).StoreStoredProcedure.SchemaQualifiedName,
                ((IRuntimeStoredProcedure)actual).StoreStoredProcedure.SchemaQualifiedName),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyStoredProcedureParameter expected,
        IReadOnlyStoredProcedureParameter actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.PropertyName, actual.PropertyName),
            () => Assert.Equal(expected.Direction, actual.Direction),
            () => Assert.Equal(expected.ForOriginalValue, actual.ForOriginalValue),
            () => Assert.Equal(expected.ForRowsAffected, actual.ForRowsAffected),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.StoredProcedure.Name, actual.StoredProcedure.Name);
                }
            },
            () => Assert.Equal(((IRuntimeStoredProcedureParameter)expected).StoreParameter.Name,
                ((IRuntimeStoredProcedureParameter)actual).StoreParameter.Name),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyStoredProcedureResultColumn expected,
        IReadOnlyStoredProcedureResultColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.PropertyName, actual.PropertyName),
            () => Assert.Equal(expected.ForRowsAffected, actual.ForRowsAffected),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.StoredProcedure.Name, actual.StoredProcedure.Name);
                }
            },
            () => Assert.Equal(((IRuntimeStoredProcedureResultColumn)expected).StoreResultColumn.Name,
                ((IRuntimeStoredProcedureResultColumn)actual).StoreResultColumn.Name),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));


        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyCheckConstraint expected,
        IReadOnlyCheckConstraint actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        Assert.Same(actual, actual.EntityType.FindCheckConstraint(actual.ModelName));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ModelName, actual.ModelName),
            () => Assert.Equal(expected.Sql, actual.Sql),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.EntityType, actual.EntityType, EntityTypeFullNameComparer.Instance);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyComplexType expected,
        IReadOnlyComplexType actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations,
            actualAnnotations,
            compareBackreferences,
            compareMemberAnnotations);

        if (expected is not ITypeBase expectedStructuralType
            || actual is not ITypeBase actualStructuralType)
        {
            return true;
        }

        Assert.Multiple(
            () => Assert.Equal(expected.GetContainerColumnName(), actual.GetContainerColumnName()),
            () => Assert.Equal(expectedStructuralType.GetJsonPropertyName(), actualStructuralType.GetJsonPropertyName()),
            () => Assert.Equal(expectedStructuralType.GetTableName(), actualStructuralType.GetTableName()),
            () => Assert.Equal(expectedStructuralType.GetViewName(), actualStructuralType.GetViewName()),
            () => Assert.Equal(expectedStructuralType.GetSqlQuery(), actualStructuralType.GetSqlQuery()),
            () => Assert.Equal(expectedStructuralType.GetFunctionName(), actualStructuralType.GetFunctionName()),
            () => AssertEqual(
                expectedStructuralType.GetInsertStoredProcedure(),
                actualStructuralType.GetInsertStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => AssertEqual(
                expectedStructuralType.GetUpdateStoredProcedure(),
                actualStructuralType.GetUpdateStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => AssertEqual(
                expectedStructuralType.GetDeleteStoredProcedure(),
                actualStructuralType.GetDeleteStoredProcedure(),
                compareBackreferences: false,
                compareMemberAnnotations),
            () => Assert.Equal(expectedStructuralType.GetMappingFragments().Select(x => x),
                actualStructuralType.GetMappingFragments(),
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareBackreferences: false)),
            () => Assert.Equal(expectedStructuralType.GetDefaultMappings().Select(x => x),
                actualStructuralType.GetDefaultMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetTableMappings().Select(x => x),
                actualStructuralType.GetTableMappings().Select(x => x),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetViewMappings().Select(x => x),
                actualStructuralType.GetViewMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetSqlQueryMappings().Select(x => x),
                actualStructuralType.GetSqlQueryMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetFunctionMappings().Select(x => x),
                actualStructuralType.GetFunctionMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetInsertStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetInsertStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetUpdateStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetUpdateStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedStructuralType.GetDeleteStoredProcedureMappings().Select(x => x),
                actualStructuralType.GetDeleteStoredProcedureMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyProperty expected,
        IReadOnlyProperty actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareBackreferences);

        if (expected is not IProperty expectedProperty
            || actual is not IProperty actualProperty)
        {
            return true;
        }

        var designTime = expected is Property && actual is Property;

        Assert.Multiple(
            () => Assert.Equal(expected.GetColumnType(), actual.GetColumnType()),
            () => Assert.Equal(expected.GetColumnName(), actual.GetColumnName()),
            () => Assert.Equal(expected.GetIsStored(), actual.GetIsStored()),
            () => Assert.Equal(expected.GetJsonPropertyName(), actual.GetJsonPropertyName()),
            () => Assert.Equal(expected.IsOrdinalKeyProperty(), actual.IsOrdinalKeyProperty()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetColumnOrder(), actual.GetColumnOrder());
                }
            },
            () => Assert.Equal(expected.GetComputedColumnSql(), actual.GetComputedColumnSql()),
            () => Assert.Equal(expected.GetDefaultValueSql(), actual.GetDefaultValueSql()),
            () => Assert.Equal(expected.GetDefaultValue(), actual.GetDefaultValue()),
            () => Assert.Equal(expected.IsFixedLength(), actual.IsFixedLength()),
            () => Assert.Equal(expected.GetMaxLength(), actual.GetMaxLength()),
            () => Assert.Equal(expected.GetScale(), actual.GetScale()),
            () => Assert.Equal(expected.GetPrecision(), actual.GetPrecision()),
            () => Assert.Equal(expected.IsColumnNullable(), actual.IsColumnNullable()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetComment(), actual.GetComment());
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetCollation(), actual.GetCollation());
                }
            },
            () => Assert.Equal(expected.GetOverrides().Select(x => x), actual.GetOverrides().Select(x => x),
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expectedProperty.GetDefaultColumnMappings().Select(x => x),
                actualProperty.GetDefaultColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetTableColumnMappings().Select(x => x),
                actualProperty.GetTableColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetViewColumnMappings().Select(x => x),
                actualProperty.GetViewColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetSqlQueryColumnMappings().Select(x => x),
                actualProperty.GetSqlQueryColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetFunctionColumnMappings().Select(x => x),
                actualProperty.GetFunctionColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetInsertStoredProcedureParameterMappings().Select(x => x),
                actualProperty.GetInsertStoredProcedureParameterMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetInsertStoredProcedureResultColumnMappings().Select(x => x),
                actualProperty.GetInsertStoredProcedureResultColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetUpdateStoredProcedureParameterMappings().Select(x => x),
                actualProperty.GetUpdateStoredProcedureParameterMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetUpdateStoredProcedureResultColumnMappings().Select(x => x),
                actualProperty.GetUpdateStoredProcedureResultColumnMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }),
            () => Assert.Equal(expectedProperty.GetDeleteStoredProcedureParameterMappings().Select(x => x),
                actualProperty.GetDeleteStoredProcedureParameterMappings(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Column.Table.SchemaQualifiedName, actual.Column.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public virtual bool AssertEqual(
        IReadOnlyRelationalPropertyOverrides expected,
        IReadOnlyRelationalPropertyOverrides actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.StoreObject, actual.StoreObject),
            () => Assert.Equal(expected.ColumnName, actual.ColumnName),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyForeignKey expected,
        IReadOnlyForeignKey actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareBackreferences,
            compareMemberAnnotations);

        if (expected is not IForeignKey expectedForeignKey
            || actual is not IForeignKey actualForeignKey)
        {
            return true;
        }

        Assert.Multiple(
            () => Assert.Equal(expected.GetConstraintName(), actual.GetConstraintName()),
            () => Assert.Equal(expectedForeignKey.GetMappedConstraints().Select(x => x),
                actualForeignKey.GetMappedConstraints(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyIndex expected,
        IReadOnlyIndex actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareBackreferences);

        if (expected is not IIndex expectedIndex
            || actual is not IIndex actualIndex)
        {
            return true;
        }

        Assert.Multiple(
            () => Assert.Equal(expected.GetDatabaseName(), actual.GetDatabaseName()),
            () => Assert.Equal(expected.GetFilter(), actual.GetFilter()),
            () => Assert.Equal(expectedIndex.GetMappedTableIndexes().Select(x => x),
                actualIndex.GetMappedTableIndexes(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public override bool AssertEqual(
        IReadOnlyKey expected,
        IReadOnlyKey actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        base.AssertEqual(
            expected,
            actual,
            expectedAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            actualAnnotations.Where(a => !RelationalAnnotationNames.AllNames.Contains(a.Name)),
            compareBackreferences);

        if (expected is not IKey expectedKey
            || actual is not IKey actualKey)
        {
            return true;
        }

        Assert.Multiple(
            () => Assert.Equal(expected.GetName(), actual.GetName()),
            () => Assert.Equal(expectedKey.GetMappedConstraints().Select(x => x),
                actualKey.GetMappedConstraints(),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName);
                    return true;
                }));

        return true;
    }

    public virtual void AssertEqual(
        IRelationalModel expected,
        IRelationalModel actual,
        bool compareAnnotations = false)
        => AssertEqual(
            expected,
            actual,
            compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareMemberAnnotations: compareAnnotations);

    public virtual void AssertEqual(
        IRelationalModel expected,
        IRelationalModel actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations = true)
    {
        if (expected is not RelationalModel expectedModel
            || actual is not RelationalModel actualModel)
        {
            return;
        }

        var designTime = expected.Model is Model && actual.Model is Model;

        Assert.Multiple(
            () => Assert.Equal(expectedModel.IsReadOnly, actualModel.IsReadOnly),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.Collation, actual.Collation);
                }
            },
            () => Assert.Equal(expectedModel.DefaultTables.Values.Select(x => x), actualModel.DefaultTables.Values,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Tables.Select(x => x), actual.Tables,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Views.Select(x => x), actual.Views,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Queries.Select(x => x), actual.Queries,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Functions.Select(x => x), actual.Functions,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.StoredProcedures.Select(x => x), actual.StoredProcedures,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Sequences.Select(x => x), actual.Sequences,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));
    }

    public virtual bool AssertEqualBase(
        ITableBase expected,
        ITableBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.Schema, actual.Schema),
            () => Assert.Equal(expected.IsShared, actual.IsShared),
            () =>
            {
                foreach (IEntityType expectedEntityType in expected.EntityTypeMappings.Select(m => m.TypeBase))
                {
                    var actualEntityType =
                        (IEntityType)actual.EntityTypeMappings.Single(m => m.TypeBase.Name == expectedEntityType.Name).TypeBase;
                    Assert.Equal(
                        expected.GetRowInternalForeignKeys(expectedEntityType).Count(),
                        actual.GetRowInternalForeignKeys(actualEntityType).Count());
                    Assert.Equal(
                        expected.GetReferencingRowInternalForeignKeys(expectedEntityType).Count(),
                        actual.GetReferencingRowInternalForeignKeys(actualEntityType).Count());
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        ITableBase expected,
        ITableBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Same(actual, ((RelationalModel)actual.Model).DefaultTables[actual.Name]),
            () => Assert.Equal(expected.Columns, actual.Columns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqual(
        ITable expected,
        ITable actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Same(actual, actual.Model.FindTable(actual.Name, actual.Schema)),
            () => Assert.Equal(expected.Columns.Select(x => x), actual.Columns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Indexes.Select(x => x), actual.Indexes,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.ForeignKeyConstraints.Select(x => x), actual.ForeignKeyConstraints,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.ReferencingForeignKeyConstraints.Select(x => x), actual.ReferencingForeignKeyConstraints,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.UniqueConstraints.Select(x => x), actual.UniqueConstraints,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.Triggers.Select(x => x), actual.Triggers,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqual(
        IView expected,
        IView actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Same(actual, actual.Model.FindView(actual.Name, actual.Schema)),
            () => Assert.Equal(expected.Columns.Select(x => x), actual.Columns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqual(
        ISqlQuery expected,
        ISqlQuery actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Equal(expected.Sql, actual.Sql),
            () => Assert.Same(actual, actual.Model.FindQuery(actual.Name)),
            () => Assert.Equal(expected.Columns.Select(x => x), actual.Columns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqual(
        IStoreFunction expected,
        IStoreFunction actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Equal(expected.ReturnType, actual.ReturnType),
            () => Assert.Equal(expected.IsBuiltIn, actual.IsBuiltIn),
            () => Assert.Same(
                actual, actual.Model.FindFunction(actual.Name, actual.Schema, actual.Parameters.Select(p => p.StoreType).ToArray())),
            () => Assert.Equal(
                actual.DbFunctions.Select(p => p.ModelName),
                expected.DbFunctions.Select(p => p.ModelName)),
            () => Assert.Equal(expected.Parameters.Select(x => x), actual.Parameters,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.Columns.Select(x => x), actual.Columns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqual(
        IStoreStoredProcedure expected,
        IStoreStoredProcedure actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations, compareMemberAnnotations),
            () => Assert.Same(actual, actual.Model.FindStoredProcedure(actual.Name, actual.Schema)),
            () => Assert.Equal(
                actual.StoredProcedures.Select(p => p.Name),
                expected.StoredProcedures.Select(p => p.Name)),
            () =>
            {
                if (expected.ReturnValue != null)
                {
                    AssertEqualBase(
                        expected.ReturnValue,
                        actual.ReturnValue,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>());
                    Assert.Same(actual, actual.ReturnValue.StoredProcedure);
                    Assert.Equal(expected.ReturnValue.PropertyMappings.Select(x => x), actual.ReturnValue.PropertyMappings,
                        (expected, actual) =>
                            AssertEqual(
                                expected,
                                actual,
                                compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                                compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>()));
                }
                else
                {
                    Assert.Null(actual.ReturnValue);
                }
            },
            () => Assert.Equal(expected.Parameters.Select(x => x), actual.Parameters,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ResultColumns.Select(x => x), actual.ResultColumns,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.EntityTypeMappings.Select(x => x), actual.EntityTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)),
            () => Assert.Equal(expected.ComplexTypeMappings.Select(x => x), actual.ComplexTypeMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations)));

        return true;
    }

    public virtual bool AssertEqualBase(
        ITableMappingBase expected,
        ITableMappingBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.TypeBase.Name, actual.TypeBase.Name),
            () => Assert.Equal(expected.Table.SchemaQualifiedName, actual.Table.SchemaQualifiedName),
            () => Assert.Equal(expected.IncludesDerivedTypes, actual.IncludesDerivedTypes),
            () => Assert.Equal(expected.IsSharedTablePrincipal, actual.IsSharedTablePrincipal),
            () => Assert.Equal(expected.IsSplitEntityTypePrincipal, actual.IsSplitEntityTypePrincipal),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        ITableMappingBase expected,
        ITableMappingBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.ColumnMappings.Select(x => x), actual.ColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        ITableMapping expected,
        ITableMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => AssertEqual(expected.DeleteStoredProcedureMapping, actual.DeleteStoredProcedureMapping, compareMemberAnnotations),
            () => AssertEqual(expected.InsertStoredProcedureMapping, actual.InsertStoredProcedureMapping, compareMemberAnnotations),
            () => AssertEqual(expected.UpdateStoredProcedureMapping, actual.UpdateStoredProcedureMapping, compareMemberAnnotations),
            () => Assert.Equal(expected.ColumnMappings.Select(x => x), actual.ColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IViewMapping expected,
        IViewMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.ColumnMappings.Select(x => x), actual.ColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        ISqlQueryMapping expected,
        ISqlQueryMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.IsDefaultSqlQueryMapping, actual.IsDefaultSqlQueryMapping),
            () => Assert.Equal(expected.ColumnMappings.Select(x => x), actual.ColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IFunctionMapping expected,
        IFunctionMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.IsDefaultFunctionMapping, actual.IsDefaultFunctionMapping),
            () => Assert.Equal(expected.ColumnMappings.Select(x => x), actual.ColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IStoredProcedureMapping expected,
        IStoredProcedureMapping actual,
        bool compareMemberAnnotations)
    {
        if (expected == null)
        {
            Assert.Null(actual);
            return true;
        }

        return AssertEqual(
            expected,
            actual,
            compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareMemberAnnotations);
    }

    public virtual bool AssertEqual(
        IStoredProcedureMapping expected,
        IStoredProcedureMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.StoredProcedure.GetSchemaQualifiedName(), actual.StoredProcedure.GetSchemaQualifiedName()),
            () => Assert.Contains(expected.TableMapping?.Table.SchemaQualifiedName, actual.TableMapping?.Table.SchemaQualifiedName),
            () => Assert.Equal(expected.ResultColumnMappings.Select(x => x), actual.ResultColumnMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())),
            () => Assert.Equal(expected.ParameterMappings.Select(x => x), actual.ParameterMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqualBase(
        IColumnBase expected,
        IColumnBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.IsNullable, actual.IsNullable),
            () => Assert.Equal(expected.ProviderClrType, actual.ProviderClrType),
            () => Assert.Equal(expected.StoreType, actual.StoreType),
            () => Assert.Equal(expected.StoreTypeMapping.StoreType, actual.StoreTypeMapping.StoreType),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IColumnBase expected,
        IColumnBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Same(actual, actual.Table.FindColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IColumn expected,
        IColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Same(actual, actual.Table.FindColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IViewColumn expected,
        IViewColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Same(actual, actual.View.FindColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        ISqlQueryColumn expected,
        ISqlQueryColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Same(actual, actual.SqlQuery.FindColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IFunctionColumn expected,
        IFunctionColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Same(actual, actual.Function.FindColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IStoreFunctionParameter expected,
        IStoreFunctionParameter actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.StoreType, actual.StoreType),
            () => Assert.Contains(actual, actual.Function.Parameters),
            () => Assert.Equal(expected.DbFunctionParameters.Select(x => x), actual.DbFunctionParameters,
                (expected, actual) =>
                {
                    Assert.Equal(expected.Name, actual.Name);
                    return true;
                }),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IStoreStoredProcedureResultColumn expected,
        IStoreStoredProcedureResultColumn actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.Position, actual.Position),
            () => Assert.Same(actual, actual.StoredProcedure.FindResultColumn(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqual(
        IStoreStoredProcedureParameter expected,
        IStoreStoredProcedureParameter actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Equal(expected.Position, actual.Position),
            () => Assert.Equal(expected.Direction, actual.Direction),
            () => Assert.Same(actual, actual.StoredProcedure.FindParameter(actual.Name)),
            () => Assert.Equal(expected.PropertyMappings.Select(x => x), actual.PropertyMappings,
                (expected, actual) =>
                    AssertEqual(
                        expected,
                        actual,
                        compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                        compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>())));

        return true;
    }

    public virtual bool AssertEqualBase(
        IColumnMappingBase expected,
        IColumnMappingBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Column.Name, actual.Column.Name),
            () => Assert.Equal(expected.Property.Name, actual.Property.Name),
            () => Assert.Equal(expected.TypeMapping.StoreType, actual.TypeMapping.StoreType),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IColumnMappingBase expected,
        IColumnMappingBase actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.TableMapping.ColumnMappings));

        return true;
    }

    public virtual bool AssertEqual(
        IColumnMapping expected,
        IColumnMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.TableMapping.ColumnMappings));

        return true;
    }
    public virtual bool AssertEqual(
        IViewColumnMapping expected,
        IViewColumnMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.ViewMapping.ColumnMappings));

        return true;
    }

    public virtual bool AssertEqual(
        ISqlQueryColumnMapping expected,
        ISqlQueryColumnMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.SqlQueryMapping.ColumnMappings));

        return true;
    }

    public virtual bool AssertEqual(
        IFunctionColumnMapping expected,
        IFunctionColumnMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.FunctionMapping.ColumnMappings));

        return true;
    }

    public virtual bool AssertEqual(
        IStoredProcedureResultColumnMapping expected,
        IStoredProcedureResultColumnMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.StoredProcedureMapping.ColumnMappings));

        return true;
    }

    public virtual bool AssertEqual(
        IStoredProcedureParameterMapping expected,
        IStoredProcedureParameterMapping actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => AssertEqualBase(expected, actual, expectedAnnotations, actualAnnotations),
            () => Assert.Contains(actual, actual.StoredProcedureMapping.ParameterMappings));

        return true;
    }

    public virtual bool AssertEqual(
        ITableIndex expected,
        ITableIndex actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Contains(actual, actual.Table.Indexes),
            () => Assert.Equal(expected.Columns.Select(c => c.Name), actual.Columns.Select(c => c.Name)),
            () => Assert.Equal(
                actual.MappedIndexes.Select(i => i.Properties.Select(p => p.Name)),
                expected.MappedIndexes.Select(i => i.Properties.Select(p => p.Name))),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IForeignKeyConstraint expected,
        IForeignKeyConstraint actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.OnDeleteAction, actual.OnDeleteAction),
            () => Assert.Equal(expected.PrincipalUniqueConstraint.Name, actual.PrincipalUniqueConstraint.Name),
            () => Assert.Equal(expected.PrincipalTable.SchemaQualifiedName, actual.PrincipalTable.SchemaQualifiedName),
            () => Assert.Contains(actual, actual.Table.ForeignKeyConstraints),
            () => Assert.Equal(expected.Columns.Select(c => c.Name), actual.Columns.Select(c => c.Name)),
            () => Assert.Equal(expected.PrincipalColumns.Select(c => c.Name), actual.PrincipalColumns.Select(c => c.Name)),
            () => Assert.Equal(
                actual.MappedForeignKeys.Select(i => i.Properties.Select(p => p.Name)),
                expected.MappedForeignKeys.Select(i => i.Properties.Select(p => p.Name))),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        IUniqueConstraint expected,
        IUniqueConstraint actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.GetIsPrimaryKey(), actual.GetIsPrimaryKey()),
            () => Assert.Contains(actual, actual.Table.UniqueConstraints),
            () => Assert.Equal(expected.Columns.Select(c => c.Name), actual.Columns.Select(c => c.Name)),
            () => Assert.Equal(
                actual.MappedKeys.Select(i => i.Properties.Select(p => p.Name)),
                expected.MappedKeys.Select(i => i.Properties.Select(p => p.Name))),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(
        ITrigger expected,
        ITrigger actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations)
    {
        Assert.Multiple(
            () => Assert.Equal(expected.ModelName, actual.ModelName),
            () => Assert.Equal(expected.GetTableName(), actual.GetTableName()),
            () => Assert.Equal(expected.GetTableSchema(), actual.GetTableSchema()),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }
}
