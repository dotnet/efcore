// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public interface IDesignTimeMetadataProviderFactory
    {
        void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection);
    }
}
