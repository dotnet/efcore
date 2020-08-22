// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SnapshotModelProcessor : ISnapshotModelProcessor
    {
        private readonly IOperationReporter _operationReporter;
        private readonly HashSet<string> _relationalNames;
        private readonly IConventionSetBuilder _conventionSetBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SnapshotModelProcessor(
            [NotNull] IOperationReporter operationReporter,
            [NotNull] IConventionSetBuilder conventionSetBuilder)
        {
            _operationReporter = operationReporter;
            _relationalNames = new HashSet<string>(
                typeof(RelationalAnnotationNames)
                    .GetRuntimeFields()
                    .Where(p => p.Name != nameof(RelationalAnnotationNames.Prefix))
                    .Select(p => ((string)p.GetValue(null)).Substring(RelationalAnnotationNames.Prefix.Length - 1)));
            _conventionSetBuilder = conventionSetBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel Process(IModel model)
        {
            if (model == null)
            {
                return null;
            }

            var version = model.GetProductVersion();
            if (version != null)
            {
                ProcessElement(model, version);
                UpdateSequences(model, version);

                foreach (var entityType in model.GetEntityTypes())
                {
                    ProcessElement(entityType, version);
                    ProcessCollection(entityType.GetProperties(), version);
                    ProcessCollection(entityType.GetKeys(), version);
                    ProcessCollection(entityType.GetIndexes(), version);

                    foreach (var element in entityType.GetForeignKeys())
                    {
                        ProcessElement(element, version);
                        ProcessElement(element.DependentToPrincipal, version);
                        ProcessElement(element.PrincipalToDependent, version);
                    }
                }
            }

            if (model is IConventionModel conventionModel)
            {
                var conventionSet = _conventionSetBuilder.CreateConventionSet();

                var typeMappingConvention = conventionSet.ModelFinalizingConventions.OfType<TypeMappingConvention>().FirstOrDefault();
                if (typeMappingConvention != null)
                {
                    typeMappingConvention.ProcessModelFinalizing(conventionModel.Builder, null);
                }

                var relationalModelConvention =
                    conventionSet.ModelFinalizedConventions.OfType<RelationalModelConvention>().FirstOrDefault();
                if (relationalModelConvention != null)
                {
                    model = relationalModelConvention.ProcessModelFinalized(conventionModel);
                }
            }

            return model is IMutableModel mutableModel
                ? mutableModel.FinalizeModel()
                : model;
        }

        private void ProcessCollection(IEnumerable<IAnnotatable> metadata, string version)
        {
            foreach (var element in metadata)
            {
                ProcessElement(element, version);
            }
        }

        private void ProcessElement(IEntityType entityType, string version)
        {
            ProcessElement((IAnnotatable)entityType, version);

            if ((version.StartsWith("2.0", StringComparison.Ordinal)
                    || version.StartsWith("2.1", StringComparison.Ordinal))
                && entityType is IMutableEntityType mutableEntityType
                && !entityType.IsOwned())
            {
                UpdateOwnedTypes(mutableEntityType);
            }
        }

        private void ProcessElement(IAnnotatable metadata, string version)
        {
            if (version.StartsWith("1.", StringComparison.Ordinal)
                && metadata is IMutableAnnotatable mutableMetadata)
            {
                foreach (var annotation in mutableMetadata.GetAnnotations().ToList())
                {
                    var colon = annotation.Name.IndexOf(':');
                    if (colon > 0)
                    {
                        var stripped = annotation.Name.Substring(colon);
                        if (_relationalNames.Contains(stripped))
                        {
                            mutableMetadata.RemoveAnnotation(annotation.Name);
                            var relationalName = "Relational" + stripped;
                            var duplicate = mutableMetadata.FindAnnotation(relationalName);

                            if (duplicate == null)
                            {
                                mutableMetadata[relationalName] = annotation.Value;
                            }
                            else if (!Equals(duplicate.Value, annotation.Value))
                            {
                                _operationReporter.WriteWarning(
                                    DesignStrings.MultipleAnnotationConflict(stripped.Substring(1)));
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSequences(IModel model, string version)
        {
            if ((!version.StartsWith("1.", StringComparison.Ordinal)
                    && !version.StartsWith("2.", StringComparison.Ordinal)
                    && !version.StartsWith("3.", StringComparison.Ordinal))
                || !(model is IMutableModel mutableModel))
            {
                return;
            }

            var sequences = model.GetAnnotations()
#pragma warning disable CS0618 // Type or member is obsolete
                .Where(a => a.Name.StartsWith(RelationalAnnotationNames.SequencePrefix, StringComparison.Ordinal))
                .Select(a => new Sequence(model, a.Name));
#pragma warning restore CS0618 // Type or member is obsolete

            var sequencesDictionary = new SortedDictionary<(string, string), Sequence>();
            foreach (var sequence in sequences)
            {
                sequencesDictionary[(sequence.Name, sequence.Schema)] = sequence;
            }

            if (sequencesDictionary.Count > 0)
            {
                mutableModel[RelationalAnnotationNames.Sequences] = sequencesDictionary;
            }
        }

        private void UpdateOwnedTypes(IMutableEntityType entityType)
        {
            var ownerships = entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk.IsOwnership && fk.IsUnique)
                .ToList();
            foreach (var ownership in ownerships)
            {
                var ownedType = ownership.DeclaringEntityType;

                var oldPrincipalKey = ownership.PrincipalKey;
                if (!oldPrincipalKey.IsPrimaryKey())
                {
                    ownership.SetProperties(
                        (IReadOnlyList<Property>)ownership.Properties,
                        ownership.PrincipalEntityType.FindPrimaryKey());

                    if (oldPrincipalKey is IConventionKey conventionKey
                        && conventionKey.GetConfigurationSource() == ConfigurationSource.Convention)
                    {
                        oldPrincipalKey.DeclaringEntityType.RemoveKey(oldPrincipalKey);
                    }

                    foreach (var oldProperty in oldPrincipalKey.Properties)
                    {
                        if (oldProperty is IConventionProperty conventionProperty
                            && conventionProperty.GetConfigurationSource() == ConfigurationSource.Convention)
                        {
                            oldProperty.DeclaringEntityType.RemoveProperty(oldProperty);
                        }
                    }
                }

                if (ownedType.FindPrimaryKey() == null)
                {
                    foreach (var mutableProperty in ownership.Properties)
                    {
                        if (mutableProperty.IsNullable)
                        {
                            mutableProperty.IsNullable = false;
                        }
                    }

                    ownedType.SetPrimaryKey(ownership.Properties);
                }

                UpdateOwnedTypes(ownedType);
            }
        }
    }
}
