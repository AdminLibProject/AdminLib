using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace AdminLib.Data.Query {
    public class QueryParameter {

        /******************** Attributes ********************/
        public string             name;
        public object             value;
        public DbType?            type;
        public ParameterDirection direction;
        public bool?              nullable;

        /******************** Constructors ********************/
        public QueryParameter ( string             name
                              , ParameterDirection direction = ParameterDirection.Input
                              , bool ?             nullable  = null
                              , DbType?            type      = null
                              , object             value     = null) {

            this.direction = direction;
            this.name      = name;
            this.nullable  = nullable;
            this.type      = type;
            this.value     = value;
        }

    }
}