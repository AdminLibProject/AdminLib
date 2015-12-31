using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp {
    [AttributeUsage ( AttributeTargets.All
                    , AllowMultiple = false)
    ]
    public class NumberField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.NumberField field { get; private set; }

        /******************** Constructors ********************/
        public NumberField ( string dbColumn
                           , string apiName      = null
                           , string apiGroup     = null
                           , bool   primaryKey   = false)
        {

            this.field = new Field.NumberField ( dbColumn     : dbColumn
                                               , apiName      : apiName
                                               , apiGroup     : apiGroup
                                               , primaryKey   : primaryKey
                                               , nullable     : null
                                               , unique       : null);

        }

        /******************** Methods ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}