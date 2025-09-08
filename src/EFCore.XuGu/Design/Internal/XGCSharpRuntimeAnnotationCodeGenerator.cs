// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Design.Internal;

// Used to generate a compiled model. The compiled model is only used at app runtime and not for design-time purposes.
// Therefore, all annotations that are related to design-time concerns (i.e. databases, tables or columns) are superfluous and should be
// removed.
// TOOD: Check behavior for `ValueGenerationStrategy`, `LegacyValueGeneratedOnAdd` and `LegacyValueGeneratedOnAddOrUpdate`.
public class XGCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
{
    public XGCSharpRuntimeAnnotationCodeGenerator(
        CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
        RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer valueComparer = null,
        ValueComparer keyValueComparer = null,
        ValueComparer providerValueComparer = null)
    {
        var result = base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);

        if (typeMapping is IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator extension)
        {
            extension.Create(parameters, Dependencies);
        }

        return result;
    }

    public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
            annotations.Remove(XGAnnotationNames.CharSetDelegation);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.CollationDelegation);
            annotations.Remove(XGAnnotationNames.GuidCollation);
        }

        base.Generate(model, parameters);
    }

    public override void Generate(IRelationalModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
            annotations.Remove(XGAnnotationNames.CharSetDelegation);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.CollationDelegation);
            annotations.Remove(XGAnnotationNames.GuidCollation);

            annotations.Remove(RelationalAnnotationNames.Collation);
        }

        base.Generate(model, parameters);
    }

    public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
            annotations.Remove(XGAnnotationNames.CharSetDelegation);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.CollationDelegation);
            annotations.Remove(XGAnnotationNames.StoreOptions);

            annotations.Remove(RelationalAnnotationNames.Collation);
        }

        base.Generate(entityType, parameters);
    }

    public override void Generate(ITable table, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
            annotations.Remove(XGAnnotationNames.CharSetDelegation);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.CollationDelegation);
            annotations.Remove(XGAnnotationNames.StoreOptions);

            annotations.Remove(RelationalAnnotationNames.Collation);
        }

        base.Generate(table, parameters);
    }

    public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.SpatialReferenceSystemId);

            annotations.Remove(RelationalAnnotationNames.Collation);

            if (!annotations.ContainsKey(XGAnnotationNames.ValueGenerationStrategy))
            {
                annotations[XGAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }

        base.Generate(property, parameters);
    }

    public override void Generate(IColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.CharSet);
#pragma warning disable CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.Collation);
#pragma warning restore CS0618 // Type or member is obsolete
            annotations.Remove(XGAnnotationNames.SpatialReferenceSystemId);
            annotations.Remove(XGAnnotationNames.ValueGenerationStrategy);

            annotations.Remove(RelationalAnnotationNames.Collation);
        }

        base.Generate(column, parameters);
    }

    public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.FullTextIndex);
            annotations.Remove(XGAnnotationNames.FullTextParser);
            annotations.Remove(XGAnnotationNames.IndexPrefixLength);
            annotations.Remove(XGAnnotationNames.SpatialIndex);
        }

        base.Generate(index, parameters);
    }

    public override void Generate(ITableIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.FullTextIndex);
            annotations.Remove(XGAnnotationNames.FullTextParser);
            annotations.Remove(XGAnnotationNames.IndexPrefixLength);
            annotations.Remove(XGAnnotationNames.SpatialIndex);
        }

        base.Generate(index, parameters);
    }

    public override void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;

            annotations.Remove(XGAnnotationNames.IndexPrefixLength);
        }

        base.Generate(key, parameters);
    }
}
