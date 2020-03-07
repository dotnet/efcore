// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
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
            [NotNull] string invariantName,
            [NotNull] IReadOnlyList<IRelationalParameter> relationalParameters)

        {
            Check.NotNull(invariantName, nameof(invariantName));
            Check.NotNull(relationalParameters, nameof(relationalParameters));

            InvariantName = invariantName;
            RelationalParameters = relationalParameters;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string InvariantName { get; }

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
        public override void AddDbParameter(DbCommand command, object value)
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
