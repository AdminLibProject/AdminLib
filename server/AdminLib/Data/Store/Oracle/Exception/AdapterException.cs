using Oracle.ManagedDataAccess.Client;
using AdminLib.Data.Query;
using AdminLib.Data.Query.Exception;
using System.Data.Common;

namespace AdminLib.Data.Store.Oracle.Exception
{

    public class AdapterException : QueryException    {

        public AdapterException ( DbException      exception
                                , string           query
                                , QueryParameter[] parameters) : base(exception, query, parameters) {

        }

        //******************** Static methods ********************/
        public static QueryException Get(OracleException exception, string query, QueryParameter[] parameters) {

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
                    return new App.QueryException.DisabledAccount ( exception  : exception
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
                    return new App.QueryException.InvalidPassword ( exception  : exception
                                                                  , query      : query
                                                                  , parameters : parameters);

                case Code.SESSION_DONT_EXISTS:
                    return new App.QueryException.SessionDontExists ( exception  : exception
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
                    return new App.QueryException.InvalidIdentifier ( exception  : exception
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