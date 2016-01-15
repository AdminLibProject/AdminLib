using Oracle.ManagedDataAccess.Client;
using AdminLib.Database.Error;
using AdminLib.Database;

namespace AdminLib.Data.Adapter.Oracle.Exception {

    public class SuccessWithCompilationError : QueryException {

        //******************** Constants ********************/
        public const Code code = Code.SUCCESS_WITH_COMPILATION_ERROR;

        //******************** Constructors ********************/
        public SuccessWithCompilationError ( OracleException exception
                                           , string query=null
                                           , QueryParameter[] parameters=null) : base(exception, query, parameters) { }

    }
}