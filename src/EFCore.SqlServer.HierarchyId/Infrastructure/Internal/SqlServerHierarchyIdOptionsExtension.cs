// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkSqlServerHierarchyId();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
        if (internalServiceProvider != null)
        {
            using (var scope = internalServiceProvider.CreateScope())
            {
                if (scope.ServiceProvider.GetService<IEnumerable<IMethodCallTranslatorPlugin>>()
                        ?.Any(s => s is SqlServerHierarchyIdMethodCallTranslatorPlugin)
                    != true
                    || scope.ServiceProvider.GetService<IEnumerable<IRelationalTypeMappingSourcePlugin>>()
                        ?.Any(s => s is SqlServerHierarchyIdTypeMappingSourcePlugin)
                    != true)
                {
                    throw new InvalidOperationException(SqlServerHierarchyIdStrings.ServicesMissing);
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

        public override bool IsDatabaseProvider
            => false;

        public override int GetServiceProviderHashCode()
            => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["SqlServer:" + nameof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions.UseHierarchyId)] = "1";

        public override string LogFragment
            => "using HierarchyId ";
    }
}
