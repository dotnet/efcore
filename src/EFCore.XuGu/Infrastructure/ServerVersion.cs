// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using System.Data.Common;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// The abstract base class of <see cref="XGServerVersion"/>.
    /// Contains static methods to create a <see cref="ServerVersion"/> from a string or to auto detect the server version from a database
    /// server.
    /// </summary>
    public abstract class ServerVersion
    {
        private static readonly Regex _versionRegex = new Regex(@"\d+\.\d+\.?(?:\d+)?");

        protected ServerVersion(Version version, ServerType type, string typeIdentifier = null)
        {
            Version = version;
            Type = type;
            TypeIdentifier = typeIdentifier ?? Enum.GetName(typeof(ServerType), type)
                .ToLowerInvariant();
        }

        public virtual Version Version { get; }
        public virtual ServerType Type { get; }
        public virtual string TypeIdentifier { get; }

        public abstract ServerVersionSupport Supports { get; }

        public virtual int MaxKeyLength => Supports.LargerKeyLength ? 3072 : 767;
        public virtual CharSet DefaultCharSet => Supports.DefaultCharSetUtf8Mb4 ? CharSet.Utf8Mb4 : CharSet.Latin1;
        public abstract string DefaultUtf8CsCollation { get; }
        public abstract string DefaultUtf8CiCollation { get; }

        public override bool Equals(object obj)
            => obj is ServerVersion version &&
               Equals(version);

        private bool Equals(ServerVersion other)
            => Version.Equals(other.Version) &&
               Type.Equals(other.Type) &&
               Equals(TypeIdentifier, other.TypeIdentifier);

        public override int GetHashCode()
            => HashCode.Combine(Version, Type, TypeIdentifier);

        /// <summary>
        /// Returns the server version and type in the format `major.minor.patch-type`.
        /// </summary>
        /// <returns>The server version and type string.</returns>
        public override string ToString()
            => $"{Version}-{TypeIdentifier}";

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// Uses a connection string to open a connection to the database server and then executes a command.
        /// The connection will ignore the database specified in the connection string. It therefore makes not difference, whether the
        /// database already exists or not.
        /// </remarks>
        public static ServerVersion AutoDetect(string connectionString)
        {
            using var connection = new XGConnection(
                new XGConnectionStringBuilder(connectionString)
                {
                    Database = string.Empty
                }.ConnectionString);
            connection.Open();
            return Parse(connection.ServerVersion);
        }

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous auto detect operation. The task result contains the <see cref="ServerVersion"/> of the database.
        /// </returns>
        /// <remarks>
        /// Uses a connection string to open a connection to the database server and then executes a command.
        /// The connection will ignore the database specified in the connection string. It therefore makes not difference, whether the
        /// database already exists or not.
        /// </remarks>
        public static async Task<ServerVersion> AutoDetectAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var connection = new XGConnection(
                new XGConnectionStringBuilder(connectionString)
                {
                    Database = string.Empty,
                }.ConnectionString);

            await using (connection.ConfigureAwait(false))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return Parse(connection.ServerVersion);
            }
        }

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// Uses a connection to the database server to execute a command.
        /// If the connection has already been opened, the connection is is being used as is. Otherwise, the connection is being cloned and
        /// ignores any database specified in the connection string of the connection. It therefore makes not difference, whether the
        /// database already exists or not, and the <see cref="ConnectionState"/> of the <paramref name="connection"/> parameter after the
        /// return of the call is the same as before the call.
        /// </remarks>
        public static ServerVersion AutoDetect(XGConnection connection)
        {
            string serverVersion;

            if (connection.State != ConnectionState.Open)
            {
                using var clonedConnection = connection.CloneWith(
                    new XGConnectionStringBuilder(connection.ConnectionString)
                    {
                        Database = string.Empty,
                    }.ConnectionString);
                clonedConnection.Open();
                serverVersion = clonedConnection.ServerVersion;
            }
            else
            {
                serverVersion = connection.ServerVersion;
            }

            return Parse(serverVersion);
        }

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous auto detect operation. The task result contains the <see cref="ServerVersion"/> of the database.
        /// </returns>
        /// <remarks>
        /// Uses a connection to the database server to execute a command.
        /// If the connection has already been opened, the connection is is being used as is. Otherwise, the connection is being cloned and
        /// ignores any database specified in the connection string of the connection. It therefore makes not difference, whether the
        /// database already exists or not, and the <see cref="ConnectionState"/> of the <paramref name="connection"/> parameter after the
        /// return of the call is the same as before the call.
        /// </remarks>
        public static async Task<ServerVersion> AutoDetectAsync(XGConnection connection, CancellationToken cancellationToken = default)
        {
            string serverVersion;

            if (connection.State != ConnectionState.Open)
            {
                var clonedConnection = connection.CloneWith(
                    new XGConnectionStringBuilder(connection.ConnectionString)
                    {
                        Database = string.Empty,
                    }.ConnectionString);

                await using (clonedConnection.ConfigureAwait(false))
                {
                    await clonedConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    serverVersion = clonedConnection.ServerVersion;
                }
            }
            else
            {
                serverVersion = connection.ServerVersion;
            }

            return Parse(serverVersion);
        }

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// Uses a <see cref="DbDataSource"/> that represents a database to execute a command.
        /// The data source is used to create a connection to the database server and ignores any database specified in the underlying
        /// connection string. It therefore makes not difference, whether a specified database already exists or not.
        /// </remarks>
        public static ServerVersion AutoDetect(DbDataSource dataSource)
        {
            using var connection = (XGConnection)dataSource.CreateConnection();
            connection.ConnectionString = new XGConnectionStringBuilder(connection.ConnectionString)
            {
                Database = string.Empty,
            }.ConnectionString;
            connection.Open();
            return Parse(connection.ServerVersion);
        }

        /// <summary>
        /// Retrieves the <see cref="ServerVersion"/> (version number and server type) from a database server.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous auto detect operation. The task result contains the <see cref="ServerVersion"/> of the database.
        /// </returns>
        /// <remarks>
        /// Uses a <see cref="DbDataSource"/> that represents a database to execute a command.
        /// The data source is used to create a connection to the database server and ignores any database specified in the underlying
        /// connection string. It therefore makes not difference, whether a specified database already exists or not.
        /// </remarks>
        public static async Task<ServerVersion> AutoDetectAsync(DbDataSource dataSource, CancellationToken cancellationToken = default)
        {
            var connection = (XGConnection)dataSource.CreateConnection();
            await using (connection.ConfigureAwait(false))
            {
                connection.ConnectionString = new XGConnectionStringBuilder(connection.ConnectionString)
                {
                    Database = string.Empty,
                }.ConnectionString;

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return Parse(connection.ServerVersion);
            }
        }

        /// <summary>
        /// Converts a string, containing the server version and type, into a <see cref="ServerVersion"/>.
        /// </summary>
        /// <param name="versionString">The server version (mandatory) and type (optional).</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// The general format is `major.minor.patch-type`, e.g. `8.0.21-mysql` or `10.5.3-mariadb`. If the type is being omitted, it is
        /// assumed to be MySQL (and not MariaDB).
        /// </remarks>
        public static ServerVersion Parse(string versionString)
            => Parse(versionString, null);

        /// <summary>
        /// Converts a string, containing the server version and type, into a <see cref="ServerVersion"/>.
        /// </summary>
        /// <param name="versionString">The server version (mandatory) and type (optional).</param>
        /// <param name="serverType">The <see cref="ServerType"/> or <see langword="null" />. </param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// The general format is `major.minor.patch-type`, e.g. `8.0.21-mysql` or `10.5.3-mariadb`. If the type is being omitted, it is
        /// assumed to be MySQL (and not MariaDB). The <paramref name="serverType"/> parameter takes precedence over a server type specified
        /// in the <paramref name="versionString"/> parameter, if not <see langword="null" />.
        /// </remarks>
        public static ServerVersion Parse(string versionString, ServerType? serverType)
        {
            Check.NotEmpty(versionString, nameof(versionString));

            if (!TryParse(versionString, serverType, out var serverVersion))
            {
                throw new InvalidOperationException($"Unable to determine server version from version string '${versionString}'.");
            }

            return serverVersion;
        }

        /// <summary>
        /// Tries to converts a string, containing the server version and type, into a <see cref="ServerVersion"/>.
        /// </summary>
        /// <param name="versionString">The server version (mandatory) and type (optional).</param>
        /// <param name="serverVersion">The <see cref="ServerVersion"/>.</param>
        /// <returns><see langword="true" /> if the conversion was successful, otherwise <see langword="false" />.</returns>
        /// <remarks>
        /// The general format is `major.minor.patch-type`, e.g. `8.0.21-mysql` or `10.5.3-mariadb`. If the type is being omitted, it is
        /// assumed to be MySQL (and not MariaDB).
        /// </remarks>
        public static bool TryParse(string versionString, out ServerVersion serverVersion)
            => TryParse(versionString, null, out serverVersion);

        /// <summary>
        /// Tries to converts a string, containing the server version and type, into a <see cref="ServerVersion"/>.
        /// </summary>
        /// <param name="versionString">The server version (mandatory) and type (optional).</param>
        /// <param name="serverType">The <see cref="ServerType"/> or <see langword="null" />. </param>
        /// <param name="serverVersion">The <see cref="ServerVersion"/>.</param>
        /// <returns><see langword="true" /> if the conversion was successful, otherwise <see langword="false" />.</returns>
        /// <remarks>
        /// The general format is `major.minor.patch-type`, e.g. `8.0.21-mysql` or `10.5.3-mariadb`. If the type is being omitted, it is
        /// assumed to be MySQL (and not MariaDB). The <paramref name="serverType"/> parameter takes precedence over a server type specified
        /// in the <paramref name="versionString"/> parameter, if not <see langword="null" />.
        /// </remarks>
        public static bool TryParse(string versionString, ServerType? serverType, out ServerVersion serverVersion)
        {
            Check.NotEmpty(versionString, nameof(versionString));

            serverVersion = null;

            var semanticVersion = _versionRegex.Matches(versionString);
            if (semanticVersion.Count > 0)
            {
                var type = serverType ?? ServerType.XG;

                var version = type == ServerType.MariaDb &&
                              semanticVersion.Count > 1
                    ? Version.Parse(semanticVersion[1].Value)
                    : Version.Parse(semanticVersion[0].Value);

                serverVersion = type switch
                {
                    ServerType.XG => new XGServerVersion(version),
                    _ => null
                };
            }

            return serverVersion != null;
        }

        /// <summary>
        /// Creates a <see cref="ServerVersion"/> object from a version and type.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> of the database server.</param>
        /// <param name="serverType">The <see cref="ServerType"/> of the database server.</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// Call this static method to obtain a <see cref="ServerVersion"/> object to use in a `UseXG()` call.
        /// Alternatively, directly instantiate an instance of the <see cref="XGServerVersion"/>
        /// classes using <see langword="new"/>, or call the static `Parse()`, `TryParse()` or `AutoDetect()` methods.
        /// </remarks>
        public static ServerVersion Create(Version version, ServerType serverType)
            => serverType switch
            {
                ServerType.XG => new XGServerVersion(version),
                _ => throw new ArgumentOutOfRangeException(nameof(serverType), serverType, null)
            };

        /// <summary>
        /// Creates a <see cref="ServerVersion"/> object from a version and type.
        /// </summary>
        /// <param name="major">The major version of the database server.</param>
        /// <param name="minor">The minor version of the database server.</param>
        /// <param name="patch">The patch level of the database server.</param>
        /// <param name="serverType">The <see cref="ServerType"/> of the database server.</param>
        /// <returns>The <see cref="ServerVersion"/>.</returns>
        /// <remarks>
        /// Call this static method to obtain a <see cref="ServerVersion"/> object to use in a `UseXG()` call.
        /// Alternatively, directly instantiate an instance of the <see cref="XGServerVersion"/>
        /// classes using <see langword="new"/>, or call the static `Parse()`, `TryParse()` or `AutoDetect()` methods.
        /// </remarks>
        public static ServerVersion Create(int major, int minor, int patch, ServerType serverType)
            => Create(new Version(major, minor, patch), serverType);
    }
}
