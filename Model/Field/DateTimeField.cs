using System;
using System.Data;

namespace AdminLib.Model.Field
{
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
        public override DbType GetDbType() {
            return DbType.Date;
        }

        public override object FromDbValue(object value) {
            return value;
        }

        public override string ToDbValue(object value) {
            return (string) value;
        }

    }
}