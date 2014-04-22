// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryConfigurationExtension : EntityConfigurationExtension
    {
        public virtual bool Persist { get; internal set; }

        protected override void ApplyServices(EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddInMemoryStore();
        }
    }
}
