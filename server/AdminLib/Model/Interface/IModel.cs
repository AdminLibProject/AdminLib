using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminLib.Data.Query;

namespace AdminLib.Data.Store.Adapter {
    public interface IModel {

        void Add<Model>   (Connection connection, Model item, string path=null);
        void Create       (Connection connection, string[] fields=null);
        void Remove<Model>(Connection connection, Model item, string path=null);
        void Update       (Connection connection, string[] fields=null, string[] emptyFields=null);
        void Delete       (Connection connection);

    }
}