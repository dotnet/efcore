// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ConventionContext<TMetadata> : IConventionContext<TMetadata>, IReadableConventionContext
    {
        private bool _stopProcessing;
        private readonly ConventionDispatcher _dispatcher;
        private TMetadata _result;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ConventionContext([NotNull] ConventionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TMetadata Result
            => _result;

        /// <inheritdoc />
        public virtual void StopProcessing()
        {
            _stopProcessing = true;
            _result = default;
        }

        /// <inheritdoc />
        public virtual void StopProcessing(TMetadata result)
        {
            _stopProcessing = true;
            _result = result;
        }

        /// <inheritdoc />
        public virtual void StopProcessingIfChanged(TMetadata result)
        {
            if (!Equals(Result, result))
            {
                StopProcessing(result);
            }
        }

        /// <inheritdoc />
        public virtual IConventionBatch DelayConventions()
            => _dispatcher.DelayConventions();

        /// <inheritdoc />
        public virtual bool ShouldStopProcessing()
            => _stopProcessing;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ResetState([CanBeNull] TMetadata input)
        {
            _stopProcessing = false;
            _result = input;
        }
    }
}
