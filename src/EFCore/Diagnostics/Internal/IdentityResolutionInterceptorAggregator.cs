// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class IdentityResolutionInterceptorAggregator : InterceptorAggregator<IIdentityResolutionInterceptor>
{
    /// <inheritdoc />
    protected override IIdentityResolutionInterceptor CreateChain(IEnumerable<IIdentityResolutionInterceptor> interceptors)
        => new CompositeIdentityResolutionInterceptor(interceptors);

    private sealed class CompositeIdentityResolutionInterceptor : IIdentityResolutionInterceptor
    {
        private readonly IIdentityResolutionInterceptor[] _interceptors;

        public CompositeIdentityResolutionInterceptor(IEnumerable<IIdentityResolutionInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public void UpdateTrackedInstance(IdentityResolutionInterceptionData interceptionData, EntityEntry existingEntry, object newEntity)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].UpdateTrackedInstance(interceptionData, existingEntry, newEntity);
            }
        }
    }
}
