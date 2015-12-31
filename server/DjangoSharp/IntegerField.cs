using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp {
    [AttributeUsage ( AttributeTargets.All
                    , AllowMultiple = false)
    ]
    public class IntegerField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.IntegerField field { get; private set; }

        /******************** Constructors ********************/
        public IntegerField ( string dbColumn
                            , string apiName      = null
                            , string apiGroup     = null
                            , string sequence     = null
                            , bool   primaryKey   = false)
        {

            this.field = new Field.IntegerField ( dbColumn     : dbColumn
                                                , apiName      : apiName
                                                , apiGroup     : apiGroup
                                                , primaryKey   : primaryKey
                                                , nullable     : null
                                                , sequence     : sequence
                                                , unique       : null);

        }

        /******************** Methods ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}