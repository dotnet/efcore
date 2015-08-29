// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class ModelBuilderConventionSource : IModelBuilderConventionSource
    {
        private readonly IServiceProvider _serviceProvider;

        public ModelBuilderConventionSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyList<IModelBuilderConvention> GetConventions()
        {
            return (IReadOnlyList<IModelBuilderConvention>) _serviceProvider.GetServices<IModelBuilderConvention>();
        }
    }
}