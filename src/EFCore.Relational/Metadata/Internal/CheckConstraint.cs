// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        private string? _name;
        private InternalCheckConstraintBuilder? _builder;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _nameConfigurationSource;

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
            ModelName = name;
            Sql = sql;
            _configurationSource = configurationSource;

            var constraints = GetConstraintsDictionary(EntityType);
            if (constraints == null)
            {
                constraints = new SortedDictionary<string, ICheckConstraint>();
                ((IMutableEntityType)EntityType).SetOrRemoveAnnotation(RelationalAnnotationNames.CheckConstraints, constraints);
            }

            if (constraints.ContainsKey(name))
            {
                throw new InvalidOperationException(RelationalStrings.DuplicateCheckConstraint(
                    name, EntityType.DisplayName(), EntityType.DisplayName()));
            }

            var baseCheckConstraint = entityType.BaseType?.FindCheckConstraint(name);
            if (baseCheckConstraint != null)
            {
                throw new InvalidOperationException(RelationalStrings.DuplicateCheckConstraint(
                    name, EntityType.DisplayName(), baseCheckConstraint.EntityType.DisplayName()));
            }

            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                var derivedCheckConstraint = FindCheckConstraint(derivedType, name);
                if (derivedCheckConstraint != null)
                {
                    throw new InvalidOperationException(RelationalStrings.DuplicateCheckConstraint(
                        name, EntityType.DisplayName(), derivedCheckConstraint.EntityType.DisplayName()));
                }
            }

            EnsureMutable();

            constraints.Add(name, this);

            _builder = new InternalCheckConstraintBuilder(this, ((IConventionModel)entityType.Model).Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IReadOnlyCheckConstraint> GetDeclaredCheckConstraints(IReadOnlyEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            if (entityType is RuntimeEntityType)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            return GetConstraintsDictionary(entityType)?.Values ?? Enumerable.Empty<ICheckConstraint>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IReadOnlyCheckConstraint> GetCheckConstraints(IReadOnlyEntityType entityType)
            => entityType.BaseType != null
                ? GetCheckConstraints(entityType.BaseType).Concat(GetDeclaredCheckConstraints(entityType))
                : GetDeclaredCheckConstraints(entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyCheckConstraint? FindCheckConstraint(
            IReadOnlyEntityType entityType,
            string name)
            => entityType.BaseType?.FindCheckConstraint(name)
                ?? FindDeclaredCheckConstraint(entityType, name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyCheckConstraint? FindDeclaredCheckConstraint(IReadOnlyEntityType entityType, string name)
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
        public static IMutableCheckConstraint? RemoveCheckConstraint(
            IMutableEntityType entityType,
            string name)
        {
            var dataDictionary = GetConstraintsDictionary(entityType);

            if (dataDictionary != null
                && dataDictionary.TryGetValue(name, out var constraint))
            {
                var checkConstraint = (CheckConstraint)constraint;
                checkConstraint.EnsureMutable();
                checkConstraint.SetRemovedFromModel();

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
        public static void Attach(IConventionCheckConstraint detachedCheckConstraint, IConventionCheckConstraint existingCheckConstraint)
        {
            var nameConfigurationSource = detachedCheckConstraint.GetNameConfigurationSource();
            if (nameConfigurationSource != null)
            {
                ((InternalCheckConstraintBuilder)existingCheckConstraint.Builder).HasName(
                    detachedCheckConstraint.Name, nameConfigurationSource.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            IReadOnlyCheckConstraint checkConstraint,
            IReadOnlyCheckConstraint duplicateCheckConstraint,
            in StoreObjectIdentifier storeObject,
            bool shouldThrow)
        {
            if (checkConstraint.Sql != duplicateCheckConstraint.Sql)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateCheckConstraintSqlMismatch(
                            checkConstraint.ModelName,
                            checkConstraint.EntityType.DisplayName(),
                            duplicateCheckConstraint.ModelName,
                            duplicateCheckConstraint.EntityType.DisplayName(),
                            checkConstraint.GetName(storeObject)));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalCheckConstraintBuilder Builder
        {
            [DebuggerStepThrough]
            get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsInModel
            => _builder is not null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetRemovedFromModel()
            => _builder = null;

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
        public virtual string ModelName { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name
        {
            get => _name ?? ((IReadOnlyCheckConstraint)this).GetDefaultName() ?? ModelName;
            set => SetName(value, ConfigurationSource.Explicit);
        }

        /// <inheritdoc/>
        public virtual string? GetName(in StoreObjectIdentifier storeObject)
            => _name ?? ((IReadOnlyCheckConstraint)this).GetDefaultName(storeObject) ?? ModelName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? SetName(string? name, ConfigurationSource configurationSource)
        {
            Check.NullButNotEmpty(name, nameof(name));

            EnsureMutable();

            _name = name;

            _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

            return name;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetNameConfigurationSource()
            => _nameConfigurationSource;

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionCheckConstraintBuilder IConventionCheckConstraint.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        string? IConventionCheckConstraint.SetName(string? name, bool fromDataAnnotation)
            => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
