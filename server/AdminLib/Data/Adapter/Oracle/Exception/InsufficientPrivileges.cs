using Oracle.ManagedDataAccess.Client;
using AdminLib.Database.Error;
using AdminLib.Database;

namespace AdminLib.Data.Adapter.Oracle.Exception {

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