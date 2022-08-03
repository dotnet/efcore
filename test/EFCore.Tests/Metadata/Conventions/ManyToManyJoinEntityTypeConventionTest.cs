// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ManyToManyJoinEntityTypeConventionTest
{
    [ConditionalFact]
    public void Join_entity_type_is_created_for_self_join()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManySelf = modelBuilder.Entity(typeof(ManyToManySelf), ConfigurationSource.Convention);

        manyToManySelf.PrimaryKey(new[] { nameof(ManyToManySelf.Id) }, ConfigurationSource.Convention);

        var firstSkipNav = manyToManySelf.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySelf).GetProperty(nameof(ManyToManySelf.ManyToManySelf1))),
            manyToManySelf.Metadata,
            ConfigurationSource.Convention);
        var secondSkipNav = manyToManySelf.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySelf).GetProperty(nameof(ManyToManySelf.ManyToManySelf2))),
            manyToManySelf.Metadata,
            ConfigurationSource.Convention);
        firstSkipNav.HasInverse(secondSkipNav.Metadata, ConfigurationSource.Convention);

        RunConvention(firstSkipNav);

        var joinEntityType = manyToManySelf.Metadata.Model.GetEntityTypes()
            .Single(et => et.IsImplicitlyCreatedJoinEntityType);
        Assert.Equal("ManyToManySelfManyToManySelf", joinEntityType.Name);
    }

    [ConditionalFact]
    public void Join_entity_type_is_not_created_when_no_inverse_skip_navigation()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);

        var manyToManyFirstPK = manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        var manyToManySecondPK = manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.ManyToManySeconds))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.ManyToManyFirsts))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention);
        // do not set SkipNav's as inverses of one another

        RunConvention(skipNavOnFirst);

        Assert.Empty(
            manyToManyFirst.Metadata.Model.GetEntityTypes()
                .Where(et => et.IsImplicitlyCreatedJoinEntityType));
    }

    [ConditionalFact]
    public void Join_entity_type_is_not_created_when_skip_navigation_is_not_collection()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);

        var manyToManyFirstPK = manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        var manyToManySecondPK = manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.Second))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention,
            collection: false);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.ManyToManyFirsts))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention);
        skipNavOnFirst.HasInverse(skipNavOnSecond.Metadata, ConfigurationSource.Convention);

        RunConvention(skipNavOnFirst);

        Assert.Empty(
            manyToManyFirst.Metadata.Model.GetEntityTypes()
                .Where(et => et.IsImplicitlyCreatedJoinEntityType));
    }

    [ConditionalFact]
    public void Join_entity_type_is_not_created_when_inverse_skip_navigation_is_not_collection()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);

        var manyToManyFirstPK = manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        var manyToManySecondPK = manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.ManyToManySeconds))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.First))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention,
            collection: false);
        skipNavOnFirst.HasInverse(skipNavOnSecond.Metadata, ConfigurationSource.Convention);

        RunConvention(skipNavOnFirst);

        Assert.Empty(
            manyToManyFirst.Metadata.Model.GetEntityTypes()
                .Where(et => et.IsImplicitlyCreatedJoinEntityType));
    }

    [ConditionalFact]
    public void Join_entity_type_is_not_created_when_skip_navigation_already_in_use()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);

        var manyToManyFirstPK = manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        var manyToManySecondPK = manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.ManyToManySeconds))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.ManyToManyFirsts))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention);
        skipNavOnFirst.HasInverse(skipNavOnSecond.Metadata, ConfigurationSource.Convention);

        // assign a non-null foreign key to skipNavOnFirst to make it appear to be "in use"
        var leftFK = manyToManyJoin.HasRelationship(
            manyToManyFirst.Metadata.Name,
            new[] { nameof(ManyToManyJoin.LeftId) },
            manyToManyFirstPK.Metadata,
            ConfigurationSource.Convention);
        skipNavOnFirst.Metadata.SetForeignKey(leftFK.Metadata, ConfigurationSource.Convention);

        RunConvention(skipNavOnFirst);

        Assert.Empty(
            manyToManyFirst.Metadata.Model.GetEntityTypes()
                .Where(et => et.IsImplicitlyCreatedJoinEntityType));
    }

    [ConditionalFact]
    public void Join_entity_type_is_not_created_when_inverse_skip_navigation_already_in_use()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);

        var manyToManyFirstPK = manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        var manyToManySecondPK = manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.ManyToManySeconds))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.ManyToManyFirsts))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention);
        skipNavOnFirst.HasInverse(skipNavOnSecond.Metadata, ConfigurationSource.Convention);

        // assign a non-null foreign key to skipNavOnSecond to make it appear to be "in use"
        var rightFK = manyToManyJoin.HasRelationship(
            manyToManySecond.Metadata.Name,
            new[] { nameof(ManyToManyJoin.RightId) },
            manyToManySecondPK.Metadata,
            ConfigurationSource.Convention);
        skipNavOnSecond.Metadata.SetForeignKey(rightFK.Metadata, ConfigurationSource.Convention);

        RunConvention(skipNavOnFirst);

        Assert.Empty(
            manyToManyFirst.Metadata.Model.GetEntityTypes()
                .Where(et => et.IsImplicitlyCreatedJoinEntityType));
    }

    [ConditionalFact]
    public void Join_entity_type_is_created()
    {
        var modelBuilder = CreateInternalModeBuilder();
        var manyToManyFirst = modelBuilder.Entity(typeof(ManyToManyFirst), ConfigurationSource.Convention);
        var manyToManySecond = modelBuilder.Entity(typeof(ManyToManySecond), ConfigurationSource.Convention);

        manyToManyFirst.PrimaryKey(new[] { nameof(ManyToManyFirst.Id) }, ConfigurationSource.Convention);
        manyToManySecond.PrimaryKey(new[] { nameof(ManyToManySecond.Id) }, ConfigurationSource.Convention);

        var skipNavOnFirst = manyToManyFirst.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyFirst).GetProperty(nameof(ManyToManyFirst.ManyToManySeconds))),
            manyToManySecond.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnSecond = manyToManySecond.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManySecond).GetProperty(nameof(ManyToManySecond.ManyToManyFirsts))),
            manyToManyFirst.Metadata,
            ConfigurationSource.Convention);
        skipNavOnFirst.HasInverse(skipNavOnSecond.Metadata, ConfigurationSource.Convention);

        RunConvention(skipNavOnSecond);

        var joinEntityType = manyToManyFirst.Metadata.Model.GetEntityTypes()
            .Single(et => et.IsImplicitlyCreatedJoinEntityType);

        Assert.Equal("ManyToManyFirstManyToManySecond", joinEntityType.Name);

        var skipNavOnManyToManyFirst = manyToManyFirst.Metadata.GetSkipNavigations().Single();
        var skipNavOnManyToManySecond = manyToManySecond.Metadata.GetSkipNavigations().Single();
        Assert.Equal("ManyToManySeconds", skipNavOnManyToManyFirst.Name);
        Assert.Equal("ManyToManyFirsts", skipNavOnManyToManySecond.Name);
        Assert.Same(skipNavOnManyToManyFirst.Inverse, skipNavOnManyToManySecond);
        Assert.Same(skipNavOnManyToManySecond.Inverse, skipNavOnManyToManyFirst);

        var manyToManyFirstForeignKey = skipNavOnManyToManyFirst.ForeignKey;
        var manyToManySecondForeignKey = skipNavOnManyToManySecond.ForeignKey;
        Assert.NotNull(manyToManyFirstForeignKey);
        Assert.NotNull(manyToManySecondForeignKey);
        Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
        Assert.Equal(manyToManyFirstForeignKey.DeclaringEntityType, joinEntityType);
        Assert.Equal(manyToManySecondForeignKey.DeclaringEntityType, joinEntityType);
    }

    public ListLoggerFactory ListLoggerFactory { get; }
        = new(l => l == DbLoggerCategory.Model.Name);

    private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
        var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
            ListLoggerFactory,
            options,
            new DiagnosticListener("Fake"),
            new TestLoggingDefinitions(),
            new NullDbContextLogger());
        return modelLogger;
    }

    private InternalSkipNavigationBuilder RunConvention(InternalSkipNavigationBuilder skipNavBuilder)
    {
        var context = new ConventionContext<IConventionSkipNavigationBuilder>(
            skipNavBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);
        CreateManyToManyConvention().ProcessSkipNavigationAdded(skipNavBuilder, context);
        return context.ShouldStopProcessing() ? (InternalSkipNavigationBuilder)context.Result : skipNavBuilder;
    }

    private ManyToManyJoinEntityTypeConvention CreateManyToManyConvention()
        => new(CreateDependencies());

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>()
            with
            {
                Logger = CreateLogger()
            };

    private InternalModelBuilder CreateInternalModeBuilder()
        => new Model().Builder;

    private class ManyToManyFirst
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManySecond> ManyToManySeconds { get; set; }
        public ManyToManySecond Second { get; set; }
    }

    private class ManyToManySecond
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManyFirst> ManyToManyFirsts { get; set; }
        public ManyToManyFirst First { get; set; }
    }

    private class ManyToManySelf
    {
        public int Id { get; set; }
        public IEnumerable<ManyToManySelf> ManyToManySelf1 { get; set; }
        public IEnumerable<ManyToManySelf> ManyToManySelf2 { get; set; }
    }

    private class ManyToManyJoin
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }
    }
}
