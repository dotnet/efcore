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
    public class ConverterMappingHints
    {
        /// <summary>
        ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
        ///     can be <c>null</c> to indicate it has not been specified.
        /// </summary>
        /// <param name="size"> The suggested size of the mapped data type.</param>
        /// <param name="precision"> The suggested precision of the mapped data type. </param>
        /// <param name="scale"> The suggested scale of the mapped data type. </param>
        /// <param name="unicode"> Whether or not the mapped data type should support Unicode. </param>
        /// <param name="valueGeneratorFactory"> An optional factory for creating a specific <see cref="ValueGenerator" />. </param>
        public ConverterMappingHints(
            int? size = null,
            int? precision = null,
            int? scale = null,
            bool? unicode = null,
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> valueGeneratorFactory = null)
        {
            Size = size;
            Precision = precision;
            Scale = scale;
            IsUnicode = unicode;
            ValueGeneratorFactory = valueGeneratorFactory;
        }

        /// <summary>
        ///     Adds hints from the given object to this one. Hints that are already specified are
        ///     not overridden.
        /// </summary>
        /// <param name="hints"> The hints to add. </param>
        /// <returns> The combined hints. </returns>
        public virtual ConverterMappingHints With([CanBeNull] ConverterMappingHints hints)
            => hints == null
                ? this
                : new ConverterMappingHints(
                    hints.Size ?? Size,
                    hints.Precision ?? Precision,
                    hints.Scale ?? Scale,
                    hints.IsUnicode ?? IsUnicode,
                    hints.ValueGeneratorFactory ?? ValueGeneratorFactory);

        /// <summary>
        ///     The suggested size of the mapped data type.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public virtual int? Precision { get; }

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public virtual int? Scale { get; }

        /// <summary>
        ///     Whether or not the mapped data type should support Unicode.
        /// </summary>
        public virtual bool? IsUnicode { get; }

        /// <summary>
        ///     An optional factory for creating a specific <see cref="ValueGenerator" /> to use for model
        ///     values when this converter is being used.
        /// </summary>
        public virtual Func<IProperty, IEntityType, ValueGenerator> ValueGeneratorFactory { get; }
    }
}
