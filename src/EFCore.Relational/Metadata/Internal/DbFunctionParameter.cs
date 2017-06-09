// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class DbFunctionParameter : IMutableDbFunctionParameter
    {
        private int _index;
        private Type _parameterType;
        private string _name;
        private ConfigurationSource? _parameterIndexConfigurationSource;
        private ConfigurationSource? _parameterTypeConfigurationSource;
        private ConfigurationSource? _nameConfigurationSource;
        private ConfigurationSource _configurationSource;

        public DbFunctionParameter([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            _name = name;
            _configurationSource = configurationSource;
            _nameConfigurationSource = configurationSource;
        }

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource.Max(_configurationSource);

        public virtual ConfigurationSource? GetConfigurationSource() => _configurationSource;

        public virtual string Name
        {
            get => _name;
            [param: NotNull] set => SetName(value, ConfigurationSource.Explicit);
        }

        public virtual void SetName([NotNull] string name, ConfigurationSource configSource)
        {
            Check.NotNull(name, nameof(name));

            UpdateNameConfigurationSource(configSource);

            _name = name;
        }

        private void UpdateNameConfigurationSource(ConfigurationSource configurationSource)
            => _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        public virtual ConfigurationSource? GetNameConfigurationSource() => _nameConfigurationSource;

        public virtual Type ParameterType 
        {
            get => _parameterType;  
            [param: NotNull] set => SetParameterType(value, ConfigurationSource.Explicit);
        }

        private void UpdateParameterTypeConfigurationSource(ConfigurationSource configurationSource)
            => _parameterTypeConfigurationSource = configurationSource.Max(_parameterTypeConfigurationSource);

        public virtual ConfigurationSource? GetParameterTypeConfigurationSource() => _parameterTypeConfigurationSource;

        public virtual void SetParameterType([NotNull] Type parameterType, ConfigurationSource configSource)
        {
            Check.NotNull(parameterType, nameof(parameterType));

            UpdateParameterTypeConfigurationSource(configSource);

            _parameterType = parameterType;
        }

        public virtual int Index
        {
            get => _index;
            set => SetParameterIndex(value, ConfigurationSource.Explicit);
        }

        private void UpdateParameterIndexConfigurationSource(ConfigurationSource configurationSource)
            => _parameterIndexConfigurationSource = configurationSource.Max(_parameterIndexConfigurationSource);

        public virtual ConfigurationSource? GetParameterIndexConfigurationSource() => _parameterIndexConfigurationSource;
     
        public virtual void SetParameterIndex(int index, ConfigurationSource configSource)
        {
            UpdateParameterIndexConfigurationSource(configSource);

            _index = index;
        }
    }
}
