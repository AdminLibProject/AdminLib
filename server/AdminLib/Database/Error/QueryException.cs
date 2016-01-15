using System;
using System.Collections.Generic;
using System.Data.Common;

namespace AdminLib.Database.Error
{

    public class QueryException : Exception {

        //******************** Attribute ********************/
        public DbException exception;

        public string      query      {get; private set; }
        public Parameter[] parameters {get; private set; }

        //******************** Classes & structures ********************/
        public class Parameter {

            public string name;
            public object value;
            public string dbType;

            public Parameter(QueryParameter parameter) {
                this.name         = parameter.name;
                this.value        = parameter.value;
                this.dbType       = parameter.type.ToString();
            }

        }

        //******************** Constructors ********************/

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="exception"></param>
        public QueryException(DbException exception, string query, QueryParameter[] parameters) : base() {
            List<Parameter> queryParameters;

            this.exception = exception;
            this.query = query;

            queryParameters = new List<Parameter>();

            parameters = parameters ?? new QueryParameter[0];

            foreach(QueryParameter parameter in parameters) {
                queryParameters.Add(new Parameter(parameter));
            }

            this.parameters = queryParameters.ToArray();
        }

        //******************** Methods ********************/
        public DbException getException() {
            return this.exception;
        }
    }
}