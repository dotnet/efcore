// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="DbFunctionParameter" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class DbFunctionParameterBuilder
    {
        private readonly InternalDbFunctionParameterBuilder _builder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbFunctionParameterBuilder(DbFunctionParameter dbFunctionParameter)
        {
            _builder = new InternalDbFunctionParameterBuilder(dbFunctionParameter);
        }

        /// <summary>
        ///     Specify if this parametere supports Nullability Propagation
        /// </summary>
        public virtual DbFunctionParameterBuilder HasNullabilityPropagation(bool supportsNullabilityPropagation)
        {
            _builder.HasNullabilityPropagation(supportsNullabilityPropagation);

            return this;
        }
    }
}
