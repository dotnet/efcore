// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Microsoft.EntityFrameworkCore;

public class RelationalApiConsistencyTest : ApiConsistencyTestBase<RelationalApiConsistencyTest.RelationalApiConsistencyFixture>
{
    public RelationalApiConsistencyTest(RelationalApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => new EntityFrameworkRelationalServicesBuilder(serviceCollection).TryAddCoreServices();

    protected override Assembly TargetAssembly
        => typeof(RelationalDatabase).Assembly;

    [ConditionalFact]
    public void Readonly_relational_metadata_methods_have_expected_name()
    {
        var errors =
            Fixture.RelationalMetadataMethods
                .SelectMany(m => m.Select(ValidateMethodName))
                .Where(e => e != null)
                .ToList();

        Assert.False(
            errors.Count > 0,
            "\r\n-- Errors: --\r\n" + string.Join(Environment.NewLine, errors));
    }

    public class RelationalApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        private static Dictionary<Type, (Type Mutable, Type Convention, Type ConventionBuilder, Type Runtime)> _metadataTypes
            => new()
            {
                {
                    typeof(IReadOnlyDbFunction),
                    (typeof(IMutableDbFunction),
                        typeof(IConventionDbFunction),
                        typeof(IConventionDbFunctionBuilder),
                        typeof(IDbFunction))
                },
                {
                    typeof(IReadOnlyDbFunctionParameter),
                    (typeof(IMutableDbFunctionParameter),
                        typeof(IConventionDbFunctionParameter),
                        typeof(IConventionDbFunctionParameterBuilder),
                        typeof(IDbFunctionParameter))
                },
                {
                    typeof(IReadOnlyStoredProcedure),
                    (typeof(IMutableStoredProcedure),
                        typeof(IConventionStoredProcedure),
                        typeof(IConventionStoredProcedureBuilder),
                        typeof(IStoredProcedure))
                },
                {
                    typeof(IReadOnlySequence),
                    (typeof(IMutableSequence),
                        typeof(IConventionSequence),
                        typeof(IConventionSequenceBuilder),
                        typeof(ISequence))
                },
                {
                    typeof(IReadOnlyCheckConstraint),
                    (typeof(IMutableCheckConstraint),
                        typeof(IConventionCheckConstraint),
                        typeof(IConventionCheckConstraintBuilder),
                        typeof(ICheckConstraint))
                },
                {
                    typeof(IReadOnlyTrigger),
                    (typeof(IMutableTrigger),
                        typeof(IConventionTrigger),
                        typeof(IConventionTriggerBuilder),
                        typeof(ITrigger))
                },
                {
                    typeof(IReadOnlyEntityTypeMappingFragment),
                    (typeof(IMutableEntityTypeMappingFragment),
                        typeof(IConventionEntityTypeMappingFragment),
                        null,
                        typeof(IEntityTypeMappingFragment))
                },
                {
                    typeof(IReadOnlyRelationalPropertyOverrides),
                    (typeof(IMutableRelationalPropertyOverrides),
                        typeof(IConventionRelationalPropertyOverrides),
                        null,
                        typeof(IRelationalPropertyOverrides))
                }
            };

        public virtual HashSet<Type> RelationalMetadataTypes { get; } = new()
        {
            typeof(IRelationalModel),
            typeof(ITableBase),
            typeof(ITable),
            typeof(IView),
            typeof(IStoreFunction),
            typeof(ITableMappingBase),
            typeof(ITableMapping),
            typeof(IViewMapping),
            typeof(IFunctionMapping),
            typeof(IColumnBase),
            typeof(IColumn),
            typeof(IViewColumn),
            typeof(IFunctionColumn),
            typeof(IStoreFunctionParameter),
            typeof(IFunctionColumnMapping),
            typeof(IColumnMappingBase),
            typeof(IColumnMapping),
            typeof(IViewColumnMapping),
            typeof(ITableIndex),
            typeof(IForeignKeyConstraint),
            typeof(IUniqueConstraint),
            typeof(ITrigger),
            typeof(IEntityTypeMappingFragment),
            typeof(IRelationalPropertyOverrides)
        };

        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(RelationalForeignKeyBuilderExtensions),
            typeof(RelationalPropertyBuilderExtensions),
            typeof(RelationalModelBuilderExtensions),
            typeof(RelationalIndexBuilderExtensions),
            typeof(RelationalKeyBuilderExtensions),
            typeof(RelationalEntityTypeBuilderExtensions),
            typeof(DbFunctionBuilder),
            typeof(DbFunctionParameterBuilder),
            typeof(TableBuilder),
            typeof(TableBuilder<>),
            typeof(OwnedNavigationTableBuilder),
            typeof(OwnedNavigationTableBuilder<,>),
            typeof(SplitTableBuilder),
            typeof(SplitTableBuilder<>),
            typeof(OwnedNavigationSplitTableBuilder),
            typeof(OwnedNavigationSplitTableBuilder<,>),
            typeof(ViewBuilder),
            typeof(ViewBuilder<>),
            typeof(OwnedNavigationViewBuilder),
            typeof(OwnedNavigationViewBuilder<,>),
            typeof(SplitViewBuilder),
            typeof(SplitViewBuilder<>),
            typeof(OwnedNavigationSplitViewBuilder),
            typeof(OwnedNavigationSplitViewBuilder<,>),
            typeof(TableValuedFunctionBuilder),
            typeof(TableValuedFunctionBuilder<>),
            typeof(OwnedNavigationTableValuedFunctionBuilder),
            typeof(OwnedNavigationTableValuedFunctionBuilder<,>),
            typeof(StoredProcedureBuilder),
            typeof(StoredProcedureBuilder<>),
            typeof(OwnedNavigationStoredProcedureBuilder),
            typeof(OwnedNavigationStoredProcedureBuilder<,>),
            typeof(ColumnBuilder),
            typeof(ColumnBuilder<>),
            typeof(ViewColumnBuilder),
            typeof(ViewColumnBuilder<>),
            typeof(StoredProcedureParameterBuilder),
            typeof(StoredProcedureResultColumnBuilder),
            typeof(SequenceBuilder),
            typeof(MigrationBuilder),
            typeof(AlterOperationBuilder<>),
            typeof(ColumnsBuilder),
            typeof(CreateTableBuilder<>),
            typeof(OperationBuilder<>)
        };

