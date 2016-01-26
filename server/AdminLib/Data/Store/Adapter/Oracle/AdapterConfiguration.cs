using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Data.Store.Adapter.Oracle {
    public class AdapterConfiguration: AdminLib.Data.Store.Adapter.IAdapterConfiguration {

        internal string connectionString;

        /******************** Constructors ********************/
        public AdapterConfiguration(string connectionString) {
            this.connectionString = connectionString;
        }

    }
}