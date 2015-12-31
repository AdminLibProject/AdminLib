using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class DependencyError : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.DEPENDENCY_ERROR;

        //******************** Constructors ********************/
        public DependencyError(OracleException exception, string query=null, OracleParameter[] parameters=null) : base(exception, query, parameters) { }

    }
}