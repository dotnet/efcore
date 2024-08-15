// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;

namespace Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryOptionsExtension : IDbContextOptionsExtension
{
    private string? _storeName;
    private bool _nullabilityCheckEnabled;
    private InMemoryDatabaseRoot? _databaseRoot;
    private DbContextOptionsExtensionInfo? _info;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryOptionsExtension()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected InMemoryOptionsExtension(InMemoryOptionsExtension copyFrom)
    {
        this._storeName = copyFrom._storeName;
        this._databaseRoot = copyFrom._databaseRoot;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContextOptionsExtensionInfo Info
        => this._info ??= new ExtensionInfo(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual InMemoryOptionsExtension Clone()
        => new(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string StoreName
        => this._storeName!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNullabilityCheckEnabled
        => this._nullabilityCheckEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryOptionsExtension WithStoreName(string storeName)
    {
        var clone = this.Clone();

        clone._storeName = storeName;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryOptionsExtension WithNullabilityCheckEnabled(bool nullabilityCheckEnabled)
    {
        var clone = this.Clone();

        clone._nullabilityCheckEnabled = nullabilityCheckEnabled;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryDatabaseRoot? DatabaseRoot
        => this._databaseRoot;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryOptionsExtension WithDatabaseRoot(InMemoryDatabaseRoot databaseRoot)
    {
        var clone = this.Clone();

        clone._databaseRoot = databaseRoot;

        return clone;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkInMemoryDatabase();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        private string? _logFragment;

        private new InMemoryOptionsExtension Extension
            => (InMemoryOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override string LogFragment
        {
            get
            {
                if (this._logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("StoreName=").Append(this.Extension._storeName).Append(' ');

                    if (!this.Extension._nullabilityCheckEnabled)
                    {
                        builder.Append("NullabilityChecksEnabled ");
                    }

                    this._logFragment = builder.ToString();
                }

                return this._logFragment;
            }
        }

        public override int GetServiceProviderHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Extension._databaseRoot);
            hashCode.Add(this.Extension._nullabilityCheckEnabled);
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && this.Extension._databaseRoot == otherInfo.Extension._databaseRoot
                && this.Extension._nullabilityCheckEnabled == otherInfo.Extension._nullabilityCheckEnabled;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["InMemoryDatabase:DatabaseRoot"]
                = (this.Extension._databaseRoot?.GetHashCode() ?? 0).ToString(CultureInfo.InvariantCulture);
            debugInfo["InMemoryDatabase:NullabilityChecksEnabled"]
                = (!this.Extension._nullabilityCheckEnabled).GetHashCode().ToString(CultureInfo.InvariantCulture);
        }
    }
}
