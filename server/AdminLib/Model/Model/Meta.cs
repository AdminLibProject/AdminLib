using System;
using System.Reflection;

namespace AdminLib.Model.Model {
    public class Meta {

        /******************** Attributes ********************/
        public string     dbTable       { get; private set; }
        public string     apiName       { get; private set; }
        public AStructure model         { get; private set; }
        public bool?      sequenceBased { get; private set; }

        /******************** Constructors ********************/
        public Meta(string table=null, string apiName = null, bool? sequenceBased=null) {

            this.dbTable       = table ?? apiName.ToUpper();
            this.apiName       = apiName ?? table;

            this.sequenceBased = sequenceBased;

        }

        public void DefineModel(AStructure model) {

            if (this.model != null)
                throw new Exception("Model already defined");

            this.model = model;
        }

        /******************** Static methods ********************/
        public static Meta Get(Type type) {

            AdminLib.Model.Meta meta;

            meta = type.GetCustomAttribute<AdminLib.Model.Meta>();

            if (meta == null)
                return null;

            return meta.meta;
        }


    }
}