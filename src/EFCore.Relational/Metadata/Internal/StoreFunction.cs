// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class StoreFunction : TableBase, IStoreFunction
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public StoreFunction([NotNull] DbFunction dbFunction, [NotNull] RelationalModel model)
            : base(dbFunction.Name, dbFunction.Schema, model)
        {
            DbFunctions = new SortedDictionary<string, DbFunction> { { dbFunction.ModelName, dbFunction } };
            IsBuiltIn = dbFunction.IsBuiltIn;
            ReturnType = dbFunction.StoreType;

            Parameters = new StoreFunctionParameter[dbFunction.Parameters.Count];
            for (var i = 0; i < dbFunction.Parameters.Count; i++)
            {
                Parameters[i] = new StoreFunctionParameter(this, dbFunction.Parameters[i]);
            }

            dbFunction.StoreFunction = this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, DbFunction> DbFunctions { get; }

        /// <inheritdoc />
        public virtual bool IsBuiltIn { get; }

        /// <inheritdoc />
        public virtual string ReturnType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual StoreFunctionParameter[] Parameters { get; }

        /// <inheritdoc />
        public override IColumnBase FindColumn(IProperty property)
            => property.GetFunctionColumnMappings()
                .FirstOrDefault(cm => cm.TableMapping.Table == this)
                ?.Column;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IEnumerable<IFunctionMapping> IStoreFunction.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => EntityTypeMappings.Cast<IFunctionMapping>();
        }

        /// <inheritdoc />
        IEnumerable<IFunctionColumn> IStoreFunction.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values.Cast<IFunctionColumn>();
        }

        /// <inheritdoc />
        IEnumerable<IStoreFunctionParameter> IStoreFunction.Parameters
        {
            [DebuggerStepThrough]
            get => Parameters;
        }

        /// <inheritdoc />
        IEnumerable<IDbFunction> IStoreFunction.DbFunctions
        {
            [DebuggerStepThrough]
            get => DbFunctions.Values;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IFunctionColumn IStoreFunction.FindColumn(string name)
            => (IFunctionColumn)base.FindColumn(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IFunctionColumn IStoreFunction.FindColumn(IProperty property)
            => (IFunctionColumn)FindColumn(property);
    }
}
