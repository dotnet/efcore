using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cud.Model
{
    public partial class SpecialProduct : Product
    {
        #region Primitive Properties
    
        public virtual string Style
        {
            get;
            set;
        }

        #endregion
    }
}
