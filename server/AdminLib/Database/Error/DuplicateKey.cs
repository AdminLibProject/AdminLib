using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class DuplicateKey : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.DUPLICATE_KEY;

        //******************** Constructors ********************/
        public DuplicateKey ( OracleException exception
                            , string query=null
                            , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }
    }
}