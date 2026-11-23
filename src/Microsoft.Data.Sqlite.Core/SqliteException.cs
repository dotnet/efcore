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
    private const int SQLITE_ERROR_SNAPSHOT = SQLITE_ERROR | (3 << 8);
    private const int SQLITE_BUSY_TIMEOUT = SQLITE_BUSY | (3 << 8);
    private const int SQLITE_LOCKED_VTAB = SQLITE_LOCKED | (2 << 8);
    private const int SQLITE_READONLY_CANTINIT = SQLITE_READONLY | (5 << 8);
    private const int SQLITE_READONLY_DIRECTORY = SQLITE_READONLY | (6 << 8);
    private const int SQLITE_IOERR_VNODE = SQLITE_IOERR | (27 << 8);
    private const int SQLITE_IOERR_AUTH = SQLITE_IOERR | (28 << 8);
    private const int SQLITE_IOERR_BEGIN_ATOMIC = SQLITE_IOERR | (29 << 8);
    private const int SQLITE_IOERR_COMMIT_ATOMIC = SQLITE_IOERR | (30 << 8);
    private const int SQLITE_IOERR_ROLLBACK_ATOMIC = SQLITE_IOERR | (31 << 8);
    private const int SQLITE_IOERR_DATA = SQLITE_IOERR | (32 << 8);
    private const int SQLITE_IOERR_CORRUPTFS = SQLITE_IOERR | (33 << 8);
    private const int SQLITE_IOERR_IN_PAGE = SQLITE_IOERR | (34 << 8);
    private const int SQLITE_CORRUPT_SEQUENCE = SQLITE_CORRUPT | (2 << 8);
    private const int SQLITE_CORRUPT_INDEX = SQLITE_CORRUPT | (3 << 8);
    private const int SQLITE_CANTOPEN_DIRTYWAL = SQLITE_CANTOPEN | (5 << 8);
    private const int SQLITE_CANTOPEN_SYMLINK = SQLITE_CANTOPEN | (6 << 8);
    private const int SQLITE_CONSTRAINT_PINNED = SQLITE_CONSTRAINT | (11 << 8);
    private const int SQLITE_CONSTRAINT_DATATYPE = SQLITE_CONSTRAINT | (12 << 8);
    private const int SQLITE_AUTH_USER = SQLITE_AUTH | (1 << 8);

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
        // When the caller has enabled extended result codes the two are equal, but the name is still worth showing.
        var name = GetExtendedErrorName(extendedErrorCode);
        if (extendedErrorCode != errorCode
            || name != null)
        {
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
            SQLITE_ERROR_SNAPSHOT => nameof(SQLITE_ERROR_SNAPSHOT),
            SQLITE_ABORT_ROLLBACK => nameof(SQLITE_ABORT_ROLLBACK),
            SQLITE_BUSY_RECOVERY => nameof(SQLITE_BUSY_RECOVERY),
            SQLITE_BUSY_SNAPSHOT => nameof(SQLITE_BUSY_SNAPSHOT),
            SQLITE_BUSY_TIMEOUT => nameof(SQLITE_BUSY_TIMEOUT),
            SQLITE_LOCKED_SHAREDCACHE => nameof(SQLITE_LOCKED_SHAREDCACHE),
            SQLITE_LOCKED_VTAB => nameof(SQLITE_LOCKED_VTAB),
            SQLITE_READONLY_RECOVERY => nameof(SQLITE_READONLY_RECOVERY),
            SQLITE_READONLY_CANTLOCK => nameof(SQLITE_READONLY_CANTLOCK),
            SQLITE_READONLY_ROLLBACK => nameof(SQLITE_READONLY_ROLLBACK),
            SQLITE_READONLY_DBMOVED => nameof(SQLITE_READONLY_DBMOVED),
            SQLITE_READONLY_CANTINIT => nameof(SQLITE_READONLY_CANTINIT),
            SQLITE_READONLY_DIRECTORY => nameof(SQLITE_READONLY_DIRECTORY),
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
            SQLITE_IOERR_BLOCKED => nameof(SQLITE_IOERR_BLOCKED),
            SQLITE_IOERR_NOMEM => nameof(SQLITE_IOERR_NOMEM),
            SQLITE_IOERR_ACCESS => nameof(SQLITE_IOERR_ACCESS),
            SQLITE_IOERR_CHECKRESERVEDLOCK => nameof(SQLITE_IOERR_CHECKRESERVEDLOCK),
            SQLITE_IOERR_LOCK => nameof(SQLITE_IOERR_LOCK),
            SQLITE_IOERR_CLOSE => nameof(SQLITE_IOERR_CLOSE),
            SQLITE_IOERR_DIR_CLOSE => nameof(SQLITE_IOERR_DIR_CLOSE),
            SQLITE_IOERR_SHMOPEN => nameof(SQLITE_IOERR_SHMOPEN),
            SQLITE_IOERR_SHMSIZE => nameof(SQLITE_IOERR_SHMSIZE),
            SQLITE_IOERR_SHMLOCK => nameof(SQLITE_IOERR_SHMLOCK),
            SQLITE_IOERR_SHMMAP => nameof(SQLITE_IOERR_SHMMAP),
            SQLITE_IOERR_SEEK => nameof(SQLITE_IOERR_SEEK),
            SQLITE_IOERR_DELETE_NOENT => nameof(SQLITE_IOERR_DELETE_NOENT),
            SQLITE_IOERR_MMAP => nameof(SQLITE_IOERR_MMAP),
            SQLITE_IOERR_GETTEMPPATH => nameof(SQLITE_IOERR_GETTEMPPATH),
            SQLITE_IOERR_CONVPATH => nameof(SQLITE_IOERR_CONVPATH),
            SQLITE_IOERR_VNODE => nameof(SQLITE_IOERR_VNODE),
            SQLITE_IOERR_AUTH => nameof(SQLITE_IOERR_AUTH),
            SQLITE_IOERR_BEGIN_ATOMIC => nameof(SQLITE_IOERR_BEGIN_ATOMIC),
            SQLITE_IOERR_COMMIT_ATOMIC => nameof(SQLITE_IOERR_COMMIT_ATOMIC),
            SQLITE_IOERR_ROLLBACK_ATOMIC => nameof(SQLITE_IOERR_ROLLBACK_ATOMIC),
            SQLITE_IOERR_DATA => nameof(SQLITE_IOERR_DATA),
            SQLITE_IOERR_CORRUPTFS => nameof(SQLITE_IOERR_CORRUPTFS),
            SQLITE_IOERR_IN_PAGE => nameof(SQLITE_IOERR_IN_PAGE),
            SQLITE_CORRUPT_VTAB => nameof(SQLITE_CORRUPT_VTAB),
            SQLITE_CORRUPT_SEQUENCE => nameof(SQLITE_CORRUPT_SEQUENCE),
            SQLITE_CORRUPT_INDEX => nameof(SQLITE_CORRUPT_INDEX),
            SQLITE_CANTOPEN_NOTEMPDIR => nameof(SQLITE_CANTOPEN_NOTEMPDIR),
            SQLITE_CANTOPEN_ISDIR => nameof(SQLITE_CANTOPEN_ISDIR),
            SQLITE_CANTOPEN_FULLPATH => nameof(SQLITE_CANTOPEN_FULLPATH),
            SQLITE_CANTOPEN_CONVPATH => nameof(SQLITE_CANTOPEN_CONVPATH),
            SQLITE_CANTOPEN_DIRTYWAL => nameof(SQLITE_CANTOPEN_DIRTYWAL),
            SQLITE_CANTOPEN_SYMLINK => nameof(SQLITE_CANTOPEN_SYMLINK),
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
            SQLITE_CONSTRAINT_PINNED => nameof(SQLITE_CONSTRAINT_PINNED),
            SQLITE_CONSTRAINT_DATATYPE => nameof(SQLITE_CONSTRAINT_DATATYPE),
            SQLITE_AUTH_USER => nameof(SQLITE_AUTH_USER),
            _ => null
        };
}
