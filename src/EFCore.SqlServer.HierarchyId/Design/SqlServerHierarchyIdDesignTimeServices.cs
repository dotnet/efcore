using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding;
using Microsoft.EntityFrameworkCore.SqlServer.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design
{
    /// <summary>
    ///     Enables configuring design-time services. Tools will automatically discover implementations of this
    ///     interface that are in the startup assembly.
    /// </summary>
    public class SqlServerHierarchyIdDesignTimeServices : IDesignTimeServices
    {
        /// <summary>
        ///     Configures design-time services. Use this method to override the default design-time services with your
        ///     own implementations.
        /// </summary>
        /// <param name="serviceCollection"> The design-time service collection. </param>
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IRelationalTypeMappingSourcePlugin, SqlServerHierarchyIdTypeMappingSourcePlugin>()
                .AddSingleton<IProviderCodeGeneratorPlugin, SqlServerHierarchyIdCodeGeneratorPlugin>();
        }
    }
}
