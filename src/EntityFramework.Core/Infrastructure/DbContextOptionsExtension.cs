// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.DependencyInjection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class DbContextOptionsExtension
    {
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

        protected IReadOnlyDictionary<string, string> RawOptions;

        protected internal virtual void Configure([NotNull] IReadOnlyDictionary<string, string> rawOptions)
        {
            Check.NotNull(rawOptions, "rawOptions");

            RawOptions = rawOptions;
        }

        protected T GetSetting<T>(string key)
        {
            string valueString;
            if (RawOptions.TryGetValue(key, out valueString))
            {
                return (T)_conversionFuncs[typeof(T).UnwrapNullableType()].Invoke(key, valueString);
            }
            return default(T);
        }

        protected internal abstract void ApplyServices([NotNull] EntityFrameworkServicesBuilder builder);
    }
}
