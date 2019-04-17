// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    public class InternalServicePropertyBuilder : InternalMetadataItemBuilder<ServiceProperty>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalServicePropertyBuilder([NotNull] ServiceProperty property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                Metadata.SetField(fieldInfo, configurationSource);
                return true;
            }

            Metadata.SetField(fieldName, configurationSource);
            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                Metadata.SetField(fieldInfo, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
