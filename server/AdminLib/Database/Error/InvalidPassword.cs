using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class InvalidPassword : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.INVALID_PASSWORD;

        //******************** Constructors ********************/
        public InvalidPassword ( OracleException exception
                               , string query=null
                               , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}