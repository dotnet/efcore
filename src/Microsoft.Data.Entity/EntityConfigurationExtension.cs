// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.Data.Entity
{
    public abstract class EntityConfigurationExtension
    {
        protected internal abstract void ApplyServices([NotNull] EntityServicesBuilder builder);
    }
}
