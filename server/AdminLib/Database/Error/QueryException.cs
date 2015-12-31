using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Data;

namespace AdminLib.Database.Error {

    public class QueryException : Exception {

        //******************** Attribute ********************/
        public OracleException        exception;

        public Code?        code      {get; private set; }
        public string      query      {get; private set; }
        public Parameter[] parameters {get; private set; }

        //******************** Classes & structures ********************/
        public class Parameter {

            public string name;
            public object value;
            public string dbType;
            public string oracleDbType;

            public Parameter(OracleParameter parameter) {
                this.name         = parameter.ParameterName;
                this.value        = parameter.Value;
                this.dbType       = parameter.DbType.ToString();
                this.oracleDbType = parameter.OracleDbType.ToString();
            }

        }

        //******************** Constructors ********************/

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="exception"></param>
        public QueryException(OracleException exception, string query, OracleParameter[] parameters) : base() {
            List<Parameter> queryParameters;

            this.exception = exception;
            this.query = query;

            queryParameters = new List<Parameter>();

            parameters = parameters ?? new OracleParameter[0];

            foreach(OracleParameter parameter in parameters) {
                queryParameters.Add(new Parameter(parameter));
            }

            this.parameters = queryParameters.ToArray();
        }

        //******************** Methods ********************/
        public OracleException getException() {
            return this.exception;
        }

        //******************** Static methods ********************/
        public static QueryException get(OracleException exception, string query, OracleParameter[] parameters) {

            Code code;

            try {
                code = (Code) exception.Number;
            }
            catch {
                return new QueryException ( exception  : exception
                                          , query      : query
                                          , parameters : parameters);
            }

            switch (code) {

                case Code.DEPENDENCY_ERROR:
                    return new DependencyError ( exception  : exception
                                               , query      : query
                                               , parameters : parameters);

                case Code.DISABLED_ACCOUNT:
                    return new DisabledAccount ( exception  : exception
                                               , query      : query
                                               , parameters : parameters);

                case Code.DUPLICATE_KEY:
                    return new DuplicateKey ( exception  : exception
                                            , query      : query
                                            , parameters : parameters);

                case Code.INVALID_DATA:
                    return new InvalidData ( exception  : exception
                                           , query      : query
                                           , parameters : parameters);

                case Code.INVALID_ID:
                    return new InvalidID ( exception  : exception
                                         , query      : query
                                         , parameters : parameters);

                case Code.INVALID_PASSWORD:
                    return new InvalidPassword ( exception  : exception
                                               , query      : query
                                               , parameters : parameters);

                case Code.SESSION_DONT_EXISTS:
                    return new SessionDontExists ( exception  : exception
                                                 , query      : query
                                                 , parameters : parameters);

                // Standard errors
                case Code.UNIQUE_CONSTRAINT_VIOLATED:
                    return new UniqueConstraintViolated ( exception  : exception
                                                        , query      : query
                                                        , parameters : parameters);
                case Code.INSUFFICIENT_PRIVILEGES:
                    return new InsufficientPrivileges ( exception  : exception
                                                      , query      : query
                                                      , parameters : parameters);

                case Code.INVALID_IDENTIFIER:
                    return new InvalidIdentifier ( exception  : exception
                                                 , query      : query
                                                 , parameters : parameters);

                case Code.SUCCESS_WITH_COMPILATION_ERROR:
                    return new SuccessWithCompilationError ( exception  : exception
                                                           , query      : query
                                                           , parameters : parameters);

                default:
                    return new QueryException ( exception  : exception
                                              , query      : query
                                              , parameters : parameters);
            }
        }
    }
}