using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Field {
    public class TimestampField : DateTimeField {

        /******************** Constructors ********************/
        public TimestampField ( string      dbColumn
                              , string      apiName      = null
                              , string      apiGroup     = null
                              , DateTime?[] choices      = null
                              , DateTime?   defaultValue = null
                              , bool        primaryKey   = false
                              , bool?       nullable     = null
                              , bool?       unique       = null)

        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {
        }

        /******************** Methods ********************/
        public override OracleDbType GetDbType() {
            return OracleDbType.TimeStamp;
        }

    }
}