        public override
            List<(Type Type,
                Type ReadonlyExtensions,
                Type MutableExtensions,
                Type ConventionExtensions,
                Type ConventionBuilderExtensions,
                Type RuntimeExtensions)> MetadataExtensionTypes { get; }
            = new()
            {
                (
                    typeof(IReadOnlyModel),
                    typeof(RelationalModelExtensions),
                    typeof(RelationalModelExtensions),
                    typeof(RelationalModelExtensions),
                    typeof(RelationalModelBuilderExtensions),
                    null
                ),
                (
                    typeof(IReadOnlyEntityType),
                    typeof(RelationalEntityTypeExtensions),
                    typeof(RelationalEntityTypeExtensions),
                    typeof(RelationalEntityTypeExtensions),
                    typeof(RelationalEntityTypeBuilderExtensions),
                    null
                ),
                (
                    typeof(IReadOnlyKey),
                    typeof(RelationalKeyExtensions),
                    typeof(RelationalKeyExtensions),
                    typeof(RelationalKeyExtensions),
                    typeof(RelationalKeyBuilderExtensions),
                    null
                ),
                (
                    typeof(IReadOnlyForeignKey),
                    typeof(RelationalForeignKeyExtensions),
                    typeof(RelationalForeignKeyExtensions),
                    typeof(RelationalForeignKeyExtensions),
                    typeof(RelationalForeignKeyBuilderExtensions),
                    null
                ),
                (
                    typeof(IReadOnlyProperty),
                    typeof(RelationalPropertyExtensions),
                    typeof(RelationalPropertyExtensions),
                    typeof(RelationalPropertyExtensions),
                    typeof(RelationalPropertyBuilderExtensions),
                    null
                ),
                (
                    typeof(IReadOnlyIndex),
                    typeof(RelationalIndexExtensions),
                    typeof(RelationalIndexExtensions),
                    typeof(RelationalIndexExtensions),
                    typeof(RelationalIndexBuilderExtensions),
                    null
                )
            };

