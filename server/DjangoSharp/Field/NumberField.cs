using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public class NumberField : Field<decimal?> {

        /******************** Constructors ********************/
        public NumberField ( string    dbColumn
                           , string    apiName      = null
                           , string    apiGroup     = null
                           , decimal?[] choices      = null
                           , decimal?   defaultValue = null
                           , bool      primaryKey   = false
                           , bool?     nullable     = null
                           , bool?     unique       = null)
        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {}

        /******************** Methods ********************/
        public override object FromDbValue(object value) {

            decimal intValue;

            if (value is DBNull)
                return null;

            intValue = Convert.ToDecimal(value);

            return intValue;
        }

        public override OracleDbType GetDbType() {
            return OracleDbType.Int32;
        }

    }
}