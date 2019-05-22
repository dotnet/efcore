using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Configuration
{
    public class CosmosDbContextOptionsExtensionsTests
    {
        [Fact]
        public void Can_create_options_with_specified_region()
        {
            var regionName = CosmosRegions.EastAsia;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosDbOptionsExtension>();

            Assert.Equal(regionName, extension.Region);
        }

        /// <summary>
        /// The region will be checked by the cosmosdb sdk, because the region list is not constant
        /// </summary>
        [Fact]
        public void Can_create_options_with_wrong_region()
        {
            var regionName = "FakeRegion";
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosDbOptionsExtension>();

            Assert.Equal(regionName, extension.Region);
        }
    }
}
