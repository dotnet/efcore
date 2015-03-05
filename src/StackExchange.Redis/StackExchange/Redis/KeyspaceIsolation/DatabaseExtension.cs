using System;

namespace StackExchange.Redis.KeyspaceIsolation
{
    /// <summary>
    ///     Provides the <see cref="WithKeyPrefix"/> extension method to <see cref="IDatabase"/>.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        ///     Creates a new <see cref="IDatabase"/> instance that provides an isolated key space
        ///     of the specified underyling database instance.
        /// </summary>
        /// <param name="database">
        ///     The underlying database instance that the returned instance shall use.
        /// </param>
        /// <param name="keyPrefix">
        ///     The prefix that defines a key space isolation for the returned database instance.
        /// </param>
        /// <returns>
        ///     A new <see cref="IDatabase"/> instance that invokes the specified underlying
        ///     <paramref name="database"/> but prepends the specified <paramref name="keyPrefix"/>
        ///     to all key paramters and thus forms a logical key space isolation.
        /// </returns>
        /// <remarks>
        /// <para>
        ///     The following methods are not supported in a key space isolated database and
        ///     will throw an <see cref="NotSupportedException"/> when invoked:
        /// </para>    
        /// <list type="bullet">
        ///     <item><see cref="IDatabaseAsync.KeyRandomAsync(CommandFlags)"/></item>
        ///     <item><see cref="IDatabase.KeyRandom(CommandFlags)"/></item>
        /// </list>
        /// <para>
        ///     Please notice that keys passed to a script are prefixed (as normal) but care must
        ///     be taken when a script returns the name of a key as that will (currently) not be
        ///     "unprefixed".
        /// </para>
        /// </remarks>
        public static IDatabase WithKeyPrefix(this IDatabase database, RedisKey keyPrefix)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (keyPrefix.IsNull)
            {
                throw new ArgumentNullException("keyPrefix");
            }

            if (keyPrefix.IsEmpty)
            {
                return database; // fine - you can keep using the original, then
            }

            if(database is DatabaseWrapper)
            {
                // combine the key in advance to minimize indirection
                var wrapper = (DatabaseWrapper)database;
                keyPrefix = wrapper.ToInner(keyPrefix);
                database = wrapper.Inner;
            }

            return new DatabaseWrapper(database, keyPrefix.AsPrefix());
        }
    }
}
