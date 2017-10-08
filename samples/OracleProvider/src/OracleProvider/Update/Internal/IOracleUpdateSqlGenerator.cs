// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic; 
using System.Text; 

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
	public interface IOracleUpdateSqlGenerator : IUpdateSqlGenerator
	{
		ResultSetMapping AppendBulkInsertOperation(
			StringBuilder commandStringBuilder,
			StringBuilder variablesCommand,
			IReadOnlyList<ModificationCommand> modificationCommands,
			int commandPosition,
			ref int cursorPosition);
	}
}
