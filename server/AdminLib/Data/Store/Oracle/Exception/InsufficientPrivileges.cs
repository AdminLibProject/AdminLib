using Oracle.ManagedDataAccess.Client;
using AdminLib.Data.Query.Exception;
using AdminLib.Data.Query;

namespace AdminLib.Data.Store.Oracle.Exception {

    public class InsufficientPrivileges : QueryException {

        //******************** Constants ********************/
        public const Code code = Code.INSUFFICIENT_PRIVILEGES;

        //******************** Constructors ********************/
        public InsufficientPrivileges ( OracleException exception
                                      , string query=null
                                      , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}