// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

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
    private readonly IModelRuntimeInitializer _modelRuntimeInitializer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SnapshotModelProcessor(
        IOperationReporter operationReporter,
        IModelRuntimeInitializer modelRuntimeInitializer)
    {
        _operationReporter = operationReporter;
        _relationalNames =
        [
            ..typeof(RelationalAnnotationNames)
                .GetRuntimeFields()
                .Where(
                    p => p.Name != nameof(RelationalAnnotationNames.Prefix)
                        && p.Name != nameof(RelationalAnnotationNames.AllNames))
                .Select(p => (string)p.GetValue(null)!)
                .Where(v => v.IndexOf(':') > 0)
                .Select(v => v[(RelationalAnnotationNames.Prefix.Length - 1)..])
        ];
        _modelRuntimeInitializer = modelRuntimeInitializer;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel? Process(IReadOnlyModel? model, bool resetVersion = false)
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

        if (model is IMutableModel mutableModel)
        {
            mutableModel.RemoveAnnotation("ChangeDetector.SkipDetectChanges");
            if (resetVersion)
            {
                mutableModel.SetProductVersion(ProductInfo.GetVersion());
            }
        }

        return _modelRuntimeInitializer.Initialize((IModel)model, designTime: true, validationLogger: null);
    }

    private void ProcessCollection(IEnumerable<IReadOnlyAnnotatable> metadata, string version)
    {
        foreach (var element in metadata)
        {
            ProcessElement(element, version);
        }
    }

    private void ProcessElement(IReadOnlyEntityType entityType, string version)
    {
        ProcessElement((IReadOnlyAnnotatable)entityType, version);

        if ((version.StartsWith("2.0", StringComparison.Ordinal)
                || version.StartsWith("2.1", StringComparison.Ordinal))
            && entityType is IMutableEntityType mutableEntityType
            && !entityType.IsOwned())
        {
            UpdateOwnedTypes(mutableEntityType);
        }
    }

    private void ProcessElement(IReadOnlyAnnotatable? metadata, string version)
    {
        if (version.StartsWith("1.", StringComparison.Ordinal)
            && metadata is IMutableAnnotatable mutableMetadata)
        {
            foreach (var annotation in mutableMetadata.GetAnnotations().ToList())
            {
                var colon = annotation.Name.IndexOf(':');
                if (colon > 0)
                {
                    var stripped = annotation.Name[colon..];
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
                                DesignStrings.MultipleAnnotationConflict(stripped[1..]));
                        }
                    }
                }
            }
        }
    }

    private static void UpdateSequences(IReadOnlyModel model, string version)
    {
        if ((!version.StartsWith("1.", StringComparison.Ordinal)
                && !version.StartsWith("2.", StringComparison.Ordinal)
                && !version.StartsWith("3.", StringComparison.Ordinal))
            || model is not IMutableModel mutableModel)
        {
            return;
        }

        var sequences = model.GetAnnotations()
#pragma warning disable CS0618 // Type or member is obsolete
            .Where(a => a.Name.StartsWith(RelationalAnnotationNames.SequencePrefix, StringComparison.Ordinal))
            .ToList();
#pragma warning restore CS0618 // Type or member is obsolete

        var sequencesDictionary = new Dictionary<(string, string?), ISequence>();
        foreach (var sequenceAnnotation in sequences)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var sequence = new Sequence(model, sequenceAnnotation.Name);
#pragma warning restore CS0618 // Type or member is obsolete
            sequencesDictionary[(sequence.Name, sequence.ModelSchema)] = sequence;
            mutableModel.RemoveAnnotation(sequenceAnnotation.Name);
        }

        if (sequencesDictionary.Count > 0)
        {
            mutableModel[RelationalAnnotationNames.Sequences] = sequencesDictionary;
        }
    }

    private static void UpdateOwnedTypes(IMutableEntityType entityType)
    {
        var ownerships = entityType.GetDeclaredReferencingForeignKeys().Where(fk => fk is { IsOwnership: true, IsUnique: true })
            .ToList();
        foreach (var ownership in ownerships)
        {
            var ownedType = ownership.DeclaringEntityType;

            var oldPrincipalKey = ownership.PrincipalKey;
            if (!oldPrincipalKey.IsPrimaryKey())
            {
                ownership.SetProperties(
                    ownership.Properties,
                    ownership.PrincipalEntityType.FindPrimaryKey()!);

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
                        oldProperty.DeclaringType.RemoveProperty(oldProperty);
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
