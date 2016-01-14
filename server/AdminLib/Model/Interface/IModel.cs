using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Interface {
    public interface IModel {

        void Add<Model>   (IConnection connection, Model item, string path=null);
        void Create       (IConnection connection, string[] fields=null);
        void Remove<Model>(IConnection connection, Model item, string path=null);
        void Update       (IConnection connection, string[] fields=null, string[] emptyFields=null);
        void Delete       (IConnection connection);

    }
}