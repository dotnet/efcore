using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalDbFunctionSourceFactory : IDbFunctionSourceFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDbFunctionSourceFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalDbFunctionSourceFactory([NotNull] RelationalDbFunctionSourceFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual RelationalDbFunctionSourceFactoryDependencies Dependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression GenerateDbFunctionSource(MethodCallExpression methodCall, IModel model)
            => new DbFunctionSourceExpression(
                        Check.NotNull(methodCall, nameof(methodCall)),
                        Check.NotNull(model, nameof(model)));
    }
}
