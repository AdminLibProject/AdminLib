using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminLib.Data.Store;

namespace AdminLib.Data.Store.Oracle {
    public class Creator : Adapter.ICreator {

        public string name {get; private set; }

        /******************** Constructor ********************/
        internal Creator(string name) {
            this.name = name;
        }

        /******************** Method ********************/
        public Store.Adapter GetNewAdapter(Configuration configuration) {
            Adapter oracleAdapter;

            oracleAdapter = new Adapter ( configuration : configuration);

            return oracleAdapter;
        }

        public Store.Adapter GetNewAdapter(AdapterConfiguration configuration, bool autoCommit)
        {
            
            if (!(configuration is Configuration))
                throw new System.Exception("Invalid configuration");

            return this.GetNewAdapter ( configuration : (Configuration) configuration
                                      , autoCommit    : autoCommit);

        }
    }
}