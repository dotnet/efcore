// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        public ConventionContext(ConventionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TMetadata Result => _result;

        /// <summary>
        ///     Calling this will prevent further processing of the associated event by other conventions.
        /// </summary>
        public virtual void StopProcessing()
        {
            _stopProcessing = true;
            _result = default;
        }

        /// <summary>
        ///     <para>
        ///         Calling this will prevent further processing of the associated event by other conventions.
        ///     </para>
        ///     <para>
        ///         The common use case is when the metadata object was removed or replaced by the convention.
        ///     </para>
        /// </summary>
        /// <param name="result"> The new metadata object or <c>null</c>. </param>
        public virtual void StopProcessing(TMetadata result)
        {
            _stopProcessing = true;
            _result = result;
        }

        /// <summary>
        ///     <para>
        ///         Calling this will prevent further processing of the associated event by other conventions
        ///         if the given objects are different.
        ///     </para>
        ///     <para>
        ///         The common use case is when the metadata object was replaced by the convention.
        ///     </para>
        /// </summary>
        /// <param name="result"> The new metadata object. </param>
        public virtual void StopProcessingIfChanged(TMetadata result)
        {
            if (!Equals(Result, result))
            {
                StopProcessing(result);
            }
        }

        /// <summary>
        ///     <para>
        ///         Prevents conventions from being executed immediately when a metadata aspect is modified. All the delayed conventions
        ///         will be executed after the returned object is disposed.
        ///     </para>
        ///     <para>
        ///         This is useful when performing multiple operations that depend on each other.
        ///     </para>
        /// </summary>
        /// <returns> An object that should be disposed to execute the delayed conventions. </returns>
        public virtual IConventionBatch DelayConventions() => _dispatcher.DelayConventions();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldStopProcessing() => _stopProcessing;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ResetState(TMetadata input)
        {
            _stopProcessing = false;
            _result = input;
        }
    }
}
