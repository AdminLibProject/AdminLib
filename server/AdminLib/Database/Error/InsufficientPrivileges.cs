using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error
{
    public class InsufficientPrivileges : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.INSUFFICIENT_PRIVILEGES;

        //******************** Constructors ********************/
        public InsufficientPrivileges ( OracleException exception
                                      , string query=null
                                      , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}