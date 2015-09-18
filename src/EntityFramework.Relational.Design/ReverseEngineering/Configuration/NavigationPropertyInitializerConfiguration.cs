// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class NavigationPropertyInitializerConfiguration
    {
        public NavigationPropertyInitializerConfiguration([NotNull] string navPropName, [NotNull] string principalEntityTypeName)
        {
            Check.NotEmpty(navPropName, nameof(navPropName));
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));

            NavigationPropertyName = navPropName;
            PrincipalEntityTypeName = principalEntityTypeName;
        }

        public virtual string NavigationPropertyName { get; }
        public virtual string PrincipalEntityTypeName { get; }
    }
}
