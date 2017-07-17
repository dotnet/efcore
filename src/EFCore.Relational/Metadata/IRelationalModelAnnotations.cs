// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     API for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IModel)" />.
    /// </summary>
    public interface IRelationalModelAnnotations
    {
        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        ISequence FindSequence([NotNull] string name, [CanBeNull] string schema = null);

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        IDbFunction FindDbFunction([NotNull] MethodInfo method);

        /// <summary>
        ///     All <see cref="ISequence" />s contained in the model.
        /// </summary>
        IReadOnlyList<ISequence> Sequences { get; }

        /// <summary>
        ///     All <see cref="IDbFunction" />s contained in the model.
        /// </summary>
        IReadOnlyList<IDbFunction> DbFunctions { get; }

        /// <summary>
        ///     The default schema to use for the model, or <c>null</c> if none has been explicitly set.
        /// </summary>
        string DefaultSchema { get; }
    }
}
