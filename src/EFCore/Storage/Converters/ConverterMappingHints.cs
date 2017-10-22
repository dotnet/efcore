// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Specifies hints used by the type mapper when mapping using a <see cref="ValueConverter" />.
    /// </summary>
    public struct ConverterMappingHints
    {
        /// <summary>
        ///     Creates a new <see cref="ConverterMappingHints" /> instance. Any hint contained in the instance
        ///     can be <c>null</c> to indicate it has not been specified.
        /// </summary>
        /// <param name="size"> The suggested size of the mapped data type.</param>
        /// <param name="precision"> The suggested precision of the mapped data type. </param>
        /// <param name="scale"> The suggested scale of the mapped data type. </param>
        /// <param name="unicode"> Whether or not the mapped data type should support Unicode. </param>
        /// <param name="fixedLength"> Whether or not the mapped data type is fixed length. </param>
        /// <param name="sizeFunction"> A function for the original size to account for conversion expansion. </param>
        public ConverterMappingHints(
            int? size = null,
            int? precision = null,
            int? scale = null,
            bool? unicode = null,
            bool? fixedLength = null,
            [CanBeNull] Func<int, int> sizeFunction = null)
        {
            Size = size;
            Precision = precision;
            Scale = scale;
            IsUnicode = unicode;
            IsFixedLength = fixedLength;
            SizeFunction = sizeFunction;
        }

        /// <summary>
        ///     Adds hints from the given object to this one. Hints that are already specified are
        ///     not overridden.
        /// </summary>
        /// <param name="hints"> The hints to add. </param>
        /// <returns> The combined hints. </returns>
        public ConverterMappingHints With(ConverterMappingHints hints)
            => new ConverterMappingHints(
                CalculateSize(hints),
                Precision ?? hints.Precision,
                Scale ?? hints.Scale,
                IsUnicode ?? hints.IsUnicode,
                IsFixedLength ?? hints.IsFixedLength,
                Size != null ? null : (SizeFunction ?? hints.SizeFunction));

        private int? CalculateSize(ConverterMappingHints hints)
        {
            var size = Size ?? hints.Size;
            if (size != null)
            {
                var sizeFunc = SizeFunction ?? hints.SizeFunction;
                if (sizeFunc != null)
                {
                    return sizeFunc(size.Value);
                }
            }

            return size;
        }

        /// <summary>
        ///     The suggested size of the mapped data type.
        /// </summary>
        public int? Size { get; }

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public int? Precision { get; }

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public int? Scale { get; }

        /// <summary>
        ///     Whether or not the mapped data type should support Unicode.
        /// </summary>
        public bool? IsUnicode { get; }

        /// <summary>
        ///     Whether or not the mapped data type is fixed length.
        /// </summary>
        public bool? IsFixedLength { get; }

        /// <summary>
        ///     A function for the original size to account for conversion expansion.
        /// </summary>
        public Func<int, int> SizeFunction { get; }
    }
}
