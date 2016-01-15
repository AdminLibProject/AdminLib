using AdminLib.Database;
using AdminLib.Database.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {
    public class SqlQuery {

        /******************** Attributes ********************/
        public string                           query;
        public Dictionary<string, SqlParameter> parameters;
        public Timer                            timer;
        public QueryException                   exception;

        /******************** Structure ********************/
        public struct SqlParameter {

            /***** Attributes *****/
            public string name;
            public object value;
            public string dbType;
            public string direction;
            public bool?  isNullable;
                    
            /***** Constructors *****/
            public SqlParameter(QueryParameter parameter) {
                this.name         = parameter.name;
                this.value        = parameter.value;
                this.dbType       = parameter.type.ToString();
                this.direction    = parameter.direction.ToString();
                this.isNullable   = parameter.nullable;
            }

        }

        /******************** Constructors ********************/
        public SqlQuery(string query, QueryParameter[] parameters) {

            QueryParameter parameter;

            parameters      = parameters == null ? new QueryParameter[0] : parameters;
            this.query      = query;
            this.parameters = new Dictionary<string,SqlParameter>();
            this.timer      = new Timer();

            for (int p = 0; p < parameters.Length; p++) {
                parameter = parameters[p];

                this.parameters[parameter.name] = new SqlParameter(parameter);
            }

        }

        /******************** Methods ********************/
        public void end() {

            this.timer.stop();
        }

        public void raiseError(QueryException exception) {
            this.end();
            this.exception = exception;
        }

        public void start() {
            this.timer.start();
        }

    }
}