using AdminLib.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Field {
    public interface IRefField : IField {
        AStructure GetRefModel();
        BaseField  GetRefField();
    }
}