// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     The default implementation of <see cref="ISnapshotModelProcessor" />. Applies fix-ups to a model loaded from a
///     Migrations snapshot so it can be used with the current version of EF Core.
/// </summary>
/// <remarks>
///     <para>
///         Database providers can derive from this class and register their derived type as <see cref="ISnapshotModelProcessor" />
///         to apply additional provider-specific fix-ups in addition to the relational fix-ups performed by this implementation.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public class SnapshotModelProcessor : ISnapshotModelProcessor
{
    private readonly IOperationReporter _operationReporter;
    private readonly HashSet<string> _relationalNames;
    private readonly IModelRuntimeInitializer _modelRuntimeInitializer;

    /// <summary>
    ///     Creates a new instance of <see cref="SnapshotModelProcessor" />.
    /// </summary>
    /// <param name="operationReporter">The reporter used to report warnings encountered while processing the model.</param>
    /// <param name="modelRuntimeInitializer">The runtime initializer used to finalize the processed model.</param>
    public SnapshotModelProcessor(
        IOperationReporter operationReporter,
        IModelRuntimeInitializer modelRuntimeInitializer)
    {
        _operationReporter = operationReporter;
        _relationalNames =
        [
            ..typeof(RelationalAnnotationNames)
                .GetRuntimeFields()
                .Where(p => p.Name != nameof(RelationalAnnotationNames.Prefix)
                    && p.Name != nameof(RelationalAnnotationNames.AllNames))
                .Select(p => (string)p.GetValue(null)!)
                .Where(v => v.IndexOf(':') > 0)
                .Select(v => v[(RelationalAnnotationNames.Prefix.Length - 1)..])
        ];
        _modelRuntimeInitializer = modelRuntimeInitializer;
    }

    /// <inheritdoc />
    public virtual IModel? Process(IReadOnlyModel? model, bool resetVersion = false)
    {
        if (model == null
#pragma warning disable EF1001 // Internal EF Core API usage.
            || model is not Model mutableModel
            || mutableModel.IsReadOnly)
#pragma warning restore EF1001 // Internal EF Core API usage.
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

                ProcessComplexProperties(entityType, version);
            }
        }

        mutableModel.RemoveAnnotation("ChangeDetector.SkipDetectChanges");
        if (resetVersion)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            mutableModel.SetProductVersion(ProductInfo.GetVersion());
#pragma warning restore EF1001 // Internal EF Core API usage.
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

    private void ProcessComplexProperties(IReadOnlyTypeBase typeBase, string version)
    {
        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            ProcessElement(complexProperty, version);

            if (complexProperty is IMutableComplexProperty mutableComplexProperty)
            {
                UpdateComplexPropertyNullability(mutableComplexProperty, version);
            }

            ProcessComplexProperties(complexProperty.ComplexType, version);
        }
    }

    private static void UpdateComplexPropertyNullability(IMutableComplexProperty complexProperty, string version)
    {
        if (version.StartsWith("8.", StringComparison.Ordinal)
            || version.StartsWith("9.", StringComparison.Ordinal))
        {
            complexProperty.IsNullable = false;
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
                                RelationalStrings.MultipleAnnotationConflict(stripped[1..]));
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
