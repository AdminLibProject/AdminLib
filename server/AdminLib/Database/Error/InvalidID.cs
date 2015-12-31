using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    
    public class InvalidID : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.INVALID_ID;

        //******************** Constructors ********************/
        public InvalidID ( OracleException exception
                         , string query=null
                         , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }

}