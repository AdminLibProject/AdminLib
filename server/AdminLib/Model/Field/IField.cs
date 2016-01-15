using AdminLib.Model.Model;
using System.Data;

namespace AdminLib.Model.Field {
    public interface IField {

        void   Validate();
        string GetDbColumn();
        void   Initialize(AStructure model);
        DbType GetDbType();
    }
}