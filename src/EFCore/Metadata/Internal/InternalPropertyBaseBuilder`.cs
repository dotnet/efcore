// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalPropertyBaseBuilder<TPropertyBase> : AnnotatableBuilder<TPropertyBase, InternalModelBuilder>
        where TPropertyBase : PropertyBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalPropertyBaseBuilder([NotNull] TPropertyBase metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBaseBuilder<TPropertyBase> HasField(
            [CanBeNull] string fieldName,
            ConfigurationSource configurationSource)
        {
            if (CanSetField(fieldName, configurationSource))
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
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                if (fieldName == null)
                {
                    return true;
                }

                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit);

                return fieldInfo != null
                    && PropertyBase.IsCompatible(
                        fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                        shouldThrow: configurationSource == ConfigurationSource.Explicit);
            }

            return Metadata.FieldInfo?.GetSimpleMemberName() == fieldName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBaseBuilder<TPropertyBase> HasField(
            [CanBeNull] FieldInfo fieldInfo,
            ConfigurationSource configurationSource)
        {
            if (CanSetField(fieldInfo, configurationSource))
            {
                Metadata.SetFieldInfo(fieldInfo, configurationSource);

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
                            shouldThrow: configurationSource == ConfigurationSource.Explicit)))
                || Equals(Metadata.FieldInfo, fieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBaseBuilder<TPropertyBase> UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
        {
            if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
            {
                Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

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
        public virtual bool CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
                || ((IPropertyBase)Metadata).GetPropertyAccessMode() == propertyAccessMode;
    }
}
