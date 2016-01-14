using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Field {
    public class CharField : Field<string> {

        /******************** Attributes ********************/

        public int?   max_length { get; private set; }
        public const OracleDbType dbType = OracleDbType.Varchar2;

        /******************** Constructors ********************/
        public CharField ( string   dbColumn
                         , string   apiName      = null
                         , string   apiGroup     = null
                         , string[] choices      = null
                         , string   defaultValue = null
                         , int?     max_length   = null
                         , bool     primaryKey   = false
                         , bool?    nullable     = null
                         , bool?    unique       = null)

        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {
            this.max_length = max_length;
        }

        /******************** Methods ********************/

        public override void Initialize(Model.AStructure model) {
            base.Initialize(model);

            if (this.AttributeType != typeof(string))
                throw new Exception("The field " + this + " is not a string");

        }
        
        public override void ValidateValue(string value) {

            base.ValidateValue(value);

            // TODO : It would be very nice that CharField could also use enum
            if (value.Length < max_length)
                throw new InvalidValue("The string is longer than \"max_length\"");

        }

        public override OracleDbType GetDbType() {
            return CharField.dbType;
        }
    }
}