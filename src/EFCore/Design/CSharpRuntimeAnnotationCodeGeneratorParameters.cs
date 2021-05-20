// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     The parameter object for a <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
    /// </summary>
    public sealed record CSharpRuntimeAnnotationCodeGeneratorParameters
    {
        /// <summary>
        ///     <para>
        ///         Creates the parameter object for a <see cref="ICSharpRuntimeAnnotationCodeGenerator" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new parameters are added.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public CSharpRuntimeAnnotationCodeGeneratorParameters(
            string targetName,
            string className,
            IndentedStringBuilder mainBuilder,
            IndentedStringBuilder methodBuilder,
            ISet<string> namespaces,
            ISet<string> scopeVariables)
        {
            TargetName = targetName;
            ClassName = className;
            MainBuilder = mainBuilder;
            MethodBuilder = methodBuilder;
            Namespaces = namespaces;
            ScopeVariables = scopeVariables;
        }

        /// <summary>
        ///     The set of annotations from which to generate fluent API calls.
        /// </summary>
        public IDictionary<string, object?> Annotations { get; init; } = null!;

        /// <summary>
        ///     The name of the target variable.
        /// </summary>
        public string TargetName { get; init; }

        /// <summary>
        ///     The name of the current class.
        /// </summary>
        public string ClassName { get; init; }

        /// <summary>
        ///     The builder for the code building the metadata item.
        /// </summary>
        public IndentedStringBuilder MainBuilder { get; init; }

        /// <summary>
        ///     The builder that could be used to add members to the current class.
        /// </summary>
        public IndentedStringBuilder MethodBuilder { get; init; }

        /// <summary>
        ///     A collection of namespaces for <see langword="using"/> generation.
        /// </summary>
        public ISet<string> Namespaces { get; init; }

        /// <summary>
        ///     A collection of variable names in the current scope.
        /// </summary>
        public ISet<string> ScopeVariables { get; init; }

        /// <summary>
        ///     Indicates whether the given annotations are runtime annotations.
        /// </summary>
        public bool IsRuntime { get; init; }
    }
}
