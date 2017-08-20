// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SnapshotModelProcessor : ISnapshotModelProcessor
    {
        private readonly IOperationReporter _operationReporter;
        private readonly HashSet<string> _relationalNames;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Process(IModel model)
        {
            if (model != null
                && model.GetProductVersion()?.StartsWith("1.") == true)
            {
                ProcessElement(model);

                foreach (var entityType in model.GetEntityTypes())
                {
                    ProcessElement(entityType);
                    ProcessCollection(entityType.GetProperties());
                    ProcessCollection(entityType.GetKeys());
                    ProcessCollection(entityType.GetIndexes());

                    foreach (var element in entityType.GetForeignKeys())
                    {
                        ProcessElement(element);
                        ProcessElement(element.DependentToPrincipal);
                        ProcessElement(element.PrincipalToDependent);
                    }
                }
            }

            return model;
        }

        private void ProcessCollection(IEnumerable<IAnnotatable> metadata)
        {
            foreach (var element in metadata)
            {
                ProcessElement(element);
            }
        }

        private void ProcessElement(IAnnotatable metadata)
        {
            if (metadata is IMutableAnnotatable mutableMetadata)
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
