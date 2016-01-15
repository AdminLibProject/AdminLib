using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Data.Store.Adapter {
    public interface IModel {

        void Add<Model>   (IAdapter connection, Model item, string path=null);
        void Create       (IAdapter connection, string[] fields=null);
        void Remove<Model>(IAdapter connection, Model item, string path=null);
        void Update       (IAdapter connection, string[] fields=null, string[] emptyFields=null);
        void Delete       (IAdapter connection);

    }
}