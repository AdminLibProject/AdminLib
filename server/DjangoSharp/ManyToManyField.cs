using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp {
    [AttributeUsage ( AttributeTargets.Field | AttributeTargets.Property
                    , AllowMultiple = false)
    ]
    public class ManyToManyField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.ManyToManyField field { get; private set; }

        /******************** Constructors ********************/
        public ManyToManyField ( string midTable
                               , string apiName        = null
                               , string apiGroup       = null
                               , string field          = null
                               , string midColumn      = null
                               , string midRefColumn   = null
                               , bool   primaryKey     = false
                               , string refColumn      = null)
        {
            this.field = new Field.ManyToManyField ( midTable     : midTable
                                                   , apiName      : apiName
                                                   , apiGroup     : apiGroup
                                                   , field        : field
                                                   , midColumn    : midColumn
                                                   , midRefColumn : midRefColumn
                                                   , nullable     : null
                                                   , primaryKey   : primaryKey
                                                   , refColumn    : refColumn
                                                   , unique       : null);
        }

        /******************** Constructors ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}