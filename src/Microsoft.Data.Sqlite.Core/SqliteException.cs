// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

/// <summary>
///     Represents a SQLite error.
/// </summary>
/// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
public class SqliteException : DbException
{
    // Extended result codes that SQLitePCL.raw doesn't define.
    internal const int SQLITE_ERROR_MISSING_COLLSEQ = SQLITE_ERROR | (1 << 8);
    private const int SQLITE_ERROR_RETRY = SQLITE_ERROR | (2 << 8);
    private const int SQLITE_READONLY_CANTINIT = SQLITE_READONLY | (5 << 8);
    private const int SQLITE_READONLY_DIRECTORY = SQLITE_READONLY | (6 << 8);
    private const int SQLITE_CANTOPEN_DIRTYWAL = SQLITE_CANTOPEN | (5 << 8);

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteException" /> class.
    /// </summary>
    /// <param name="message">The message to display for the exception. Can be null.</param>
    /// <param name="errorCode">The SQLite error code.</param>
    public SqliteException(string? message, int errorCode)
        : this(message, errorCode, errorCode)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteException" /> class.
    /// </summary>
    /// <param name="message">The message to display for the exception. Can be null.</param>
    /// <param name="errorCode">The SQLite error code.</param>
    /// <param name="extendedErrorCode">The extended SQLite error code.</param>
    public SqliteException(string? message, int errorCode, int extendedErrorCode)
        : base(message)
    {
        SqliteErrorCode = errorCode;
        SqliteExtendedErrorCode = extendedErrorCode;
    }

    /// <summary>
    ///     Gets the SQLite error code.
    /// </summary>
    /// <value>The SQLite error code.</value>
    /// <seealso href="https://www.sqlite.org/rescode.html">SQLite Result Codes</seealso>
    public virtual int SqliteErrorCode { get; }

    /// <summary>
    ///     Gets the extended SQLite error code.
    /// </summary>
    /// <value>The SQLite error code.</value>
    /// <seealso href="https://www.sqlite.org/rescode.html#extrc">SQLite Result Codes</seealso>
    public virtual int SqliteExtendedErrorCode { get; }

    /// <summary>
    ///     Throws an exception with a specific SQLite error code value.
    /// </summary>
    /// <param name="rc">The SQLite error code corresponding to the desired exception.</param>
    /// <param name="db">A handle to database connection.</param>
    /// <remarks>
    ///     No exception is thrown for non-error result codes.
    /// </remarks>
    public static void ThrowExceptionForRC(int rc, sqlite3? db)
    {
        if (rc is SQLITE_OK or SQLITE_ROW or SQLITE_DONE)
        {
            return;
        }

        string message;
        int extendedErrorCode;
        if (db == null
            || db.IsInvalid
            || rc != sqlite3_errcode(db))
        {
            message = sqlite3_errstr(rc).utf8_to_string() + " " + Resources.DefaultNativeError;
            extendedErrorCode = rc;
        }
        else
        {
            message = sqlite3_errmsg(db).utf8_to_string();
            extendedErrorCode = sqlite3_extended_errcode(db);
        }

        throw new SqliteException(FormatMessage(rc, extendedErrorCode, message), rc, extendedErrorCode);
    }

    internal static string FormatMessage(int errorCode, int extendedErrorCode, string message)
    {
        var result = Resources.SqliteNativeError(errorCode, message);

        // Surface the extended code, since it often identifies the specific cause (e.g. which kind of constraint failed).
        if (extendedErrorCode != errorCode)
        {
            var name = GetExtendedErrorName(extendedErrorCode);
            result += " " + Resources.SqliteExtendedErrorCode(name == null ? extendedErrorCode : $"{extendedErrorCode} ({name})");
        }

        var guidance = GetGuidance(extendedErrorCode);
        if (guidance != null)
        {
            result += " " + guidance;
        }

        return result;
    }

    // Only for errors where there is a common underlying cause that the raw SQLite message doesn't make clear.
    // Plain SQLITE_BUSY has too many causes (another writer, backup, ...) for guidance to be accurate.
    private static string? GetGuidance(int extendedErrorCode)
        => extendedErrorCode switch
        {
            SQLITE_BUSY_SNAPSHOT => Resources.SqliteBusySnapshotGuidance,
            _ => null
        };

