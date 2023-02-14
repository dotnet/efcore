using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Design;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer
{
    public class DesignTimeServicesTests
    {
        [Fact]
        public void ConfigureDesignTimeServices_works()
        {
            var serviceCollection = new ServiceCollection();
            new SqlServerHierarchyIdDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.IsType<SqlServerHierarchyIdTypeMappingSourcePlugin>(serviceProvider.GetService<IRelationalTypeMappingSourcePlugin>());
            Assert.IsType<SqlServerHierarchyIdCodeGeneratorPlugin>(serviceProvider.GetService<IProviderCodeGeneratorPlugin>());
        }
    }
}
