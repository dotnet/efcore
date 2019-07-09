using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class CosmosDbContextOptionsExtensionsTests
    {
        [ConditionalFact]
        public void Can_create_options_with_specified_region()
        {
            var regionName = Regions.EastAsia;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(regionName, extension.Region);
        }

        [ConditionalFact]
        public void Can_create_options_with_wrong_region()
        {
            var regionName = "FakeRegion";
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosOptionsExtension>();

            // The region will be validated by the Cosmos SDK, because the region list is not constant
            Assert.Equal(regionName, extension.Region);
        }
    }
}
