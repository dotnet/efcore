// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ConventionalAnnotation : Annotation
    {
        private ConfigurationSource _configurationSource;

        public ConventionalAnnotation([NotNull] string name, [NotNull] object value, ConfigurationSource configurationSource)
            : base(name, value)
        {
            _configurationSource = configurationSource;
        }

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual ConfigurationSource UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);
    }
}
