using Oracle.ManagedDataAccess.Client;
using AdminLib.Database.Error;
using AdminLib.Database;

namespace AdminLib.Data.Adapter.Oracle.Exception {

    public class DependencyError : AdapterException {

        //******************** Constants ********************/
        public const Code code = Code.DEPENDENCY_ERROR;

        //******************** Constructors ********************/
        public DependencyError ( OracleException exception
                               , string query=null
                               , QueryParameter[] parameters=null) : base(exception, query, parameters) { }

    }
}