using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {
    [AttributeUsage ( AttributeTargets.All
                    , AllowMultiple = false)
    ]
    public class DateTimeField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.DateTimeField field { get; private set; }

        /******************** Constructors ********************/
        public DateTimeField ( string dbColumn
                             , string apiName      = null
                             , string apiGroup     = null
                             , bool   primaryKey   = false)
        {

            this.field = new Field.DateTimeField ( dbColumn     : dbColumn
                                                 , apiName      : apiName
                                                 , apiGroup     : apiGroup
                                                 , primaryKey   : primaryKey
                                                 , nullable     : null
                                                 , unique       : null);

        }

        /******************** Constructors ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}