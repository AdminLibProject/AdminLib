using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public class BooleanField : Field<bool?> {

        /******************** Constructors ********************/
        public BooleanField ( string  dbColumn
                            , string  apiName      = null
                            , string  apiGroup     = null
                            , bool?[] choices      = null
                            , bool?   defaultValue = null
                            , bool    primaryKey   = false
                            , bool?   nullable     = null
                            , bool?   unique       = null)
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
        public override object FromDbValue(object value) {

            int intValue;

            if (value is DBNull)
                return null;

            intValue = Convert.ToInt32(value);

            return intValue != 0;
        }

        public override string ToDbValue(object value) {

            if (value is bool)
                return ((bool) value) ? "1" : "0";

            if (value == null)
                return "0";

            return "1";
        }

        public override OracleDbType GetDbType() {
            return OracleDbType.Int32;
        }

    }
}