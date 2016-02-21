using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using store=AdminLib.Data.Store;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using AdminLib.Data.Store;

namespace AdminLib.Server {
    internal static class Configuration {

        /******************** Static Constructors ********************/
        static Configuration(){

            XPathNavigator nav;
            XPathDocument  serverConfig;
            
            serverConfig = new XPathDocument("server.config");
            nav          = serverConfig.CreateNavigator();

            foreach(XPathNavigator storeConfiguration in nav.Select("server/stores/store")) {
                AdapterConfiguration.RegisterNewConfiguration(storeConfiguration);
            }
        }

    }
}