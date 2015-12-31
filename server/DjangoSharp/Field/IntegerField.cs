using System;
using DjangoSharp.Model;
using Oracle.ManagedDataAccess.Client;

namespace DjangoSharp.Field {
    public class IntegerField : Field<int?> {

        /******************** Static Attributes ********************/
        public const OracleDbType dbType = OracleDbType.Int32;

        /******************** Attributes ********************/
        public    string sequence { get; private set; }
        public    bool   isEnum   { get; private set; }
        protected Type   enumType { get; private set; }

        /******************** Constructors ********************/
        public IntegerField ( string dbColumn
                            , string apiName      = null
                            , string apiGroup     = null
                            , int?[] choices      = null
                            , int?   defaultValue = null
                            , bool   primaryKey   = false
                            , bool?  nullable     = null
                            , string sequence     = null
                            , bool?  unique       = null)
        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : unique)
        {
        
            this.sequence = sequence;
        
        }

        /******************** Methods ********************/

        public override void Initialize(AStructure model) {

            bool isValidInteger;

            base.Initialize(model);

            // Checking that the field is nullable;
            if (Nullable.GetUnderlyingType(this.AttributeType) == null)
                throw new Exception("The field " + this + " is not nullable");

            this.isEnum = Nullable.GetUnderlyingType(this.AttributeType).IsEnum;

            if (this.isEnum)
                this.enumType = Nullable.GetUnderlyingType(this.AttributeType);

            isValidInteger =    this.AttributeType == typeof(Int16?) 
                             || this.AttributeType == typeof(Int32?) 
                             || this.AttributeType == typeof(Int64?)
                             || this.isEnum;

            if (!isValidInteger)
                throw new Exception("The field " + this + " is not an integer nor an enum");

        }

        public override object FromDbValue(object value) {

            if (value is DBNull)
                return null;

            if (this.isEnum) {
                if (value is decimal)
                    value = (int) ((decimal) value);

                return Enum.ToObject(this.enumType, value);
            }
            else
                return Convert.ToInt32(value);
        }

        public override OracleDbType GetDbType() {
            return IntegerField.dbType;
        }

        public override string ToDbValue(object value) {
            
            if (this.isEnum)
                return ((int) value).ToString();
            else
                return value.ToString();

        }

    }
}