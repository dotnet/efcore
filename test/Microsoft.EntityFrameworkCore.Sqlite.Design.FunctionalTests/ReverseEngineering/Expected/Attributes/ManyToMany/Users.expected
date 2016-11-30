using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class Users
    {
        public Users()
        {
            UsersGroups = new HashSet<UsersGroups>();
        }

        public string Id { get; set; }

        [InverseProperty("User")]
        public ICollection<UsersGroups> UsersGroups { get; set; }
    }
}
