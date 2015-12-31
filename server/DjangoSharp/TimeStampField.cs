using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp {
    [AttributeUsage ( AttributeTargets.All
                    , AllowMultiple = false)
    ]
    public class TimestampField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.TimestampField field { get; private set; }

        /******************** Constructors ********************/
        public TimestampField ( string dbColumn
                              , string apiName      = null
                              , string apiGroup     = null
                              , bool   primaryKey   = false)
        {

            this.field = new Field.TimestampField ( dbColumn     : dbColumn
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