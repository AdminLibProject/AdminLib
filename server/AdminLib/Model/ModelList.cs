using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using db=AdminLib.Database;

namespace AdminLib.Model {
    public class ModelList<Model>  : IAdminQueryResult {

        /******************* Attributes *******************/
        public Model[] list   { get; private set; }
        public Cursor  cursor { get; private set; }

        public Debug.Debug debug   { get; set; }
        public string      message { get; set; }

        /******************* Structure *******************/
        public struct Cursor {

            /***** Attributes *****/
            public int  id;
            public bool open;

            /***** Constructors *****/
            public Cursor(db.CursorResult<Model> cursor) {
                this.id   = cursor.id;
                this.open = cursor.IsOpen;
            }

        }

        /******************* Constructors *******************/
        public ModelList(Model[] list) {
            this.list   = list;
        }

        public ModelList(Object[] list) {

            List<Model> listModel;

            listModel = new List<Model>();

            foreach (Object element in list) {
                listModel.Add((Model) element);
            }

            this.list = listModel.ToArray();
        }

        public ModelList(db.CursorResult<Model> cursor) {
            this.list = cursor.items;
            this.cursor = new Cursor(cursor);
        }

    }
}
