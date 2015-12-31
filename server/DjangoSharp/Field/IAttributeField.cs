using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public interface IAttributeField {
        Field.BaseField GetField();
    }
}