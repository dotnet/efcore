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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SnapshotModelProcessor([NotNull] IOperationReporter operationReporter)
        {
            _operationReporter = operationReporter;
            _relationalNames = new HashSet<string>(
                typeof(RelationalAnnotationNames)
                    .GetTypeInfo()
                    .GetRuntimeFields()
                    .Where(p => p.Name != nameof(RelationalAnnotationNames.Prefix))
                    .Select(p => ((string)p.GetValue(null)).Substring(RelationalAnnotationNames.Prefix.Length - 1)));
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

            return model;
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
                && entityType.FindPrimaryKey() == null)
            {
                var ownership = mutableEntityType.FindOwnership();
                if (ownership is IMutableForeignKey mutableOwnership
                    && ownership.IsUnique)
                {
                    mutableEntityType.SetPrimaryKey(mutableOwnership.Properties);
                }
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
    }
}
