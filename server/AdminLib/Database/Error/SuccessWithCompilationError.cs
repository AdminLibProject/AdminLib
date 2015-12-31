using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class SuccessWithCompilationError : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.SUCCESS_WITH_COMPILATION_ERROR;

        //******************** Constructors ********************/
        public SuccessWithCompilationError(OracleException exception, string query=null, OracleParameter[] parameters=null) : base(exception, query, parameters) { }

    }
}