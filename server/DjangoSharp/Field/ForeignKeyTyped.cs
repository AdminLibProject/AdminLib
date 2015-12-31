using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public class ForeignKey : Field.ForeignKey<int>, IForeignKey {

        /******************** Constructors ********************/
        public ForeignKey ( string dbColumn
                          , string apiName      = null
                          , string apiGroup     = null
                          , int?[] choices      = null
                          , int?   defaultValue = null
                          , bool   primaryKey   = false
                          , bool?  nullable     = null
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
        }

    }
}