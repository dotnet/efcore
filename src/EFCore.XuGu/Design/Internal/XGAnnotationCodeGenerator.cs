// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Design.Internal
{
    public class XGAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        private static readonly MethodInfo _modelUseIdentityColumnsMethodInfo
            = typeof(XGModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGModelBuilderExtensions.AutoIncrementColumns),
                typeof(ModelBuilder));

        private static readonly MethodInfo _modelHasCharSetMethodInfo
            = typeof(XGModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGModelBuilderExtensions.HasCharSet),
                typeof(ModelBuilder),
                typeof(string),
                typeof(DelegationModes?));

        private static readonly MethodInfo _modelUseCollationMethodInfo
            = typeof(XGModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGModelBuilderExtensions.UseCollation),
                typeof(ModelBuilder),
                typeof(string),
                typeof(DelegationModes?));

        private static readonly MethodInfo _modelUseGuidCollationMethodInfo
            = typeof(XGModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGModelBuilderExtensions.UseGuidCollation),
                typeof(ModelBuilder),
                typeof(string));

        private static readonly MethodInfo _modelHasAnnotationMethodInfo
            = typeof(ModelBuilder).GetRequiredRuntimeMethod(
                nameof(ModelBuilder.HasAnnotation),
                typeof(string),
                typeof(object));

        private static readonly MethodInfo _entityTypeHasCharSetMethodInfo
            = typeof(XGEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGEntityTypeBuilderExtensions.HasCharSet),
                typeof(EntityTypeBuilder),
                typeof(string),
                typeof(DelegationModes?));

        private static readonly MethodInfo _entityTypeUseCollationMethodInfo
            = typeof(XGEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGEntityTypeBuilderExtensions.UseCollation),
                typeof(EntityTypeBuilder),
                typeof(string),
                typeof(DelegationModes?));

        private static readonly MethodInfo _propertyUseIdentityColumnMethodInfo
            = typeof(XGPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGPropertyBuilderExtensions.UseXGIdentityColumn),
                typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyUseComputedColumnMethodInfo
            = typeof(XGPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGPropertyBuilderExtensions.UseXGComputedColumn),
                typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyHasCharSetMethodInfo
            = typeof(XGPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGPropertyBuilderExtensions.HasCharSet),
                typeof(PropertyBuilder),
                typeof(string));

        private static readonly MethodInfo _complexTypePropertyHasCharSetMethodInfo
            = typeof(XGComplexTypePropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGComplexTypePropertyBuilderExtensions.HasCharSet),
                typeof(ComplexTypePropertyBuilder),
                typeof(string));

        public XGAnnotationCodeGenerator([JetBrains.Annotations.NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> FilterIgnoredAnnotations(IEnumerable<IAnnotation> annotations)
        {
            annotations = base.FilterIgnoredAnnotations(annotations).ToArray();

            var hasCharSetAnnotation = annotations.Any(a => a.Name == XGAnnotationNames.CharSet);
            var hasCollationAnnotation = annotations.Any(a => a.Name == RelationalAnnotationNames.Collation);

            foreach (var annotation in annotations)
            {
                // Charsets and their delegation and collations and their delegation are handled in the same Fluent API call.
                // Since the GenerateFluentApi methods cannot skip annotations, we have to ignore one of them here early, if both have been
                // set, so we don't output a HasCharSet()/UseCollation() call and a CharSetDelegation/CollationDelegation annotation in
                // addition to that.
                if (annotation.Name == XGAnnotationNames.CharSetDelegation && hasCharSetAnnotation ||
                    annotation.Name == XGAnnotationNames.CollationDelegation && hasCollationAnnotation)
                {
                    continue;
                }

                yield return annotation;
            }
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            if (annotation.Name == XGAnnotationNames.CharSet)
            {
                var delegationModes = model[XGAnnotationNames.CharSetDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    _modelHasCharSetMethodInfo,
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CharSetDelegation &&
                model[XGAnnotationNames.CharSet] is null)
            {
                return new MethodCallCodeFragment(
                    _modelHasCharSetMethodInfo,
                    null,
                    annotation.Value);
            }

            // EF Core currently just falls back on using the `Relational:Collation` annotation instead of generating the `UseCollation()`
            // method call (though it could), so we can return our method call fragment here, without generating an ugly duplicate.
            if (annotation.Name == RelationalAnnotationNames.Collation)
            {
                var delegationModes = model[XGAnnotationNames.CollationDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    _modelUseCollationMethodInfo,
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CollationDelegation &&
                model[RelationalAnnotationNames.Collation] is null)
            {
                return new MethodCallCodeFragment(
                    _modelUseCollationMethodInfo,
                    null,
                    annotation.Value);
            }

            if (annotation.Name == XGAnnotationNames.GuidCollation)
            {
                return new MethodCallCodeFragment(
                    _modelUseGuidCollationMethodInfo,
                    annotation.Value);
            }

            return null;
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            if (annotation.Name == XGAnnotationNames.CharSet)
            {
                var delegationModes = entityType[XGAnnotationNames.CharSetDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    _entityTypeHasCharSetMethodInfo,
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CharSetDelegation &&
                entityType[XGAnnotationNames.CharSet] is null)
            {
                return new MethodCallCodeFragment(
                    _entityTypeHasCharSetMethodInfo,
                    null,
                    annotation.Value);
            }

            if (annotation.Name == RelationalAnnotationNames.Collation)
            {
                var delegationModes = entityType[XGAnnotationNames.CollationDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    _entityTypeUseCollationMethodInfo,
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CollationDelegation &&
                entityType[RelationalAnnotationNames.Collation] is null)
            {
                return new MethodCallCodeFragment(
                    _entityTypeUseCollationMethodInfo,
                    null,
                    annotation.Value);
            }

            return null;
        }

        protected override AttributeCodeFragment GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == XGAnnotationNames.CharSet)
            {
                var delegationModes = entityType[XGAnnotationNames.CharSetDelegation] as DelegationModes?;
                return new AttributeCodeFragment(
                    typeof(XGCharSetAttribute),
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CharSetDelegation &&
                entityType[XGAnnotationNames.CharSet] is null)
            {
                return new AttributeCodeFragment(
                    typeof(XGCharSetAttribute),
                    null,
                    annotation.Value);
            }

            if (annotation.Name == RelationalAnnotationNames.Collation)
            {
                var delegationModes = entityType[XGAnnotationNames.CollationDelegation] as DelegationModes?;
                return new AttributeCodeFragment(
                    typeof(XGCollationAttribute),
                    new[] { annotation.Value }
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CollationDelegation &&
                entityType[RelationalAnnotationNames.Collation] is null)
            {
                return new AttributeCodeFragment(
                    typeof(XGCollationAttribute),
                    null,
                    annotation.Value);
            }

            return base.GenerateDataAnnotation(entityType, annotation);
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            // At this point, all legacy `XG:Collation` annotations should have been replaced by `Relational:Collation` ones.
#pragma warning disable 618
            Debug.Assert(annotation.Name != XGAnnotationNames.Collation);
#pragma warning restore 618

            switch (annotation.Name)
            {
                case XGAnnotationNames.CharSet when annotation.Value is string { Length: > 0 } charSet:
                    if (property.DeclaringType is IComplexType)
                    {
                        return new MethodCallCodeFragment(
                            _complexTypePropertyHasCharSetMethodInfo,
                            charSet);
                    }

                    return new MethodCallCodeFragment(
                        _propertyHasCharSetMethodInfo,
                        charSet);

                default:
                    return null;
            }
        }

        protected override AttributeCodeFragment GenerateDataAnnotation(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return annotation.Name switch
            {
                XGAnnotationNames.CharSet when annotation.Value is string { Length: > 0 } charSet => new AttributeCodeFragment(
                    typeof(XGCharSetAttribute), charSet),
                RelationalAnnotationNames.Collation when annotation.Value is string { Length: > 0 } collation => new AttributeCodeFragment(
                    typeof(XGCollationAttribute), collation),
                _ => base.GenerateDataAnnotation(property, annotation)
            };
        }

        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

            if (GenerateValueGenerationStrategy(annotations, onModel: true) is { } valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            return fragments;
        }

        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

            if (GenerateValueGenerationStrategy(annotations, onModel: false) is { } valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            return fragments;
        }

        private MethodCallCodeFragment GenerateValueGenerationStrategy(IDictionary<string, IAnnotation> annotations, bool onModel)
            => TryGetAndRemove(annotations, XGAnnotationNames.ValueGenerationStrategy, out XGValueGenerationStrategy strategy)
                ? strategy switch
                {
                    XGValueGenerationStrategy.IdentityColumn => new MethodCallCodeFragment(
                        onModel
                            ? _modelUseIdentityColumnsMethodInfo
                            : _propertyUseIdentityColumnMethodInfo),
                    XGValueGenerationStrategy.ComputedColumn => new MethodCallCodeFragment(_propertyUseComputedColumnMethodInfo),
                    XGValueGenerationStrategy.None => new MethodCallCodeFragment(
                        _modelHasAnnotationMethodInfo,
                        XGAnnotationNames.ValueGenerationStrategy,
                        XGValueGenerationStrategy.None),
                    _ => throw new ArgumentOutOfRangeException(strategy.ToString())
                }
                : null;

        private static bool TryGetAndRemove<T>(
            IDictionary<string, IAnnotation> annotations,
            string annotationName,
            [NotNullWhen(true)] out T annotationValue)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value is not null)
            {
                annotations.Remove(annotationName);
                annotationValue = (T)annotation.Value;
                return true;
            }

            annotationValue = default;
            return false;
        }
    }
}
