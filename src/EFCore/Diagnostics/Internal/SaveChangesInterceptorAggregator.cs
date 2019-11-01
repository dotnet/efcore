// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SaveChangesInterceptorAggregator : InterceptorAggregator<ISaveChangesInterceptor>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ISaveChangesInterceptor CreateChain(IEnumerable<ISaveChangesInterceptor> interceptors)
            => new CompositeSaveChangesInterceptor(interceptors);

        private sealed class CompositeSaveChangesInterceptor : ISaveChangesInterceptor
        {
            private readonly ISaveChangesInterceptor[] _interceptors;

            public CompositeSaveChangesInterceptor([NotNull] IEnumerable<ISaveChangesInterceptor> interceptors)
            {
                _interceptors = interceptors.ToArray();
            }

            public InterceptionResult<int> SavingChanges([NotNull] DbContextEventData eventData, InterceptionResult<int> result)
            {
                for(var i =0; i < _interceptors.Count(); i++)
                {
                    result = _interceptors[i].SavingChanges(eventData, result);
                }
                return result;
            }

            public Task<InterceptionResult<int>> SavingChangesAsync([NotNull] DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
            {
                throw new System.NotImplementedException();
            }

            public int SavedChanges([NotNull] SaveChangesCompletedEventData eventData, int result)
            {
                for(var i =0; i < _interceptors.Count(); i++)
                {
                    result = _interceptors[i].SavedChanges(eventData, result);
                }
                return result;
            }

            public Task<int> SavedChangesAsync([NotNull] SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
            {
                throw new System.NotImplementedException();
            }

            public void SavingChangesFailed([NotNull] DbContextErrorEventData eventData)
            {
                for(var i =0; i < _interceptors.Count(); i++)
                {
                    _interceptors[i].SavingChangesFailed(eventData);
                }
            }
        }
    }
}
