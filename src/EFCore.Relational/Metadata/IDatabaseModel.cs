// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table-like object in the database.
    /// </summary>
    public interface IRelationalModel : IAnnotatable
    {
        /// <summary>
        ///     Gets the full model.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Returns all the tables mapped in the model.
        /// </summary>
        IEnumerable<ITable> Tables { get; }

        /// <summary>
        ///     Returns all the views mapped in the model.
        /// </summary>
        /// <returns> All the views mapped in the model. </returns>
        IEnumerable<IView> Views { get; }

        /// <summary>
        ///     Returns all sequences contained in the model.
        /// </summary>
        IEnumerable<ISequence> Sequences => Model.GetSequences();

        /// <summary>
        ///     Returns all user-defined functions contained in the model.
        /// </summary>
        IEnumerable<IDbFunction> DbFunctions => Model.GetDbFunctions();

        /// <summary>
        ///     Gets the table with a given name. Returns <c>null</c> if no table with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The table with a given name or <c>null</c> if no table with the given name is defined. </returns>
        ITable FindTable([NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Gets the view with a given name. Returns <c>null</c> if no view with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The view with a given name or <c>null</c> if no view with the given name is defined. </returns>
        IView FindView([NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        ISequence FindSequence([NotNull] string name, [CanBeNull] string schema)
            => Model.FindSequence(name, schema);

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        IDbFunction FindDbFunction([NotNull] MethodInfo method)
            => DbFunction.FindDbFunction(Model, Check.NotNull(method, nameof(method)));

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given name.
        /// </summary>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        IDbFunction FindDbFunction([NotNull] string name)
            => DbFunction.FindDbFunction(Model, Check.NotNull(name, nameof(name)));
    }
}
