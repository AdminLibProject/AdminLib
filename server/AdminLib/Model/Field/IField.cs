using AdminLib.Model.Model;
using Oracle.ManagedDataAccess.Client;

namespace AdminLib.Model.Field {
    public interface IField {

        void         Validate();
        string       GetDbColumn();
        void         Initialize(AStructure model);
        OracleDbType GetDbType();
    }
}