using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Field {
    public interface IAttributeField {
        Field.BaseField GetField();
    }
}