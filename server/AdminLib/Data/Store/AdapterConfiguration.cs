using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;

namespace AdminLib.Data.Store {
    public abstract class AdapterConfiguration {
        
        /******************** Static Attributes ********************/
        internal static Dictionary<string, AdapterConfiguration> configurations = new Dictionary<string, AdapterConfiguration>();

        /******************** Attributes & Fields ********************/
        public   string           name           {get; }

        /// <summary>
        ///     Object that will create a new adapter.
        /// </summary>
        internal Adapter.ICreator adapterCreator;

        /// <summary>
        /// Indicate if the autocommit
        /// </summary>
        public virtual bool autoCommit {
            get {
                return false;
            }
        }

        /******************** Classes & Structures ********************/
        [AttributeUsage ( AttributeTargets.Class
                        , AllowMultiple = false )]
        public class Declaration : Attribute {

            /***** Attributes *****/
            private string name;

            /***** Constructors *****/
            public Declaration(string adapter) {
                this.name = adapter;
            }

        }

        /******************** Constructors ********************/
        internal AdapterConfiguration ( XPathNavigator storeConfiguration) {

            string               adapter;
            string               name;

            name    = storeConfiguration.GetAttribute("name"   , "");
            adapter = storeConfiguration.GetAttribute("adapter", "");

            if (AdapterConfiguration.GetAdapterConfiguration(name) != null)
                throw new Exception("An adapter manager \"" + name + "\" already exists");

            this.name = name;

            AdapterConfiguration.configurations[name] = this;
        }

        /******************** Static methods ********************/

        /// <summary>
        ///     Return the adapter configuration corresponding to the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AdapterConfiguration GetAdapterConfiguration(string name) {
            if (!AdapterConfiguration.configurations.ContainsKey(name))
                return null;

            return AdapterConfiguration.configurations[name];
        }

        public static void RegisterNewConfiguration(XPathNavigator storeConfiguration) {
            string               adapter;
            AdapterConfiguration adapterConfiguration;
            string               name;

            name    = storeConfiguration.GetAttribute("name"   , "");
            adapter = storeConfiguration.GetAttribute("adapter", "");

            if (AdapterConfiguration.GetAdapterConfiguration(name) != null)
                throw new Exception("An adapter manager \"" + name + "\" already exists");

            //AdapterConfiguration.configurations[name] = adapterConfiguration;
        }

    }
}