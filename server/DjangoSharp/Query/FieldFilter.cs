using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DjangoSharp.Field;
using DjangoSharp.Model;
using DjangoSharp;

namespace DjangoSharp.Query {
    public class FieldFilter {

        /******************** Attributes ********************/
        public string apiPath {
            get {
                return this.path.pathString;
            }
        }
        private bool           array;
        public  BaseField      field      { get; private set; }
        public  Path      path       { get; private set; }
        public  FilterOperator type       { get; private set; }
        private string[]       values;
        public  AStructure      rootModel { get; private set; }

        /******************** Constructors ********************/
        // String value, with type
        /// <summary>
        ///     Create a new filter who take a single string value
        /// </summary>
        /// <param name="rootModel">Model from wich begin the path string</param>
        /// <param name="path">Path of the field in the model</param>
        /// <param name="type">Type of operation</param>
        /// <param name="value">Value use to filter</param>
        public FieldFilter( AStructure rootModel, string path, FilterOperator type, string value) {
            this.Initialize ( rootModel : rootModel
                            , path      : path
                            , type      : type
                            , values    : new string[1] {value}
                            , isArray   : false);
        }

        // String values
        public FieldFilter( AStructure rootModel, string path, FilterOperator type, string[] value) {

            this.Initialize ( rootModel : rootModel
                            , path      : path
                            , type      : type
                            , values    : value
                            , isArray   : true);
        }

        // Int value
        public FieldFilter( AStructure rootModel, string path, FilterOperator type, int value) {

            this.Initialize ( rootModel : rootModel
                            , path      : path
                            , type      : type
                            , values    : new string[1] {value.ToString()}
                            , isArray   : false);
        }

        // Int values
        public FieldFilter ( AStructure rootModel, string path, FilterOperator type, int[] value) {

            string[] values;

            values     = new string[value.Length];

            for (int i = 0; i < values.Length; i++) {
                values[i] = values[i].ToString();
            }

            this.Initialize ( rootModel : rootModel
                            , path      : path
                            , type      : type
                            , values    : values
                            , isArray   : true);
        }

        /******************** Methods ********************/

        public FieldFilter Clone(BaseField from) {

            FieldFilter filter;
            Path   path;

            path = this.path.Clone(from: from);

            filter = new FieldFilter ( rootModel : path.rootModel
                                     , path      : path.pathString
                                     , type      : this.type
                                     , value     : this.values);

            return filter;
        }

        private void Initialize ( AStructure     rootModel
                                , string         path
                                , FilterOperator type
                                , string[]       values
                                , bool           isArray) 
        {

            this.array     = isArray;
            this.rootModel = rootModel;
            this.type      = type;

            this.path      = new Path ( rootModel : this.rootModel
                                           , path      : path);

            if (this.path.field == null)
                throw new Exception("No field found");

            this.field     = this.path.field;

            this.values = new string[values.Length];

            for(int v=0; v < values.Length; v++) {
                this.values[v] = this.field.ToDbValue(values[v]);
            }
        }

        /// <summary>
        ///     Return the list of values used for filter
        /// </summary>
        /// <returns></returns>
        public string[] GetValues() {
            return this.values;
        }

        /// <summary>
        ///     Indicate if the value of the filter is an array or not
        /// </summary>
        /// <returns>
        ///     . True : The attribute is filtered by an array or value
        ///     . False: The attribute is filtered by a simple value
        /// </returns>
        public bool isArray() {
            return this.array;
        }

        /// <summary>
        ///     Convert the filter to a SQL filter (in the "where" condition)
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public string toSQL(string tableAlias, List<OracleParameter> parameters, string parameterName=null) {

            return this.type.toSQL ( column        : tableAlias + '.' + this.field.dbColumn
                                   , dbType        : this.field.GetDbType()
                                   , values        : this.values
                                   , parameters    : parameters
                                   , parameterName : parameterName);

        }

        public override string ToString() {
            return "<FieldFilter: {" + this.rootModel.ApiName + "} " + this.path.pathString + ':' + this.type.ToString() + ">";
        }
    }
}