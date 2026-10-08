namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class ModelValidatorTest : ModelValidatorTestBase
{
    [ConditionalFact]
    public virtual void Allows_non_key_property_not_auto_loaded()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<AutoLoadEntity>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.Property(e => e.Name);
            });

        var model = modelBuilder.Model;
        var property = model.FindEntityType(typeof(AutoLoadEntity))!.FindProperty(nameof(AutoLoadEntity.Name))!;
        property.IsAutoLoaded = false;

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_constructor_bound_property_not_auto_loaded()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<AutoLoadEntityWithConstructor>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.Property(e => e.Name);
            });

        var model = modelBuilder.Model;
        var property = model.FindEntityType(typeof(AutoLoadEntityWithConstructor))!
            .FindProperty(nameof(AutoLoadEntityWithConstructor.Name))!;
        property.IsAutoLoaded = false;

        VerifyError(
            CoreStrings.AutoLoadedConstructorProperty(
                nameof(AutoLoadEntityWithConstructor.Name),
                nameof(AutoLoadEntityWithConstructor)),
            modelBuilder);
    }

    protected class AutoLoadEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    protected class AutoLoadEntityWithConstructor
    {
        public AutoLoadEntityWithConstructor(string name)
            => Name = name;

        public int Id { get; set; }
        public string Name { get; set; }
    }
}