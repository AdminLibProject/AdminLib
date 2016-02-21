using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {
    [AttributeUsage ( AttributeTargets.Field | AttributeTargets.Property
                    , AllowMultiple = false)
    ]
    public class ListField : System.Attribute, Field.IAttributeField {

        /******************** Attributes ********************/
        public Field.ListField field { get; private set; }

        /******************** Constructors ********************/

        /// <summary>
        ///     API name of the field.
        ///     Note that if no refField is provided, then the model will search for a ForeignKey in the referenced model
        ///     that link back to us. If there is more than one foreign key, then an exception will be raised.
        ///             
        /// </summary>
        /// <param name="storeName">  API name of the field.</param>
        /// <param name="apiGroup"> API name of the group</param>
        /// <param name="refField">Complete API name of the field in the referenced model. </param>
        public ListField ( string storeName = null
                         , string apiGroup  = null
                         , string refField  = null)
        {
            this.field = new Field.ListField ( apiName   : storeName
                                             , apiGroup  : apiGroup
                                             , refColumn : refField);
        }

        /******************** Constructors ********************/
        public Field.BaseField GetField() {
            return this.field;
        }

    }
}