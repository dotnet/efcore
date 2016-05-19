// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeDbParameter : DbParameter
    {
        public override string ParameterName { get; set; }

        public override object Value { get; set; }

        public override ParameterDirection Direction { get; set; }

        public static bool DefaultIsNullable = false;
        public override bool IsNullable { get; set; } = DefaultIsNullable;

        public static DbType DefaultDbType = DbType.AnsiString;
        public override DbType DbType { get; set; } = DefaultDbType;

        public override int Size { get; set; }

        public override string SourceColumn
        {
            get { throw new NotImplementedException(); }

            set { throw new NotImplementedException(); }
        }

        public override bool SourceColumnNullMapping
        {
            get { throw new NotImplementedException(); }

            set { throw new NotImplementedException(); }
        }

#if NET451
        public override DataRowVersion SourceVersion
        {
            get { throw new NotImplementedException(); }

            set { throw new NotImplementedException(); }
        }
#endif

        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }
    }
}
