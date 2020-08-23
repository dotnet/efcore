// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Defines conversions from an object of one type in a model to an object of the same or
    ///     different type in the store.
    /// </summary>
    public class ValueConverter<TModel, TProvider> : ValueConverter
    {
        private Func<object, object> _convertToProvider;
        private Func<object, object> _convertFromProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverter{TModel,TProvider}" /> class.
        /// </summary>
        /// <param name="convertToProviderExpression"> An expression to convert objects when writing data to the store. </param>
        /// <param name="convertFromProviderExpression"> An expression to convert objects when reading data from the store. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public ValueConverter(
            [NotNull] Expression<Func<TModel, TProvider>> convertToProviderExpression,
            [NotNull] Expression<Func<TProvider, TModel>> convertFromProviderExpression,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
        {
        }

        private static Func<object, object> SanitizeConverter<TIn, TOut>(Expression<Func<TIn, TOut>> convertExpression)
        {
            var compiled = convertExpression.Compile();

            return v => v == null
                ? (object)null
                : compiled(Sanitize<TIn>(v));
        }

        private static T Sanitize<T>(object value)
        {
            var unwrappedType = typeof(T).UnwrapNullableType();

            return (T)(!unwrappedType.IsInstanceOfType(value)
                ? Convert.ChangeType(value, unwrappedType)
                : value);
        }

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public override Func<object, object> ConvertToProvider
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _convertToProvider, this, c => SanitizeConverter(c.ConvertToProviderExpression));

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public override Func<object, object> ConvertFromProvider
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _convertFromProvider, this, c => SanitizeConverter(c.ConvertFromProviderExpression));

        /// <summary>
        ///     Gets the expression to convert objects when writing data to the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Expression<Func<TModel, TProvider>> ConvertToProviderExpression
            => (Expression<Func<TModel, TProvider>>)base.ConvertToProviderExpression;

        /// <summary>
        ///     Gets the expression to convert objects when reading data from the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public new virtual Expression<Func<TProvider, TModel>> ConvertFromProviderExpression
            => (Expression<Func<TProvider, TModel>>)base.ConvertFromProviderExpression;

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public override Type ModelClrType
            => typeof(TModel);

        /// <summary>
        ///     The CLR type used when reading and writing from the store.
        /// </summary>
        public override Type ProviderClrType
            => typeof(TProvider);
    }
}
