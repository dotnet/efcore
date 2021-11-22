// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     Detects the exceptions caused by SQL Server transient failures.
/// </summary>
public static class SqlServerTransientExceptionDetector
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool ShouldRetryOn(Exception? ex)
    {
        if (ex is SqlException sqlException)
        {
            foreach (SqlError err in sqlException.Errors)
            {
                switch (err.Number)
                {
                    // SQL Error Code: 49920
                    // Cannot process request. Too many operations in progress for subscription "%ld".
                    // The service is busy processing multiple requests for this subscription.
                    // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for operation status.
                    // Wait until pending requests are complete or delete one of your pending requests and retry your request later.
                    case 49920:
                    // SQL Error Code: 49919
                    // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
                    // The service is busy processing multiple create or update requests for your subscription or server.
                    // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for pending operations.
                    // Wait till pending create or update requests are complete or delete one of your pending requests and
                    // retry your request later.
                    case 49919:
                    // SQL Error Code: 49918
                    // Cannot process request. Not enough resources to process request.
                    // The service is currently busy.Please retry the request later.
                    case 49918:
                    // SQL Error Code: 41839
                    // Transaction exceeded the maximum number of commit dependencies.
                    case 41839:
                    // SQL Error Code: 41325
                    // The current transaction failed to commit due to a serializable validation failure.
                    case 41325:
                    // SQL Error Code: 41305
                    // The current transaction failed to commit due to a repeatable read validation failure.
                    case 41305:
                    // SQL Error Code: 41302
                    // The current transaction attempted to update a record that has been updated since the transaction started.
                    case 41302:
                    // SQL Error Code: 41301
                    // Dependency failure: a dependency was taken on another transaction that later failed to commit.
                    case 41301:
                    // SQL Error Code: 40613
                    // Database XXXX on server YYYY is not currently available. Please retry the connection later.
                    // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
                    case 40613:
                    // SQL Error Code: 40501
                    // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
                    case 40501:
                    // SQL Error Code: 40197
                    // The service has encountered an error processing your request. Please try again.
                    case 40197:
                    // SQL Error Code: 20041
                    // Transaction rolled back. Could not execute trigger. Retry your transaction.
                    case 20041:
                    // SQL Error Code: 17197
                    // Login failed due to timeout; the connection has been closed. This error may indicate heavy server load.
                    // Reduce the load on the server and retry login.
                    case 17197:
                    // SQL Error Code: 14355
                    // The MSSQLServerADHelper service is busy. Retry this operation later.
                    case 14355:
                    // SQL Error Code: 10936
                    // Resource ID : %d. The request limit for the elastic pool is %d and has been reached.
                    // See 'http://go.microsoft.com/fwlink/?LinkId=267637' for assistance.
                    case 10936:
                    // SQL Error Code: 10929
                    // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
                    // However, the server is currently too busy to support requests greater than %d for this database.
                    // For more information, see http://go.microsoft.com/fwlink/?LinkId=267637. Otherwise, please try again.
                    case 10929:
                    // SQL Error Code: 10928
                    // Resource ID: %d. The %s limit for the database is %d and has been reached. For more information,
                    // see http://go.microsoft.com/fwlink/?LinkId=267637.
                    case 10928:
                    // SQL Error Code: 10922
                    // %ls failed. Rerun the statement.
                    case 10922:
                    // SQL Error Code: 10060
                    // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
                    // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
                    // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
                    // because the connected party did not properly respond after a period of time, or established connection failed
                    // because connected host has failed to respond.)"}
                    case 10060:
                    // SQL Error Code: 10054
                    // A transport-level error has occurred when sending the request to the server.
                    // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                    case 10054:
                    // SQL Error Code: 10053
                    // A transport-level error has occurred when receiving results from the server.
                    // An established connection was aborted by the software in your host machine.
                    case 10053:
                    // SQL Error Code: 9515
                    // An XML schema has been altered or dropped, and the query plan is no longer valid. Please rerun the query batch.
                    case 9515:
                    // SQL Error Code: 8651
                    // Could not perform the operation because the requested memory grant was not available in resource pool '%ls' (%ld).
                    // Rerun the query, reduce the query load, or check resource governor configuration setting.
                    case 8651:
                    // SQL Error Code: 8645
                    // A timeout occurred while waiting for memory resources to execute the query in resource pool '%ls' (%ld). Rerun the query.
                    case 8645:
                    // SQL Error Code: 8628
                    // A time out occurred while waiting to optimize the query. Rerun the query.
                    case 8628:
                    // SQL Error Code: 4221
                    // Login to read-secondary failed due to long wait on 'HADR_DATABASE_WAIT_FOR_TRANSITION_TO_VERSIONING'.
                    // The replica is not available for login because row versions are missing for transactions that were in-flight
                    // when the replica was recycled. The issue can be resolved by rolling back or committing the active transactions
                    // on the primary replica. Occurrences of this condition can be minimized by avoiding long write transactions on the primary.
                    case 4221:
                    // SQL Error Code: 4060
                    // Cannot open database "%.*ls" requested by the login. The login failed.
                    case 4060:
                    // SQL Error Code: 3966
                    // Transaction is rolled back when accessing version store. It was earlier marked as victim when the version store
                    // was shrunk due to insufficient space in tempdb. This transaction was marked as a victim earlier because it may need
                    // the row version(s) that have already been removed to make space in tempdb. Retry the transaction
                    case 3966:
                    // SQL Error Code: 3960
                    // Snapshot isolation transaction aborted due to update conflict. You cannot use snapshot isolation to access table '%.*ls'
                    // directly or indirectly in database '%.*ls' to update, delete, or insert the row that has been modified or deleted
                    // by another transaction. Retry the transaction or change the isolation level for the update/delete statement.
                    case 3960:
                    // SQL Error Code: 3935
                    // A FILESTREAM transaction context could not be initialized. This might be caused by a resource shortage. Retry the operation.
                    case 3935:
                    // SQL Error Code: 1807
                    // Could not obtain exclusive lock on database 'model'. Retry the operation later.
                    case 1807:
                    // SQL Error Code: 1221
                    // The Database Engine is attempting to release a group of locks that are not currently held by the transaction.
                    // Retry the transaction. If the problem persists, contact your support provider.
                    case 1221:
                    // SQL Error Code: 1205
                    // Deadlock
                    case 1205:
                    // SQL Error Code: 1204
                    // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement
                    // when there are fewer active users. Ask the database administrator to check the lock and memory configuration for
                    // this instance, or to check for long-running transactions.
                    case 1204:
                    // SQL Error Code: 1203
                    // Process ID %d attempted to unlock a resource it does not own: %.*ls. Retry the transaction, because this error
                    // may be caused by a timing condition. If the problem persists, contact the database administrator.
                    case 1203:
                    // SQL Error Code: 997
                    // A connection was successfully established with the server, but then an error occurred during the login process.
                    // (provider: Named Pipes Provider, error: 0 - Overlapped I/O operation is in progress)
                    case 997:
                    // SQL Error Code: 921
                    // Database '%.*ls' has not been recovered yet. Wait and try again.
                    case 921:
                    // SQL Error Code: 669
                    // The row object is inconsistent. Please rerun the query.
                    case 669:
                    // SQL Error Code: 617
                    // Descriptor for object ID %ld in database ID %d not found in the hash table during attempt to unhash it.
                    // A work table is missing an entry. Rerun the query. If a cursor is involved, close and reopen the cursor.
                    case 617:
                    // SQL Error Code: 601
                    // Could not continue scan with NOLOCK due to data movement.
                    case 601:
                    // SQL Error Code: 233
                    // The client was unable to establish a connection because of an error during connection initialization process before login.
                    // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server;
                    // the server was too busy to accept new connections; or there was a resource limitation (insufficient memory or maximum
                    // allowed connections) on the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by
                    // the remote host.)
                    case 233:
                    // SQL Error Code: 121
                    // The semaphore timeout period has expired
                    case 121:
                    // SQL Error Code: 64
                    // A connection was successfully established with the server, but then an error occurred during the login process.
                    // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
                    case 64:
                    // DBNETLIB Error Code: 20
                    // The instance of SQL Server you attempted to connect to does not support encryption.
                    case 20:
                        return true;
                    // This exception can be thrown even if the operation completed successfully, so it's safer to let the application fail.
                    // DBNETLIB Error Code: -2
                    // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated.
                    //case -2:
                }
            }

            return false;
        }

        return ex is TimeoutException;
    }
}
