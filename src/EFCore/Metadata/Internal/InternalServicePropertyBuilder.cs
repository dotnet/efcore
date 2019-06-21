// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalServicePropertyBuilder : InternalModelItemBuilder<ServiceProperty>, IConventionServicePropertyBuilder
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
        public virtual InternalServicePropertyBuilder HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName
                || configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                Metadata.SetField(fieldName, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetField([CanBeNull] string fieldName, ConfigurationSource? configurationSource)
        {
            if (fieldName != null
                && configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: false);
                return fieldInfo != null
                       && PropertyBase.IsCompatible(
                           fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                           shouldThrow: false);
            }

            return Metadata.FieldInfo?.GetSimpleMemberName() == fieldName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServicePropertyBuilder HasField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetField(fieldInfo, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                && (fieldInfo == null
                    || PropertyBase.IsCompatible(
                        fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                        shouldThrow: false)))
               || Equals(Metadata.FieldInfo, fieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServicePropertyBuilder UsePropertyAccessMode(
            PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => (InternalServicePropertyBuilder)HasAnnotation(
                CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServicePropertyBuilder HasParameterBinding(
            ServiceParameterBinding parameterBinding, ConfigurationSource configurationSource)
        {
            if (CanSetParameterBinding(parameterBinding, configurationSource))
            {
                Metadata.SetParameterBinding(parameterBinding, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetParameterBinding(ServiceParameterBinding parameterBinding, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetParameterBindingConfigurationSource())
               || (Metadata.ParameterBinding == parameterBinding);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServiceProperty IConventionServicePropertyBuilder.Metadata => Metadata;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServicePropertyBuilder IConventionServicePropertyBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServicePropertyBuilder IConventionServicePropertyBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionServicePropertyBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionServicePropertyBuilder.CanSetField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServicePropertyBuilder IConventionServicePropertyBuilder.HasParameterBinding(
            ServiceParameterBinding parameterBinding, bool fromDataAnnotation)
            => HasParameterBinding(
                parameterBinding, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionServicePropertyBuilder.CanSetParameterBinding(ServiceParameterBinding parameterBinding, bool fromDataAnnotation)
            => CanSetParameterBinding(
                parameterBinding, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
