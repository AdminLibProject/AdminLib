
using System;
namespace DjangoSharp.Field {
    public class OneToOneField : ForeignKey {

        /******************** Constructors ********************/
        public OneToOneField ( string apiName      = null
                             , string dbColumn     = null
                             , string apiGroup     = null
                             , int?[] choices      = null
                             , int?   defaultValue = null
                             , bool   primaryKey   = false
                             , bool?  nullable     = null)
        :   base ( apiName      : apiName
                 , apiGroup     : apiGroup
                 , choices      : choices
                 , dbColumn     : dbColumn
                 , defaultValue : defaultValue
                 , primaryKey   : primaryKey
                 , nullable     : nullable
                 , unique       : true)
        {
        }

        /******************** Methods ********************/
        public override void Initialize(Model.AStructure model) {

            base.Initialize(model);

            if (this.dbColumn == null) {

                if (this.model.primaryKeys.Length != 1)
                    throw new Exception("Invalid number of primary keys");

                this.dbColumn = this.model.primaryKeys[0].dbColumn;
            }
        }

    }
}