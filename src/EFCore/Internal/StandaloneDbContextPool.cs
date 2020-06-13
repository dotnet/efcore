// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// using JetBrains.Annotations;
//
// namespace Microsoft.EntityFrameworkCore.Internal
// {
//     /// <summary>
//     ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//     ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//     ///     any release. You should only use it directly in your code with extreme caution and knowing that
//     ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//     /// </summary>
//     public class StandaloneDbContextPool<TContext> : DbContextPool<TContext>
//         where TContext : DbContext
//     {
//         /// <summary>
//         ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//         ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//         ///     any release. You should only use it directly in your code with extreme caution and knowing that
//         ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//         /// </summary>
//         public StandaloneDbContextPool([NotNull] DbContextOptions<TContext> options)
//             : base(options)
//         {
//         }
//
//         /// <summary>
//         ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//         ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//         ///     any release. You should only use it directly in your code with extreme caution and knowing that
//         ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//         /// </summary>
//         public override void ContextDisposed(DbContext context)
//             => Return(context);
//     }
// }
