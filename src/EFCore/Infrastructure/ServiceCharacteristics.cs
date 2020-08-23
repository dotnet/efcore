// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Characteristics of a given EF service.
    /// </summary>
    public readonly struct ServiceCharacteristics
    {
        /// <summary>
        ///     Creates a new <see cref="ServiceCharacteristics" /> struct.
        /// </summary>
        /// <param name="lifetime"> The service lifetime. </param>
        /// <param name="multipleRegistrations">
        ///     <see langword="true" /> if multiple registrations of the service is allowed; <see langword="false" />
        ///     otherwise.
        /// </param>
        public ServiceCharacteristics(ServiceLifetime lifetime, bool multipleRegistrations = false)
        {
            Lifetime = lifetime;
            MultipleRegistrations = multipleRegistrations;
        }

        /// <summary>
        ///     The service lifetime.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        ///     <see langword="true" /> if multiple registrations of the service is allowed; <see langword="false" /> otherwise.
        /// </summary>
        public bool MultipleRegistrations { get; }
    }
}
