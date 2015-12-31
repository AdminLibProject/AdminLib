using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public abstract class Field<T> : BaseField {

        /******************** Attributes ********************/
        public T      defaultValue { get; private set; }
        public T      value        { get; private set; }
        public T[]    choices      { get; private set; }

        /******************** Constructors ********************/
        public Field ( string dbColumn
                     , T      defaultValue
                     , string apiName      = null
                     , string apiGroup     = null
                     , T[]    choices      = null
                     , bool   primaryKey   = false
                     , bool?  nullable     = null
                     , bool?  unique       = null)

            :   base ( apiName    : apiName
                     , apiGroup   : apiGroup
                     , dbColumn   : dbColumn
                     , primaryKey : primaryKey
                     , nullable   : nullable
                     , unique     : unique)
        {
            this.choices      = choices;
            this.defaultValue = defaultValue;

            // Validating default value
            if (this.defaultValue != null)
                this.ValidateValue(this.defaultValue);

            // Validating choices
            if (this.choices != null) {
                for(int c=0; c < this.choices.Length; c++) {
                    this.ValidateValue(this.choices[c]);
                }
            }

        }

        private Field(T value) {
            this.value = value;
        }

        /******************** Methods ********************/
        public override void Validate() {

            // Nullable values
            if (this.nullable && this.value == null)
                throw new InvalidValue("Value is not nullable");

            // Choices
            if (this.choices != null) {

                if (!this.choices.Contains(this.value))
                    throw new InvalidValue("Value is not present in choices");

            }

            base.Validate();
        }
        
        public virtual void ValidateValue(T value) {}

        public override Oracle.ManagedDataAccess.Client.OracleDbType GetDbType() {
            throw new NotImplementedException();
        }

    }
}