// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class CompositeRelationalParameter : IRelationalParameter
    {
        private readonly IRelationalParameter[] _relationalParameters;

        public CompositeRelationalParameter(
            [NotNull] string invariantName, [NotNull] object value)

        {
            Check.NotNull(invariantName, nameof(invariantName));
            Check.NotNull(value, nameof(value));

            InvariantName = invariantName;

            _relationalParameters = (IRelationalParameter[])value;
        }

        public virtual string InvariantName { get; }

        public virtual void AddDbParameter(DbCommand command, object value)
        {
            Check.NotNull(command, nameof(command));

            if (value != null)
            {
                var innerValues = (object[])value;

                for (var i = 0; i < _relationalParameters.Length; i++)
                {
                    _relationalParameters[i].AddDbParameter(command, innerValues[i]);
                }
            }
        }
    }
}
