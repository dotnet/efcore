// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
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

        public virtual Dictionary<string, List<FluentApiConfiguration>>
            GetFluentApiConfigurations(bool useFluentApiExclusively)
        {
            var fluentApiConfigsDictionary = new Dictionary<string, List<FluentApiConfiguration>>();
            var fluentApiConfigs = useFluentApiExclusively
                ? FluentApiConfigurations
                : FluentApiConfigurations.Where(fc => !fc.HasAttributeEquivalent);
            foreach (var fluentApiConfiguration in fluentApiConfigs)
            {
                var @for = fluentApiConfiguration.For ?? string.Empty;
                List<FluentApiConfiguration> listOfFluentApiMethodBodies;
                if (!fluentApiConfigsDictionary.TryGetValue(@for, out listOfFluentApiMethodBodies))
                {
                    listOfFluentApiMethodBodies = new List<FluentApiConfiguration>();
                    fluentApiConfigsDictionary.Add(@for, listOfFluentApiMethodBodies);
                }
                listOfFluentApiMethodBodies.Add(fluentApiConfiguration);
            }
            return fluentApiConfigsDictionary;
        }
    }
}
