using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class UniqueConstraintViolated : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.UNIQUE_CONSTRAINT_VIOLATED;

        //******************** Constructors ********************/
        public UniqueConstraintViolated(OracleException exception, string query=null, OracleParameter[] parameters=null) : base(exception, query, parameters) { }

    }
}