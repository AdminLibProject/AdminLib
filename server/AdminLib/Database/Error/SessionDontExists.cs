using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace AdminLib.Database.Error {
    public class SessionDontExists : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.DISABLED_ACCOUNT;

        //******************** Constructors ********************/
        public SessionDontExists ( OracleException exception
                                 , string query=null
                                 , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}