using Oracle.ManagedDataAccess.Client;
using AdminLib.Data.Query.Exception;
using AdminLib.Data.Query;

namespace AdminLib.Data.Store.Oracle.Exception {

    public class DuplicateKey : QueryException {

        //******************** Constants ********************/
        public const Code code = Code.DUPLICATE_KEY;

        //******************** Constructors ********************/
        public DuplicateKey ( OracleException exception
                            , string query=null
                            , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }
    }
}