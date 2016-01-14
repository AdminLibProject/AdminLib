using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Field {
    public class DateTimeField : Field<DateTime?> {

        /******************** Constructors ********************/
        public DateTimeField ( string      dbColumn
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
            return OracleDbType.Date;
        }

        public override object FromDbValue(object value) {
            return value;
        }

        public override string ToDbValue(object value) {
            return (string) value;
        }

    }
}