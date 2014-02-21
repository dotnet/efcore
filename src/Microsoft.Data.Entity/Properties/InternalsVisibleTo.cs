// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

#if !INTERNALS_INVISIBLE

[assembly: InternalsVisibleTo("Microsoft.Data.Entity.Tests")]
[assembly: InternalsVisibleTo("Microsoft.Data.SqlServer.Tests")]

// for Moq

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif
