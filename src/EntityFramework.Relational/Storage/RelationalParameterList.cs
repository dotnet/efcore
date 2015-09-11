// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalParameterList
    {
        private readonly List<RelationalParameter> _parameters = new List<RelationalParameter>();

        public virtual IReadOnlyList<RelationalParameter> RelationalParameters => _parameters;

        public virtual RelationalParameter GetOrAdd(
            [NotNull] string name,
            [CanBeNull] object value,
            [CanBeNull] IProperty property = null)
        {
            Check.NotEmpty(name, nameof(name));

            var parameter = _parameters.FirstOrDefault(p => p.Name == name);

            if (parameter != null)
            {
                if (parameter.Value != value)
                {
                    throw new ArgumentException(Strings.DuplicateParameterName(name));
                }

                return parameter;
            }

            parameter = new RelationalParameter(
                name,
                value,
                property);

            _parameters.Add(parameter);

            return parameter;
        }
    }
}
