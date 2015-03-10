// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class DbContextOptions : IDbContextOptions
    {
        protected DbContextOptions(
            [NotNull] IReadOnlyDictionary<string, string> rawOptions,
            [NotNull] IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
        {
            Check.NotNull(rawOptions, nameof(rawOptions));
            Check.NotNull(extensions, nameof(extensions));

            RawOptions = rawOptions;
            _extensions = extensions;
        }

        public virtual IReadOnlyDictionary<string, string> RawOptions { get; }

        public virtual IEnumerable<IDbContextOptionsExtension> Extensions => _extensions.Values;

        public virtual TExtension FindExtension<TExtension>()
            where TExtension : class, IDbContextOptionsExtension
        {
            IDbContextOptionsExtension extension;
            return _extensions.TryGetValue(typeof(TExtension), out extension) ? (TExtension)extension : null;
        }

        public abstract DbContextOptions WithExtension<TExtension>([NotNull] TExtension extension)
            where TExtension : class, IDbContextOptionsExtension;

        public virtual TValue FindRawOption<TValue>(string key)
        {
            Check.NotNull(key, nameof(key));

            string valueString;
            if (RawOptions.TryGetValue(key, out valueString))
            {
                return (TValue)_conversionFuncs[typeof(TValue).UnwrapNullableType()].Invoke(key, valueString);
            }
            return default(TValue);
        }

        private static readonly Dictionary<Type, Func<string, string, object>> _conversionFuncs = new Dictionary<Type, Func<string, string, object>>
            {
                {
                    typeof(int), (valueKey, valueString) =>
                        {
                            int valueInt;
                            if (!Int32.TryParse(valueString, out valueInt))
                            {
                                throw new InvalidOperationException(Strings.IntegerConfigurationValueFormatError(valueKey, valueString));
                            }
                            return valueInt;
                        }
                },
                {
                    typeof(string), (valueKey, valueString) => valueString
                }
            };

        private readonly IReadOnlyDictionary<Type, IDbContextOptionsExtension> _extensions;
    }
}
