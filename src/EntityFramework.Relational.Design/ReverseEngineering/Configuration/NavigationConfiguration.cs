// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class NavigationConfiguration
    {
        public NavigationConfiguration([NotNull] EntityConfiguration entityConfiguration,
            [NotNull] IForeignKey foreignKey, [NotNull] string dependentEndNavigationPropertyName,
            [NotNull] string principalEndNavigationPropertyName)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotEmpty(dependentEndNavigationPropertyName, nameof(dependentEndNavigationPropertyName));
            Check.NotEmpty(principalEndNavigationPropertyName, nameof(principalEndNavigationPropertyName));

            EntityConfiguration = entityConfiguration;
            ForeignKey = foreignKey;
            DependentEndNavigationPropertyName = dependentEndNavigationPropertyName;
            PrincipalEndNavigationPropertyName = principalEndNavigationPropertyName;
        }

        public virtual EntityConfiguration EntityConfiguration { get; [param: NotNull] private set; }
        public virtual IForeignKey ForeignKey { get; [param: NotNull] private set; }
        public virtual string DependentEndNavigationPropertyName { get; [param: NotNull] private set; }
        public virtual string PrincipalEndNavigationPropertyName { get; [param: NotNull] private set; }
    }
}
