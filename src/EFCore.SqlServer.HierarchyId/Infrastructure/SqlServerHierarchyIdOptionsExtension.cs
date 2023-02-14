using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Properties;
using Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.SqlServer.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure
{
    internal class SqlServerHierarchyIdOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;

        public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

        public virtual void ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlServerHierarchyId();
        }

        public virtual void Validate(IDbContextOptions options)
        {
            var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                using (var scope = internalServiceProvider.CreateScope())
                {
                    if (scope.ServiceProvider.GetService<IEnumerable<IMethodCallTranslatorPlugin>>()
                            ?.Any(s => s is SqlServerHierarchyIdMethodCallTranslatorPlugin) != true ||
                        scope.ServiceProvider.GetService<IEnumerable<IRelationalTypeMappingSourcePlugin>>()
                           ?.Any(s => s is SqlServerHierarchyIdTypeMappingSourcePlugin) != true)
                    {
                        throw new InvalidOperationException(Resources.ServicesMissing);
                    }
                }
            }
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new SqlServerHierarchyIdOptionsExtension Extension
                => (SqlServerHierarchyIdOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => false;

            public override int GetServiceProviderHashCode() => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
                => other is ExtensionInfo;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["SqlServer:" + nameof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions.UseHierarchyId)] = "1";

            public override string LogFragment => "using HierarchyId ";
        }
    }
}