    private static string? GetExtendedErrorName(int extendedErrorCode)
        => extendedErrorCode switch
        {
            SQLITE_ERROR_MISSING_COLLSEQ => nameof(SQLITE_ERROR_MISSING_COLLSEQ),
            SQLITE_ERROR_RETRY => nameof(SQLITE_ERROR_RETRY),
            SQLITE_BUSY_RECOVERY => nameof(SQLITE_BUSY_RECOVERY),
            SQLITE_BUSY_SNAPSHOT => nameof(SQLITE_BUSY_SNAPSHOT),
            SQLITE_LOCKED_SHAREDCACHE => nameof(SQLITE_LOCKED_SHAREDCACHE),
            SQLITE_READONLY_RECOVERY => nameof(SQLITE_READONLY_RECOVERY),
            SQLITE_READONLY_CANTLOCK => nameof(SQLITE_READONLY_CANTLOCK),
            SQLITE_READONLY_ROLLBACK => nameof(SQLITE_READONLY_ROLLBACK),
            SQLITE_READONLY_DBMOVED => nameof(SQLITE_READONLY_DBMOVED),
            SQLITE_READONLY_CANTINIT => nameof(SQLITE_READONLY_CANTINIT),
            SQLITE_READONLY_DIRECTORY => nameof(SQLITE_READONLY_DIRECTORY),
            SQLITE_CANTOPEN_NOTEMPDIR => nameof(SQLITE_CANTOPEN_NOTEMPDIR),
            SQLITE_CANTOPEN_ISDIR => nameof(SQLITE_CANTOPEN_ISDIR),
            SQLITE_CANTOPEN_FULLPATH => nameof(SQLITE_CANTOPEN_FULLPATH),
            SQLITE_CANTOPEN_CONVPATH => nameof(SQLITE_CANTOPEN_CONVPATH),
            SQLITE_CANTOPEN_DIRTYWAL => nameof(SQLITE_CANTOPEN_DIRTYWAL),
            SQLITE_IOERR_READ => nameof(SQLITE_IOERR_READ),
            SQLITE_IOERR_SHORT_READ => nameof(SQLITE_IOERR_SHORT_READ),
            SQLITE_IOERR_WRITE => nameof(SQLITE_IOERR_WRITE),
            SQLITE_IOERR_FSYNC => nameof(SQLITE_IOERR_FSYNC),
            SQLITE_IOERR_DIR_FSYNC => nameof(SQLITE_IOERR_DIR_FSYNC),
            SQLITE_IOERR_TRUNCATE => nameof(SQLITE_IOERR_TRUNCATE),
            SQLITE_IOERR_FSTAT => nameof(SQLITE_IOERR_FSTAT),
            SQLITE_IOERR_UNLOCK => nameof(SQLITE_IOERR_UNLOCK),
            SQLITE_IOERR_RDLOCK => nameof(SQLITE_IOERR_RDLOCK),
            SQLITE_IOERR_DELETE => nameof(SQLITE_IOERR_DELETE),
            SQLITE_IOERR_NOMEM => nameof(SQLITE_IOERR_NOMEM),
            SQLITE_IOERR_ACCESS => nameof(SQLITE_IOERR_ACCESS),
            SQLITE_IOERR_CHECKRESERVEDLOCK => nameof(SQLITE_IOERR_CHECKRESERVEDLOCK),
            SQLITE_IOERR_LOCK => nameof(SQLITE_IOERR_LOCK),
            SQLITE_IOERR_CLOSE => nameof(SQLITE_IOERR_CLOSE),
            SQLITE_IOERR_SEEK => nameof(SQLITE_IOERR_SEEK),
            SQLITE_CONSTRAINT_CHECK => nameof(SQLITE_CONSTRAINT_CHECK),
            SQLITE_CONSTRAINT_COMMITHOOK => nameof(SQLITE_CONSTRAINT_COMMITHOOK),
            SQLITE_CONSTRAINT_FOREIGNKEY => nameof(SQLITE_CONSTRAINT_FOREIGNKEY),
            SQLITE_CONSTRAINT_FUNCTION => nameof(SQLITE_CONSTRAINT_FUNCTION),
            SQLITE_CONSTRAINT_NOTNULL => nameof(SQLITE_CONSTRAINT_NOTNULL),
            SQLITE_CONSTRAINT_PRIMARYKEY => nameof(SQLITE_CONSTRAINT_PRIMARYKEY),
            SQLITE_CONSTRAINT_TRIGGER => nameof(SQLITE_CONSTRAINT_TRIGGER),
            SQLITE_CONSTRAINT_UNIQUE => nameof(SQLITE_CONSTRAINT_UNIQUE),
            SQLITE_CONSTRAINT_VTAB => nameof(SQLITE_CONSTRAINT_VTAB),
            SQLITE_CONSTRAINT_ROWID => nameof(SQLITE_CONSTRAINT_ROWID),
            _ => null
        };
}
