﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Database.Error {

    public class InvalidIdentifier : QueryException {

        //******************** Constants ********************/
        public new const Code code = Code.INVALID_IDENTIFIER;

        //******************** Constructors ********************/
        public InvalidIdentifier ( OracleException exception
                                 , string query=null
                                 , OracleParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}