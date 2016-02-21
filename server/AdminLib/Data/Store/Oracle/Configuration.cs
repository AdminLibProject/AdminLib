using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;

namespace AdminLib.Data.Store.Oracle {

    public class Configuration: AdapterConfiguration {

        /******************** Attributes ********************/
        internal string connectionString;

        /******************** Constructors ********************/
        public Configuration(XPathNavigator storeConfiguration) : base (storeConfiguration) {
            this.connectionString = name;
        }

    }
}