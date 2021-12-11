// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

#nullable enable

public class NonNullableNavigationConventionTest
{
    [ConditionalFact]
    public void Non_nullability_does_not_override_configuration_from_explicit_source()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Post>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention)!;

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Post.Blog),
            nameof(Blog.Posts),
            ConfigurationSource.Convention)!;

        var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation(nameof(Post.Blog))!;

        relationshipBuilder.IsRequired(false, ConfigurationSource.Explicit);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.Posts));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Post.Blog));
    }

    [ConditionalFact]
    public void Non_nullability_does_not_override_configuration_from_data_annotation()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Post>();
        var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention)!;

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
            principalEntityTypeBuilder.Metadata,
            nameof(Post.Blog),
            nameof(Blog.Posts),
            ConfigurationSource.Convention)!;

        var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation(nameof(Post.Blog))!;

        relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);
        Assert.Contains(principalEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Blog.Posts));
        Assert.Contains(dependentEntityTypeBuilder.Metadata.GetNavigations(), nav => nav.Name == nameof(Post.Blog));
    }

    [ConditionalFact]
    public void Non_nullability_does_not_set_is_required_for_collection_navigation()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention)!;

        var relationshipBuilder = principalEntityTypeBuilder.HasRelationship(
            dependentEntityTypeBuilder.Metadata,
            nameof(Principal.Dependents),
            nameof(Dependent.Principal),
            ConfigurationSource.Convention)!;

        var navigation = principalEntityTypeBuilder.Metadata.FindNavigation(nameof(Principal.Dependents))!;

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        Assert.Empty(ListLoggerFactory.Log);
    }

    [ConditionalFact]
    public void Non_nullability_does_not_set_is_required_for_navigation_to_dependent()
    {
        var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<Dependent>();
        var principalEntityTypeBuilder =
            dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Principal), ConfigurationSource.Convention)!;

        var relationshipBuilder = dependentEntityTypeBuilder.HasRelationship(
                principalEntityTypeBuilder.Metadata,
                nameof(Dependent.Principal),
                nameof(Principal.Dependent),
                ConfigurationSource.Convention)!
            .HasEntityTypes
                (principalEntityTypeBuilder.Metadata, dependentEntityTypeBuilder.Metadata, ConfigurationSource.Explicit)!;

        var navigation = principalEntityTypeBuilder.Metadata.FindNavigation(nameof(Principal.Dependent))!;

        Assert.False(relationshipBuilder.Metadata.IsRequired);

        RunConvention(relationshipBuilder, navigation);

        Assert.False(relationshipBuilder.Metadata.IsRequired);
    }

    [ConditionalFact]
    public void Non_nullability_sets_is_required_with_conventional_builder()
    {
        var modelBuilder = CreateModelBuilder();
        var model = (Model)modelBuilder.Model;
        modelBuilder.Entity<BlogDetails>();

        Assert.True(
            model.FindEntityType(typeof(BlogDetails))!.GetForeignKeys().Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Blog))
                .IsRequired);
    }

    private Navigation RunConvention(InternalForeignKeyBuilder relationshipBuilder, Navigation navigation)
    {
        var context = new ConventionContext<IConventionNavigationBuilder>(
            relationshipBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);
        CreateNotNullNavigationConvention().ProcessNavigationAdded(navigation.Builder, context);
        return context.ShouldStopProcessing() ? (Navigation)context.Result?.Metadata! : navigation;
    }

    private NonNullableNavigationConvention CreateNotNullNavigationConvention()
        => new(CreateDependencies());

    public ListLoggerFactory ListLoggerFactory { get; }
        = new(l => l == DbLoggerCategory.Model.Name);

    private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
    {
        var dependencies = CreateDependencies();
        // Use public API to add conventions, issue #214
        var conventionSet = new ConventionSet();
        conventionSet.EntityTypeAddedConventions.Add(
            new PropertyDiscoveryConvention(dependencies));

        conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention(dependencies));

        var modelBuilder = new Model(conventionSet).Builder;

        return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit)!;
    }

    private ModelBuilder CreateModelBuilder()
    {
        var serviceProvider = CreateServiceProvider();
        return new ModelBuilder(
            serviceProvider.GetRequiredService<IConventionSetBuilder>().CreateConventionSet(),
            serviceProvider.GetRequiredService<ModelDependencies>());
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => CreateServiceProvider().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    protected IServiceProvider CreateServiceProvider()
        => InMemoryTestHelpers.Instance.CreateContextServices(
            new ServiceCollection()
                .AddScoped<IDiagnosticsLogger<DbLoggerCategory.Model>>(_ => CreateLogger()));

    private DiagnosticsLogger<DbLoggerCategory.Model> CreateLogger()
    {
        ListLoggerFactory.Clear();
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

#pragma warning disable CS8618
    // ReSharper disable UnusedMember.Local
    // ReSharper disable ClassNeverInstantiated.Local
    private class Blog
    {
        public int Id { get; set; }

        [NotMapped]
        public BlogDetails BlogDetails { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    private class BlogDetails
    {
        public int Id { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        private Post Post { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }

        public Blog Blog { get; set; }
    }

    private class Principal
    {
        public static readonly PropertyInfo DependentIdProperty = typeof(Principal).GetProperty("DependentId")!;

        public int Id { get; set; }

        public int DependentId { get; set; }

        [ForeignKey("PrincipalFk")]
        public ICollection<Dependent> Dependents { get; set; }

        public Dependent Dependent { get; set; }
    }

    private class Dependent
    {
        public static readonly PropertyInfo PrincipalIdProperty = typeof(Dependent).GetProperty("PrincipalId")!;

        public int Id { get; set; }

        public int PrincipalId { get; set; }

        public int PrincipalFk { get; set; }

        [ForeignKey("AnotherPrincipal")]
        public int PrincipalAnotherFk { get; set; }

        [ForeignKey("PrincipalFk")]
        [InverseProperty("Dependent")]
        public Principal? Principal { get; set; }

        public Principal? AnotherPrincipal { get; set; }

        [ForeignKey("PrincipalId, PrincipalFk")]
        public Principal? CompositePrincipal { get; set; }
    }
    // ReSharper restore ClassNeverInstantiated.Local
    // ReSharper restore UnusedMember.Local
#pragma warning restore CS8618
}
