// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

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
            IMutableEntityType entityType,
            string name,
            string sql,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(sql, nameof(sql));

            EntityType = entityType;
            Name = name;
            Sql = sql;
            _configurationSource = configurationSource;

            var constraints = GetConstraintsDictionary(EntityType);
            if (constraints == null)
            {
                constraints = new SortedDictionary<string, ICheckConstraint>();
                ((IMutableEntityType)EntityType).SetOrRemoveAnnotation(RelationalAnnotationNames.CheckConstraints, constraints);
            }

            if (constraints.ContainsKey(Name))
            {
                throw new InvalidOperationException(RelationalStrings.DuplicateCheckConstraint(Name, EntityType.DisplayName()));
            }

            EnsureMutable();

            constraints.Add(name, this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<ICheckConstraint> GetCheckConstraints(IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return GetConstraintsDictionary(entityType)?.Values ?? Enumerable.Empty<ICheckConstraint>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ICheckConstraint? FindCheckConstraint(
            IReadOnlyEntityType entityType,
            string name)
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
            IMutableEntityType entityType,
            string name)
        {
            var dataDictionary = GetConstraintsDictionary(entityType);

            if (dataDictionary != null
                && dataDictionary.TryGetValue(name, out var constraint))
            {
                var checkConstraint = (CheckConstraint)constraint;
                checkConstraint.EnsureMutable();

                dataDictionary.Remove(name);
                return checkConstraint;
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

        private static SortedDictionary<string, ICheckConstraint>? GetConstraintsDictionary(IReadOnlyEntityType entityType)
            => (SortedDictionary<string, ICheckConstraint>?)entityType[RelationalAnnotationNames.CheckConstraints];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((ICheckConstraint)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

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
