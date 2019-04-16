// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    public class InternalServicePropertyBuilder : InternalMetadataItemBuilder<ServiceProperty>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalServicePropertyBuilder([NotNull] ServiceProperty property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName)
            {
                Metadata.SetField(fieldName, configurationSource);
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                return false;
            }

            if (fieldName != null)
            {
                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit);
                Metadata.SetFieldInfo(fieldInfo, configurationSource);
                return true;
            }

            Metadata.SetField(fieldName, configurationSource);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasFieldInfo([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if ((configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                 && (fieldInfo == null
                     || PropertyBase.IsCompatible(
                         fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                         shouldThrow: configurationSource == ConfigurationSource.Explicit)))
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetFieldInfo(fieldInfo, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool SetParameterBinding(
            [NotNull] ServiceParameterBinding parameterBinding, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetParameterBindingConfigurationSource())
                || (Metadata.ParameterBinding == parameterBinding))
            {
                Metadata.SetParameterBinding(parameterBinding, configurationSource);
                return true;
            }

            return false;
        }
    }
}
