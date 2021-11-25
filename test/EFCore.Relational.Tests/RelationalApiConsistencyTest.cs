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
                    typeof(IReadOnlyDbFunction), (typeof(IMutableDbFunction),
                        typeof(IConventionDbFunction),
                        typeof(IConventionDbFunctionBuilder),
                        typeof(IDbFunction))
                },
                {
                    typeof(IReadOnlyDbFunctionParameter), (typeof(IMutableDbFunctionParameter),
                        typeof(IConventionDbFunctionParameter),
                        typeof(IConventionDbFunctionParameterBuilder),
                        typeof(IDbFunctionParameter))
                },
                {
                    typeof(IReadOnlySequence), (typeof(IMutableSequence),
                        typeof(IConventionSequence),
                        typeof(IConventionSequenceBuilder),
                        typeof(ISequence))
                },
                {
                    typeof(IReadOnlyCheckConstraint), (typeof(IMutableCheckConstraint),
                        typeof(IConventionCheckConstraint),
                        null,
                        typeof(ICheckConstraint))
                }
            };

        public virtual HashSet<Type> RelationalMetadataTypes { get; } = new()
        {
            typeof(IRelationalModel),
            typeof(ITableBase),
            typeof(ITable),
            typeof(IView),
            typeof(ITableMappingBase),
            typeof(ITableMapping),
            typeof(IViewMapping),
            typeof(IColumnBase),
            typeof(IColumn),
            typeof(IViewColumn),
            typeof(IColumnMappingBase),
            typeof(IColumnMapping),
            typeof(IViewColumnMapping),
            typeof(ITableIndex),
            typeof(IForeignKeyConstraint),
            typeof(IUniqueConstraint)
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
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.CanSetFunction),
                new[] { typeof(IConventionEntityTypeBuilder), typeof(MethodInfo), typeof(bool) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToFunction),
                new[] { typeof(IConventionEntityTypeBuilder), typeof(string), typeof(bool) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(EntityTypeBuilder), typeof(Action<TableBuilder>) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(EntityTypeBuilder), typeof(string), typeof(Action<TableBuilder>) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(EntityTypeBuilder), typeof(string), typeof(string), typeof(Action<TableBuilder>) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(OwnedNavigationBuilder), typeof(Action<OwnedNavigationTableBuilder>) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(OwnedNavigationBuilder), typeof(string), typeof(Action<OwnedNavigationTableBuilder>) }),
            typeof(RelationalEntityTypeBuilderExtensions).GetMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                new[] { typeof(OwnedNavigationBuilder), typeof(string), typeof(string), typeof(Action<OwnedNavigationTableBuilder>) }),
            typeof(RelationalIndexBuilderExtensions).GetMethod(
                nameof(RelationalIndexBuilderExtensions.HasName),
                new[] { typeof(IndexBuilder), typeof(string) })
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
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosingAsync)),
            typeof(IRelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosedAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosingAsync)),
            typeof(RelationalConnectionDiagnosticsLogger).GetMethod(
                nameof(IRelationalConnectionDiagnosticsLogger.ConnectionClosedAsync))
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

            base.Initialize();
        }
    }
}
