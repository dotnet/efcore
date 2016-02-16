// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class CompositeRelationalParameter : IRelationalParameter
    {
        public CompositeRelationalParameter(
            [NotNull] string invariantName,
            [NotNull] IReadOnlyList<IRelationalParameter> relationalParameters)

        {
            Check.NotNull(invariantName, nameof(invariantName));
            Check.NotNull(relationalParameters, nameof(relationalParameters));

            InvariantName = invariantName;
            RelationalParameters = relationalParameters;
        }

        public virtual string InvariantName { get; }

        public virtual IReadOnlyList<IRelationalParameter> RelationalParameters { get; }

        public virtual void AddDbParameter(
            [NotNull] DbCommand command,
            [CanBeNull] object value)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(value, nameof(value));

            var innerValues = value as object[];

            if (innerValues != null)
            {
                if(innerValues.Length < RelationalParameters.Count)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MissingParameterValue(
                            RelationalParameters[innerValues.Length].InvariantName));
                }

                for (var i = 0; i < RelationalParameters.Count; i++)
                {
                    RelationalParameters[i].AddDbParameter(command, innerValues[i]);
                }
            }
            else
            {
                throw new InvalidOperationException(RelationalStrings.ParameterNotObjectArray(InvariantName));
            }
        }
    }
}
