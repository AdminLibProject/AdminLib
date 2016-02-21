﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {
    [AttributeUsage ( AttributeTargets.Field | AttributeTargets.Property
                    , AllowMultiple = false)
    ]
    public class ForeignKey : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.ForeignKey field { get; private set; }

        /******************** Constructors ********************/
        public ForeignKey ( string storeName
                          , string apiName      = null
                          , string apiGroup     = null
                          , bool   primaryKey   = false)
        {
            this.field = new Field.ForeignKey ( dbColumn     : storeName
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