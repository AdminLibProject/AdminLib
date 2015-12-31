using DjangoSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Field {
    public interface IRefField : IField {
        AStructure GetRefModel();
        BaseField  GetRefField();
    }
}