using Oracle.ManagedDataAccess.Client;
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
        public OracleException                  exception;

        /******************** Structure ********************/
        public struct SqlParameter {

            /***** Attributes *****/
            public string name;
            public object value;
            public string dbType;
            public string oracleDbType;
            public string direction;
            public bool   isNullable;
            public byte   precision;
            public byte   scale;
            public int    size;
                    
            /***** Constructors *****/
            public SqlParameter(OracleParameter parameter) {
                this.name         = parameter.ParameterName;
                this.dbType       = parameter.DbType.ToString();
                this.oracleDbType = parameter.OracleDbType.ToString();
                this.direction    = parameter.Direction.ToString();
                this.isNullable   = parameter.IsNullable;
                this.precision    = parameter.Precision;
                this.scale        = parameter.Scale;
                this.size         = parameter.Size;
                this.value        = parameter.Value;
            }

        }

        /******************** Constructors ********************/
        public SqlQuery(string query, OracleParameter[] parameters) {

            OracleParameter parameter;

            parameters      = parameters == null ? new OracleParameter[0] : parameters;
            this.query      = query;
            this.parameters = new Dictionary<string,SqlParameter>();
            this.timer      = new Timer();

            for (int p = 0; p < parameters.Length; p++) {
                parameter = parameters[p];

                this.parameters[parameter.ParameterName] = new SqlParameter(parameter);
            }

        }

        /******************** Methods ********************/
        public void end() {

            this.timer.stop();
        }

        public void raiseError(OracleException exception) {
            this.end();
            this.exception = exception;
        }

        public void start() {
            this.timer.start();
        }

    }
}