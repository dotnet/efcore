// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal.Configuration
{
    public class PropertyConfiguration
    {
        public PropertyConfiguration(
            [NotNull] EntityConfiguration entityConfiguration, [NotNull] IProperty property)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(property, nameof(property));

            EntityConfiguration = entityConfiguration;
            Property = property;
        }

        public virtual EntityConfiguration EntityConfiguration { get; [param: NotNull] private set; }
        public virtual IProperty Property { get; [param: NotNull] private set; }
        public virtual List<IAttributeConfiguration> AttributeConfigurations { get; } = new List<IAttributeConfiguration>();
        public virtual List<FluentApiConfiguration> FluentApiConfigurations { get; } = new List<FluentApiConfiguration>();

        public virtual List<FluentApiConfiguration> GetFluentApiConfigurations(bool useFluentApiOnly)
        {
            return useFluentApiOnly
                ? FluentApiConfigurations
                : FluentApiConfigurations.Where(fc => !fc.HasAttributeEquivalent).ToList();
        }
    }
}
