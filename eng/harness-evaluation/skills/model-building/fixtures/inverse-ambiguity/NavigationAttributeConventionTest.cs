namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class NavigationAttributeConventionTest
{
    [Fact]
    public void InversePropertyAttribute_does_not_configure_ambiguous_navigations()
    {
        var dependentBuilder = CreateInternalEntityTypeBuilder<AmbiguousDependent>();
        var principalBuilder = dependentBuilder.ModelBuilder.Entity(
            typeof(AmbiguousPrincipal), ConfigurationSource.Convention)!;

        dependentBuilder.HasRelationship(
            principalBuilder.Metadata,
            nameof(AmbiguousDependent.Principal),
            nameof(AmbiguousPrincipal.Dependent),
            ConfigurationSource.Convention);

        var convention = new InversePropertyAttributeConvention(CreateDependencies());
        convention.ProcessEntityTypeAdded(
            dependentBuilder,
            new ConventionContext<IConventionEntityTypeBuilder>(
                dependentBuilder.Metadata.Model.ConventionDispatcher));

        Assert.Empty(principalBuilder.Metadata.GetNavigations());
        Assert.Empty(dependentBuilder.Metadata.GetNavigations());
    }

    private class AmbiguousDependent
    {
        public int Id { get; set; }

        [InverseProperty(nameof(AmbiguousPrincipal.Dependent))]
        public AmbiguousPrincipal Principal { get; set; } = null!;

        [InverseProperty(nameof(AmbiguousPrincipal.Dependent))]
        public AmbiguousPrincipal AlternatePrincipal { get; set; } = null!;
    }

    private class AmbiguousPrincipal
    {
        public int Id { get; set; }
        public AmbiguousDependent Dependent { get; set; } = null!;
    }
}