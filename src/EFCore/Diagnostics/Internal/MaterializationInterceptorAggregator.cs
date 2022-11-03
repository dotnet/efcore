// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MaterializationInterceptorAggregator : InterceptorAggregator<IMaterializationInterceptor>
{
    /// <summary>
    ///     Must be implemented by the inheriting type to create a single interceptor from the given list.
    /// </summary>
    /// <param name="interceptors">The interceptors to combine.</param>
    /// <returns>The combined interceptor.</returns>
    protected override IMaterializationInterceptor CreateChain(IEnumerable<IMaterializationInterceptor> interceptors)
        => new CompositeMaterializationInterceptor(interceptors);

    private sealed class CompositeMaterializationInterceptor : IMaterializationInterceptor
    {
        private readonly IMaterializationInterceptor[] _interceptors;

        public CompositeMaterializationInterceptor(IEnumerable<IMaterializationInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public InterceptionResult<object> CreatingInstance(
            MaterializationInterceptionData materializationData,
            InterceptionResult<object> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].CreatingInstance(materializationData, result);
            }

            return result;
        }

        public object CreatedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                entity = _interceptors[i].CreatedInstance(materializationData, entity);
            }

            return entity;
        }

        public InterceptionResult InitializingInstance(
            MaterializationInterceptionData materializationData,
            object entity,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].InitializingInstance(materializationData, entity, result);
            }

            return result;
        }

        public object InitializedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                entity = _interceptors[i].InitializedInstance(materializationData, entity);
            }

            return entity;
        }
    }
}
