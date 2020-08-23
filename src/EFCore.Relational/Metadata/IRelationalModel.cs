// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database.
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
        ///     Returns all the SQL queries mapped in the model.
        /// </summary>
        /// <returns> All the SQL queries mapped in the model. </returns>
        IEnumerable<ISqlQuery> Queries { get; }

        /// <summary>
        ///     Returns all sequences contained in the model.
        /// </summary>
        IEnumerable<ISequence> Sequences
            => Model.GetSequences();

        /// <summary>
        ///     Returns all user-defined functions contained in the model.
        /// </summary>
        IEnumerable<IStoreFunction> Functions { get; }

        /// <summary>
        ///     Returns the database collation.
        /// </summary>
        string Collation
            => Model.GetCollation();

        /// <summary>
        ///     Gets the table with the given name. Returns <see langword="null" /> if no table with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The table with a given name or <see langword="null" /> if no table with the given name is defined. </returns>
        ITable FindTable([NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Gets the view with the given name. Returns <see langword="null" /> if no view with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The view with a given name or <see langword="null" /> if no view with the given name is defined. </returns>
        IView FindView([NotNull] string name, [CanBeNull] string schema);

        /// <summary>
        ///     Gets the SQL query with the given name. Returns <see langword="null" /> if no SQL query with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the SQL query. </param>
        /// <returns> The SQL query with a given name or <see langword="null" /> if no SQL query with the given name is defined. </returns>
        ISqlQuery FindQuery([NotNull] string name);

        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        ISequence FindSequence([NotNull] string name, [CanBeNull] string schema)
            => Model.FindSequence(name, schema);

        /// <summary>
        ///     Finds a <see cref="IStoreFunction" /> with the given signature.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="schema"> The schema of the function. </param>
        /// <param name="parameters"> A list of parameter types. </param>
        /// <returns> The <see cref="IStoreFunction" /> or <see langword="null" /> if no function with the given name was defined. </returns>
        IStoreFunction FindFunction([NotNull] string name, [CanBeNull] string schema, [NotNull] IReadOnlyList<string> parameters);
    }
}
