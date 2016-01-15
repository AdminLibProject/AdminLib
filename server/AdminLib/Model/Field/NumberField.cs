using System;
using System.Data;

namespace AdminLib.Model.Field
{
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

        public override DbType GetDbType() {
            return DbType.Int32;
        }

    }
}