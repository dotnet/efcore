// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CheckConstraint : ConventionAnnotatable, IMutableCheckConstraint, IConventionCheckConstraint, ICheckConstraint
    {
        private ConfigurationSource _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CheckConstraint(
            [NotNull] IMutableEntityType entityType,
            [NotNull] string name,
            [NotNull] string sql,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(sql, nameof(sql));

            EntityType = entityType;
            Name = name;
            Sql = sql;
            _configurationSource = configurationSource;

            var dataDictionary = GetConstraintsDictionary(EntityType);
            if (dataDictionary == null)
            {
                dataDictionary = new Dictionary<string, CheckConstraint>();
                ((IMutableEntityType)EntityType).SetOrRemoveAnnotation(RelationalAnnotationNames.CheckConstraints, dataDictionary);
            }

            if (dataDictionary.ContainsKey(Name))
            {
                throw new InvalidOperationException(RelationalStrings.DuplicateCheckConstraint(Name, EntityType.DisplayName()));
            }

            EnsureMutable();

            dataDictionary.Add(name, this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<CheckConstraint> GetCheckConstraints([NotNull] IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return GetConstraintsDictionary(entityType)?.Values ?? Enumerable.Empty<CheckConstraint>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ICheckConstraint? FindCheckConstraint(
            [NotNull] IReadOnlyEntityType entityType,
            [NotNull] string name)
        {
            var dataDictionary = GetConstraintsDictionary(entityType);

            return dataDictionary == null
                ? null
                : dataDictionary.TryGetValue(name, out var checkConstraint)
                    ? checkConstraint
                    : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static CheckConstraint? RemoveCheckConstraint(
            [NotNull] IMutableEntityType entityType,
            [NotNull] string name)
        {
            var dataDictionary = GetConstraintsDictionary(entityType);

            if (dataDictionary != null
                && dataDictionary.TryGetValue(name, out var constraint))
            {
                constraint.EnsureMutable();

                dataDictionary.Remove(name);
                return constraint;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyEntityType EntityType { get; }

        /// <summary>
        ///     Indicates whether the check constraint is read-only.
        /// </summary>
        public override bool IsReadOnly => ((Annotatable)EntityType.Model).IsReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Sql { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource()
            => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource.Max(_configurationSource);
        }

        private static Dictionary<string, CheckConstraint>? GetConstraintsDictionary(IReadOnlyEntityType entityType)
            => (Dictionary<string, CheckConstraint>?)entityType[RelationalAnnotationNames.CheckConstraints];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionCheckConstraint.EntityType
        {
            [DebuggerStepThrough]
            get => (IConventionEntityType)EntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableCheckConstraint.EntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)EntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType ICheckConstraint.EntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)EntityType;
        }
    }
}
