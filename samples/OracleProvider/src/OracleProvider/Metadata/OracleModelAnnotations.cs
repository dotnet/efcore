// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OracleModelAnnotations : RelationalModelAnnotations, IOracleModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        public OracleModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        protected OracleModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[OracleAnnotationNames.HiLoSequenceName];
            [param: CanBeNull] set => SetHiLoSequenceName(value);
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                OracleAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual OracleValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (OracleValueGenerationStrategy?)Annotations.Metadata[OracleAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(OracleValueGenerationStrategy? value)
            => Annotations.SetAnnotation(OracleAnnotationNames.ValueGenerationStrategy, value);
    }
}
