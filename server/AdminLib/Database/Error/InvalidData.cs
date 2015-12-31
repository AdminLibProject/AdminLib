using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error
{
    public class InvalidData : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.INVALID_DATA;

        //******************** Constructors ********************/
        public InvalidData ( OracleException exception
                           , string query=null
                           , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}