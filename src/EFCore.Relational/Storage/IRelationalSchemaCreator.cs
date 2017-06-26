using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage
{
	public interface IRelationalSchemaCreator : ISchemaCreator
	{
		/// <summary>
		///     Determines whether the schema exists.
		/// </summary>
		/// <returns>
		///     True if the schema exists; otherwise false.
		/// </returns>
		bool Exists();

		/// <summary>
		///     Asynchronously determines whether the schema exists. 
		/// </summary>
		/// <param name="cancellationToken">
		///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous operation. The task result contains
		///     true if the schema exists; otherwise false.
		/// </returns>
		Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		///     Creates the schema.
		/// </summary>
		void Create();

		/// <summary>
		///     Asynchronously creates the schema.
		/// </summary>
		/// <param name="cancellationToken">
		///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		/// </returns>
		Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		///     Deletes the schema.
		/// </summary>
		void Delete();

		/// <summary>
		///     Asynchronously deletes the schema.
		/// </summary>
		/// <param name="cancellationToken">
		///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
		/// </param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		/// </returns>
		Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

	}
}
