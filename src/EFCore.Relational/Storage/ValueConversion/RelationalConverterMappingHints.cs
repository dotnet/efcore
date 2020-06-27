// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Specifies hints used by the type mapper when mapping using a <see cref="ValueConverter" />.
    /// </summary>
    public class RelationalConverterMappingHints : ConverterMappingHints
    {
        /// <summary>
        ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
        ///     can be <see langword="null" /> to indicate it has not been specified.
        /// </summary>
        /// <param name="size"> The suggested size of the mapped data type.</param>
        /// <param name="precision"> The suggested precision of the mapped data type. </param>
        /// <param name="scale"> The suggested scale of the mapped data type. </param>
        /// <param name="unicode"> Whether or not the mapped data type should support Unicode. </param>
        /// <param name="fixedLength"> Whether or not the mapped data type is fixed length. </param>
        /// <param name="valueGeneratorFactory"> An optional factory for creating a specific <see cref="ValueGenerator" />. </param>
        public RelationalConverterMappingHints(
            int? size = null,
            int? precision = null,
            int? scale = null,
            bool? unicode = null,
            bool? fixedLength = null,
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory = null)
            : base(size, precision, scale, unicode, valueGeneratorFactory)
        {
            IsFixedLength = fixedLength;
        }

        /// <summary>
        ///     Adds hints from the given object to this one. Hints that are already specified are
        ///     not overridden.
        /// </summary>
        /// <param name="hints"> The hints to add. </param>
        /// <returns> The combined hints. </returns>
        public override ConverterMappingHints With(ConverterMappingHints hints)
            => hints == null
                ? this
                : new RelationalConverterMappingHints(
                    hints.Size ?? Size,
                    hints.Precision ?? Precision,
                    hints.Scale ?? Scale,
                    hints.IsUnicode ?? IsUnicode,
                    (hints as RelationalConverterMappingHints)?.IsFixedLength ?? IsFixedLength,
                    hints.ValueGeneratorFactory ?? ValueGeneratorFactory);

        /// <summary>
        ///     Whether or not the mapped data type is fixed length.
        /// </summary>
        public virtual bool? IsFixedLength { get; }
    }
}
