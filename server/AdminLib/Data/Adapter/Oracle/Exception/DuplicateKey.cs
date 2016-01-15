using Oracle.ManagedDataAccess.Client;
using AdminLib.Database.Error;
using AdminLib.Database;

namespace AdminLib.Data.Adapter.Oracle.Exception {

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