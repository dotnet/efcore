namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IModelBuilderConvention
    {
        void Apply(ModelBuilder modelBuilder);
    }
}