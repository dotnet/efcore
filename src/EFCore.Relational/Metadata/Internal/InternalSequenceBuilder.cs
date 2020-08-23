// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalSequenceBuilder : AnnotatableBuilder<Sequence, IConventionModelBuilder>, IConventionSequenceBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalSequenceBuilder([NotNull] Sequence sequence, [NotNull] IConventionModelBuilder modelBuilder)
            : base(sequence, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder HasType([CanBeNull] Type type, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetTypeConfigurationSource())
                || Metadata.Type == type)
            {
                Metadata.SetType(type, configurationSource);
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
        public virtual bool CanSetType([CanBeNull] Type type, ConfigurationSource configurationSource)
            => (type == null || Sequence.SupportedTypes.Contains(type))
                && (configurationSource.Overrides(Metadata.GetTypeConfigurationSource())
                    || Metadata.Type == type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder IncrementsBy(
            int? increment,
            ConfigurationSource configurationSource)
        {
            if (CanSetIncrementsBy(increment, configurationSource))
            {
                Metadata.SetIncrementBy(increment, configurationSource);
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
        public virtual bool CanSetIncrementsBy(int? increment, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetIncrementByConfigurationSource())
                || Metadata.IncrementBy == increment;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder StartsAt(long? startValue, ConfigurationSource configurationSource)
        {
            if (CanSetStartsAt(startValue, configurationSource))
            {
                Metadata.SetStartValue(startValue, configurationSource);
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
        public virtual bool CanSetStartsAt(long? startValue, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetStartValueConfigurationSource())
                || Metadata.StartValue == startValue;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder HasMax(long? maximum, ConfigurationSource configurationSource)
        {
            if (CanSetMax(maximum, configurationSource))
            {
                Metadata.SetMaxValue(maximum, configurationSource);
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
        public virtual bool CanSetMax(long? maximum, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetMaxValueConfigurationSource())
                || Metadata.MaxValue == maximum;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder HasMin(long? minimum, ConfigurationSource configurationSource)
        {
            if (CanSetMin(minimum, configurationSource))
            {
                Metadata.SetMinValue(minimum, configurationSource);
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
        public virtual bool CanSetMin(long? minimum, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetMinValueConfigurationSource())
                || Metadata.MinValue == minimum;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionSequenceBuilder IsCyclic(bool? cyclic, ConfigurationSource configurationSource)
        {
            if (CanSetIsCyclic(cyclic, configurationSource))
            {
                Metadata.SetIsCyclic(cyclic, configurationSource);
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
        public virtual bool CanSetIsCyclic(bool? cyclic, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetIsCyclicConfigurationSource())
                || Metadata.IsCyclic == cyclic;

        /// <inheritdoc />
        IConventionSequence IConventionSequenceBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.HasType(Type type, bool fromDataAnnotation)
            => HasType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetType(Type type, bool fromDataAnnotation)
            => CanSetType(type, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.IncrementsBy(int? increment, bool fromDataAnnotation)
            => IncrementsBy(increment, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetIncrementsBy(int? increment, bool fromDataAnnotation)
            => CanSetIncrementsBy(increment, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.StartsAt(long? startValue, bool fromDataAnnotation)
            => StartsAt(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetStartsAt(long? startValue, bool fromDataAnnotation)
            => CanSetStartsAt(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.HasMax(long? maximum, bool fromDataAnnotation)
            => HasMax(maximum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetMax(long? maximum, bool fromDataAnnotation)
            => CanSetMax(maximum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.HasMin(long? minimum, bool fromDataAnnotation)
            => HasMin(minimum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetMin(long? minimum, bool fromDataAnnotation)
            => CanSetMin(minimum, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSequenceBuilder IConventionSequenceBuilder.IsCyclic(bool? cyclic, bool fromDataAnnotation)
            => IsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionSequenceBuilder.CanSetIsCyclic(bool? cyclic, bool fromDataAnnotation)
            => CanSetIsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
