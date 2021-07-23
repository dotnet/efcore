// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CompositeRelationalParameter : RelationalParameterBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CompositeRelationalParameter(
            string invariantName,
            IReadOnlyList<IRelationalParameter> relationalParameters)
            : base(invariantName)
        {
            Check.NotNull(relationalParameters, nameof(relationalParameters));

            RelationalParameters = relationalParameters;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> RelationalParameters { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddDbParameter(DbCommand command, object? value)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(value, nameof(value));

            if (value is object[] innerValues)
            {
                if (innerValues.Length < RelationalParameters.Count)
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