        public override HashSet<MethodInfo> NonVirtualMethods { get; }
            = new()
            {
                typeof(RelationalCompiledQueryCacheKeyGenerator)
                    .GetRuntimeMethods()
                    .Single(
                        m => m.Name == "GenerateCacheKeyCore"
                            && m.DeclaringType == typeof(RelationalCompiledQueryCacheKeyGenerator))
            };

        public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new()
        {
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ExcludeTableFromMigrations)),
            typeof(RelationalIndexBuilderExtensions).GetMethod(
                nameof(RelationalIndexBuilderExtensions.HasName),
                new[] { typeof(IndexBuilder), typeof(string) }),
            typeof(RelationalPropertyExtensions).GetMethod(
                nameof(RelationalPropertyExtensions.FindOverrides),
                new[] { typeof(IReadOnlyProperty), typeof(StoreObjectIdentifier).MakeByRefType() }),
            typeof(RelationalPropertyExtensions).GetMethod(
                nameof(RelationalPropertyExtensions.GetOverrides),
                new[] { typeof(IReadOnlyProperty) }),
            GetMethod(
                typeof(StoredProcedureBuilder<>),
                    nameof(StoredProcedureBuilder<object>.HasParameter),
                genericParameterCount: 2,
                    (typeTypes, methodTypes) => new[]
                    {
                        typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(methodTypes[0], methodTypes[1]))
                    }),
            GetMethod(
                typeof(StoredProcedureBuilder<>),
                nameof(StoredProcedureBuilder<object>.HasParameter),
                genericParameterCount: 2,
                (typeTypes, methodTypes) => new[]
                {
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(methodTypes[0], methodTypes[1])),
                    typeof(Action<StoredProcedureParameterBuilder>)
                }),
            GetMethod(
                typeof(StoredProcedureBuilder<>),
                nameof(StoredProcedureBuilder<object>.HasResultColumn),
                genericParameterCount: 2,
                (typeTypes, methodTypes) => new[]
                {
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(methodTypes[0], methodTypes[1]))
                }),
            GetMethod(
                typeof(StoredProcedureBuilder<>),
                nameof(StoredProcedureBuilder<object>.HasResultColumn),
                genericParameterCount: 2,
                (typeTypes, methodTypes) => new[]
                {
                    typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(methodTypes[0], methodTypes[1])),
                    typeof(Action<StoredProcedureResultColumnBuilder>)
                })
        };

        public override HashSet<MethodInfo> AsyncMethodExceptions { get; } = new()
        {
            typeof(RelationalDatabaseFacadeExtensions).GetMethod(nameof(RelationalDatabaseFacadeExtensions.CloseConnectionAsync)),
            typeof(IRelationalConnection).GetMethod(nameof(IRelationalConnection.CloseAsync)),
            typeof(RelationalConnection).GetMethod(nameof(RelationalConnection.CloseAsync)),
            typeof(RelationalConnection).GetMethod("CloseDbConnectionAsync", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(DbConnectionInterceptor).GetMethod(nameof(DbConnectionInterceptor.ConnectionClosingAsync)),
            typeof(DbConnectionInterceptor).GetMethod(nameof(DbConnectionInterceptor.ConnectionClosedAsync)),
            typeof(IDbConnectionInterceptor).GetMethod(nameof(IDbConnectionInterceptor.ConnectionClosingAsync)),
            typeof(IDbConnectionInterceptor).GetMethod(nameof(IDbConnectionInterceptor.ConnectionClosedAsync)),
            typeof(IDbConnectionInterceptor).GetMethod(nameof(IDbConnectionInterceptor.ConnectionDisposingAsync)),
            typeof(IDbConnectionInterceptor).GetMethod(nameof(IDbConnectionInterceptor.ConnectionDisposedAsync)),
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosingAsync)),
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosedAsync)),
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionDisposingAsync)),
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionDisposedAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosingAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosedAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                    nameof(IRelationalConnectionDiagnosticsLogger.ConnectionDisposingAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionDisposedAsync))
        };
        
        public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new()
        {
            typeof(IMutableStoredProcedure).GetMethod(nameof(IMutableStoredProcedure.AddParameter)),
            typeof(IMutableStoredProcedure).GetMethod(nameof(IMutableStoredProcedure.AddResultColumn))
        };

        public List<IReadOnlyList<MethodInfo>> RelationalMetadataMethods { get; } = new();

        protected override void Initialize()
        {
            AddInstanceMethods(_metadataTypes);
            foreach (var typeTuple in _metadataTypes)
            {
                MetadataTypes.Add(typeTuple.Key, typeTuple.Value);
            }

            foreach (var metadataType in RelationalMetadataTypes)
            {
                var readOnlyMethods = metadataType.GetMethods(PublicInstance)
                    .Where(m => !IsObsolete(m)).ToArray();
                RelationalMetadataMethods.Add(readOnlyMethods);
            }

            GenericFluentApiTypes.Add(typeof(TableBuilder), typeof(TableBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationTableBuilder), typeof(OwnedNavigationTableBuilder<,>));
            GenericFluentApiTypes.Add(typeof(SplitTableBuilder), typeof(SplitTableBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationSplitTableBuilder), typeof(OwnedNavigationSplitTableBuilder<,>));
            GenericFluentApiTypes.Add(typeof(ViewBuilder), typeof(ViewBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationViewBuilder), typeof(OwnedNavigationViewBuilder<,>));
            GenericFluentApiTypes.Add(typeof(SplitViewBuilder), typeof(SplitViewBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationSplitViewBuilder), typeof(OwnedNavigationSplitViewBuilder<,>));
            GenericFluentApiTypes.Add(typeof(TableValuedFunctionBuilder), typeof(TableValuedFunctionBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationTableValuedFunctionBuilder), typeof(OwnedNavigationTableValuedFunctionBuilder<,>));
            GenericFluentApiTypes.Add(typeof(StoredProcedureBuilder), typeof(StoredProcedureBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationStoredProcedureBuilder), typeof(OwnedNavigationStoredProcedureBuilder<,>));
            GenericFluentApiTypes.Add(typeof(ColumnBuilder), typeof(ColumnBuilder<>));
            GenericFluentApiTypes.Add(typeof(ViewColumnBuilder), typeof(ViewColumnBuilder<>));

            MirrorTypes.Add(typeof(TableBuilder), typeof(OwnedNavigationTableBuilder));
            MirrorTypes.Add(typeof(TableBuilder<>), typeof(OwnedNavigationTableBuilder<,>));
            MirrorTypes.Add(typeof(SplitTableBuilder), typeof(OwnedNavigationSplitTableBuilder));
            MirrorTypes.Add(typeof(SplitTableBuilder<>), typeof(OwnedNavigationSplitTableBuilder<,>));
            MirrorTypes.Add(typeof(ViewBuilder), typeof(OwnedNavigationViewBuilder));
            MirrorTypes.Add(typeof(ViewBuilder<>), typeof(OwnedNavigationViewBuilder<,>));
            MirrorTypes.Add(typeof(SplitViewBuilder), typeof(OwnedNavigationSplitViewBuilder));
            MirrorTypes.Add(typeof(SplitViewBuilder<>), typeof(OwnedNavigationSplitViewBuilder<,>));
            MirrorTypes.Add(typeof(TableValuedFunctionBuilder), typeof(OwnedNavigationTableValuedFunctionBuilder));
            MirrorTypes.Add(typeof(TableValuedFunctionBuilder<>), typeof(OwnedNavigationTableValuedFunctionBuilder<,>));
            MirrorTypes.Add(typeof(StoredProcedureBuilder), typeof(OwnedNavigationStoredProcedureBuilder));
            MirrorTypes.Add(typeof(StoredProcedureBuilder<>), typeof(OwnedNavigationStoredProcedureBuilder<,>));

            base.Initialize();
        }
    }
}
