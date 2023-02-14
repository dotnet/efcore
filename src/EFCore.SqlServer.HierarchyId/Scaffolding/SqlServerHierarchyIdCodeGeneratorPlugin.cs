using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.SqlServer.Scaffolding
{
    internal class SqlServerHierarchyIdCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
    {
        public override MethodCallCodeFragment GenerateProviderOptions()
        {
            return new MethodCallCodeFragment(
                typeof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions).GetRuntimeMethod(
                    nameof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions.UseHierarchyId),
                    new[] { typeof(SqlServerDbContextOptionsBuilder) }));
        }
    }
}
