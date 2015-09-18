// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class NavigationPropertyConfiguration
    {
        public NavigationPropertyConfiguration([NotNull] string errorAnnotation)
        {
            Check.NotEmpty(errorAnnotation, nameof(errorAnnotation));

            ErrorAnnotation = errorAnnotation;
        }

        public NavigationPropertyConfiguration([NotNull] string type, [NotNull] string name)
        {
            Check.NotNull(type, nameof(type));
            Check.NotEmpty(name, nameof(name));

            Type = type;
            Name = name;
        }

        public virtual string ErrorAnnotation { get; [param: NotNull] private set; }
        public virtual string Type { get; }
        public virtual string Name { get; }
        public virtual List<IAttributeConfiguration> AttributeConfigurations { get; } = new List<IAttributeConfiguration>();
    }
}
