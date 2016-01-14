using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {
    [AttributeUsage ( AttributeTargets.Field | AttributeTargets.Property
                    , AllowMultiple = false)
    ]
    public class CharField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.CharField field { get; private set; }

        /******************** Constructors ********************/
        public CharField ( string   dbColumn
                         , string   apiName      = null
                         , string   apiGroup     = null
                         , int      max_length   = -1
                         , bool     primaryKey   = false)
        {
            this.field = new Field.CharField ( dbColumn     : dbColumn
                                             , apiName      : apiName
                                             , apiGroup     : apiGroup
                                             , max_length   : max_length < 0 ? null : (int?) max_length
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