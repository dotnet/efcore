// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

            if (GenerateValueGenerationStrategy(annotations, onModel: true) is MethodCallCodeFragment valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

            if (GenerateValueGenerationStrategy(annotations, onModel: false) is MethodCallCodeFragment valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            if (GetAndRemove<bool?>(annotations, SqlServerAnnotationNames.Sparse) is bool isSparse)
            {
                fragments.Add(
                    isSparse
                        ? new(nameof(SqlServerPropertyBuilderExtensions.IsSparse))
                        : new(nameof(SqlServerPropertyBuilderExtensions.IsSparse), false));
            }

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
            {
                return (string?)annotation.Value == "dbo";
            }

            return annotation.Name == SqlServerAnnotationNames.ValueGenerationStrategy
                && (SqlServerValueGenerationStrategy)annotation.Value! == SqlServerValueGenerationStrategy.IdentityColumn;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
            => annotation.Name == SqlServerAnnotationNames.Clustered
                ? (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.IsClustered), false)
                    : new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.IsClustered))
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
            => annotation.Name switch
            {
                SqlServerAnnotationNames.Clustered => (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.IsClustered), false)
                    : new MethodCallCodeFragment(nameof(SqlServerIndexBuilderExtensions.IsClustered)),

                SqlServerAnnotationNames.Include => new MethodCallCodeFragment(
                    nameof(SqlServerIndexBuilderExtensions.IncludeProperties), annotation.Value),

                SqlServerAnnotationNames.FillFactor => new MethodCallCodeFragment(
                    nameof(SqlServerIndexBuilderExtensions.HasFillFactor), annotation.Value),

                _ => null
            };

        private MethodCallCodeFragment? GenerateValueGenerationStrategy(
            IDictionary<string, IAnnotation> annotations,
            bool onModel)
        {
            SqlServerValueGenerationStrategy strategy;
            if (annotations.TryGetValue(SqlServerAnnotationNames.ValueGenerationStrategy, out var strategyAnnotation)
                && strategyAnnotation.Value != null)
            {
                annotations.Remove(SqlServerAnnotationNames.ValueGenerationStrategy);
                strategy = (SqlServerValueGenerationStrategy)strategyAnnotation.Value;
            }
            else
            {
                return null;
            }

            switch (strategy)
            {
                case SqlServerValueGenerationStrategy.IdentityColumn:
                    var seed = GetAndRemove<int?>(annotations, SqlServerAnnotationNames.IdentitySeed) ?? 1;
                    var increment = GetAndRemove<int?>(annotations, SqlServerAnnotationNames.IdentityIncrement) ?? 1;
                    return new(
                        onModel
                            ? nameof(SqlServerModelBuilderExtensions.UseIdentityColumns)
                            : nameof(SqlServerPropertyBuilderExtensions.UseIdentityColumn),
                        (seed, increment) switch
                        {
                            (1, 1) => Array.Empty<object>(),
                            (_, 1) => new object[] { seed },
                            _ => new object[] { seed, increment }
                        });

                case SqlServerValueGenerationStrategy.SequenceHiLo:
                    var name = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceName);
                    var schema = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceSchema);
                    return new(
                        nameof(SqlServerModelBuilderExtensions.UseHiLo),
                        (name, schema) switch
                        {
                            (null, null) => Array.Empty<object>(),
                            (_, null) => new object[] { name! },
                            _ => new object[] { name!, schema! }
                        });

                case SqlServerValueGenerationStrategy.None:
                    return new(
                        nameof(ModelBuilder.HasAnnotation),
                        SqlServerAnnotationNames.ValueGenerationStrategy,
                        SqlServerValueGenerationStrategy.None);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static T? GetAndRemove<T>(IDictionary<string, IAnnotation> annotations, string annotationName)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value != null)
            {
                annotations.Remove(annotationName);
                return (T)annotation.Value;
            }

            return default;
        }
    }
}
