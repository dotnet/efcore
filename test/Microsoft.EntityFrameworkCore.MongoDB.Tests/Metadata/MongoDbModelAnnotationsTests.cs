#if !(NET451 && DRIVER_NOT_SIGNED)
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Metadata
{
    public class MongoDbModelAnnotationsTests
    {
        [Fact]
        public void Database_name_null_by_default()
        {
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(new Model());
            Assert.Null(mongoDbModelAnnotations.Database);
        }

        [Fact]
        public void Can_write_database_name()
        {
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(new Model()) { Database = "test" };
            Assert.Equal(expected: "test", actual: mongoDbModelAnnotations.Database);
        }

        [Fact]
        public void Complex_types_is_not_null_but_empty_by_default()
        {
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(new Model());
            Assert.NotNull(mongoDbModelAnnotations.ComplexTypes);
            Assert.Empty(mongoDbModelAnnotations.ComplexTypes);
        }

        [Fact]
        public void Can_add_complex_type()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(RootType), model, ConfigurationSource.Explicit);
            var mongoDbModelAnnotations = new MongoDbModelAnnotations(model)
            {
                ComplexTypes =
                {
                    entityType
                }
            };
            Assert.True(mongoDbModelAnnotations.ComplexTypes.Contains(entityType));
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)