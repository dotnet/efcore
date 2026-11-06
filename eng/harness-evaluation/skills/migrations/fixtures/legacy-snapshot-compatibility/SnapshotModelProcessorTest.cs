namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

public class SnapshotModelProcessorTest
{
    [ConditionalFact]
    public void Updates_complex_property_nullability_for_pre_10_snapshots()
        => AssertComplexPropertyNullability("9.0.0", expectedConfiguration: true);

    [ConditionalFact]
    public void Does_not_update_complex_property_nullability_for_10_or_later_snapshots()
        => AssertComplexPropertyNullability("10.0.0", expectedConfiguration: false);

    [ConditionalFact]
    public void Updates_nested_complex_property_nullability_for_pre_10_snapshots()
    {
        var builder = new ModelBuilder();
        ((Model)builder.Model).SetProductVersion("9.0.0");

        var entityType = builder.Entity<EntityWithNestedComplexProperty>();
        entityType.ComplexProperty(
            e => e.OuterComplexProperty,
            outer => outer.ComplexProperty(
                e => e.InnerComplexProperty,
                inner => inner.Property(e => e.Value)));

        var outerProperty = entityType.Metadata.GetComplexProperties().Single();
        var innerProperty = outerProperty.ComplexType.GetComplexProperties().Single();

        new SnapshotModelProcessor(new TestOperationReporter(), DummyModelRuntimeInitializer.Instance)
            .Process(builder.Model);

        Assert.False(outerProperty.IsNullable);
        Assert.False(innerProperty.IsNullable);
    }

    private static void AssertComplexPropertyNullability(string version, bool expectedConfiguration)
    {
        var builder = new ModelBuilder();
        ((Model)builder.Model).SetProductVersion(version);

        var entityType = builder.Entity<EntityWithComplexProperty>();
        entityType.ComplexProperty(e => e.StructComplexProperty, b => b.Property(c => c.Value));
        var complexProperty = (ComplexProperty)entityType.Metadata.GetComplexProperties().Single();

        new SnapshotModelProcessor(new TestOperationReporter(), DummyModelRuntimeInitializer.Instance)
            .Process(builder.Model);

        Assert.Equal(expectedConfiguration, complexProperty.GetIsNullableConfigurationSource() != null);
    }
}