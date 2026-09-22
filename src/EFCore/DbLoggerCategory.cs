// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     An API for getting logger categories in an Intellisense/tab-completion friendly manner.
/// </summary>
/// <remarks>
///     <para>
///         Get an Entity Framework Core logger category using its Name property. For example,
///         <c>LoggerCategory.Database.Sql.Name</c>.
///     </para>
///     <para>
///         Use these types with <see cref="IDiagnosticsLogger{TLoggerCategory}" /> or
///         <see cref="IDiagnosticsLogger{TLoggerCategory}" /> to create a logger.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
///     </para>
/// </remarks>
public static class DbLoggerCategory
{
    /// <summary>
    ///     The root/prefix for all Entity Framework categories.
    /// </summary>
    public const string Name = "Microsoft.EntityFrameworkCore";

    /// <summary>
    ///     Logger categories for messages related to database interactions.
    /// </summary>
    public class Database : LoggerCategory<Database>
    {
        /// <summary>
        ///     Logger category for messages related to connection operations.
        /// </summary>
        public class Connection : LoggerCategory<Connection>;

        /// <summary>
        ///     Logger category for command execution, including SQL sent to the database.
        /// </summary>
        public class Command : LoggerCategory<Command>;

        /// <summary>
        ///     Logger category for messages related to transaction operations.
        /// </summary>
        public class Transaction : LoggerCategory<Transaction>;
    }

    /// <summary>
    ///     Logger category for messages related to <see cref="DbContext.SaveChanges()" />, excluding
    ///     messages specifically relating to database interactions which are covered by
    ///     the <see cref="Database" /> categories.
    /// </summary>
    public class Update : LoggerCategory<Update>;

    /// <summary>
    ///     Logger categories for messages related to model building and metadata.
    /// </summary>
    public class Model : LoggerCategory<Model>
    {
        /// <summary>
        ///     Logger category for messages from model validation.
        /// </summary>
        public class Validation : LoggerCategory<Validation>;
    }

    /// <summary>
    ///     Logger category for messages related to queries, excluding
    ///     the generated SQL, which is in the <see cref="Database.Command" /> category.
    /// </summary>
    public class Query : LoggerCategory<Query>;

    /// <summary>
    ///     Logger category for miscellaneous messages from the Entity Framework infrastructure.
    /// </summary>
    public class Infrastructure : LoggerCategory<Infrastructure>;

    /// <summary>
    ///     Logger category for messages from scaffolding/reverse engineering.
    /// </summary>
    public class Scaffolding : LoggerCategory<Scaffolding>;

    /// <summary>
    ///     Logger category messages from Migrations.
    /// </summary>
    public class Migrations : LoggerCategory<Migrations>;

    /// <summary>
    ///     Logger category for messages from change detection and tracking.
    /// </summary>
    public class ChangeTracking : LoggerCategory<ChangeTracking>;
}
