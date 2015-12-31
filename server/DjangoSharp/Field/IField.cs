using DjangoSharp.Model;
using Oracle.ManagedDataAccess.Client;

namespace DjangoSharp.Field {
    public interface IField {

        void         Validate();
        string       GetDbColumn();
        void         Initialize(AStructure model);
        OracleDbType GetDbType();
    }
}