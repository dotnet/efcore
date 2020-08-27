// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public sealed class EndpointRouteConfiguration : IEndpointRouteBuilder
    {
        private readonly IEndpointRouteBuilder _routeBuilder;
        private ApplicationPartManager _scopedPartManager;

        public EndpointRouteConfiguration(IEndpointRouteBuilder routeBuilder)
        {
            _routeBuilder = routeBuilder ?? throw new ArgumentNullException(nameof(routeBuilder));
        }

        /// <summary>
        /// Add a list of controllers to be discovered by the application.
        /// </summary>
        /// <param name="controllers"></param>
        public void AddControllers(params Type[] controllers)
        {
            // Strip out all the IApplicationPartTypeProvider parts.
            _scopedPartManager = _routeBuilder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            var parts = _scopedPartManager.ApplicationParts;
            var nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            _scopedPartManager.ApplicationParts.Clear();
            _scopedPartManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            var part = new AssemblyPart(new TestAssembly(controllers));
            _scopedPartManager.ApplicationParts.Add(part);
        }

        public ICollection<EndpointDataSource> DataSources => _routeBuilder.DataSources;

        public IServiceProvider ServiceProvider => _routeBuilder.ServiceProvider;

        public IApplicationBuilder CreateApplicationBuilder() => _routeBuilder.CreateApplicationBuilder();
    }
}
