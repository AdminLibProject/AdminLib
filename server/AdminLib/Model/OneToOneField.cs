using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {
    [AttributeUsage ( AttributeTargets.Field | AttributeTargets.Property
                    , AllowMultiple = false)
    ]
    public class OneToOneField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.OneToOneField field { get; private set; }

        /******************** Constructors ********************/
        public OneToOneField ( string apiName      = null
                             , string dbColumn     = null
                             , string apiGroup     = null
                             , bool   primaryKey   = false)
        {
            this.field = new Field.OneToOneField ( dbColumn     : dbColumn
                                                 , apiName      : apiName
                                                 , apiGroup     : apiGroup
                                                 , nullable     : null
                                                 , primaryKey   : primaryKey);
        }

        /******************** Constructors ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}