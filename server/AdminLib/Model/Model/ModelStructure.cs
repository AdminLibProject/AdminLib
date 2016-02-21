using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminLib.Model.Field;
using AdminLib.Data.Store;

namespace AdminLib.Model.Model {

    using ListField = Dictionary<string, BaseField>;

    public class ModelStructure : AStructure {

        /******************** Static Attribute ********************/
        private static Dictionary<string, ModelStructure> modelsByFullName = new Dictionary<string, ModelStructure>();

        /******************** Attribute ********************/

        public ListField fieldsByAttributeName = new ListField();

        /******************** Constructor ********************/
        public ModelStructure(Type type) {

            MemberInfo[] fieldsInfo;
            BaseField    field;
            Meta         meta;

            this.DefineType(type);

            ModelStructure.modelsByFullName[this.ModelType.FullName] = this;

            meta = Meta.Get(this.ModelType);

            if (meta == null)
                meta = new Meta(apiName : this.ModelType.Name);

            this.DefineMeta(meta);

            // Fields
            fieldsInfo = this.ModelType.GetMembers();

            foreach (MemberInfo info in fieldsInfo) {

                field = this.AddField(info);

                if (field == null)
                    continue;
            }
        }

        /******************** Methods ********************/
        private BaseField AddField(MemberInfo member) {

            System.Attribute[] attributes;
            BaseField          field;

            attributes = System.Attribute.GetCustomAttributes (element : member);

            foreach (System.Attribute attribute in attributes) {

                if (!(attribute is IAttributeField))
                    continue;

                field = ((IAttributeField) attribute).GetField();

                // Adding the field to the list of fields
                field.DefineAttribute(member);

                this.AddField(field);

                this.fieldsByAttributeName[member.Name]  = field;

                return field;
            }

            return null;
        }

        public BaseField GetFieldByAttributeName(string name) {
            if (!this.fieldsByAttributeName.ContainsKey(name))
                return null;

            return this.fieldsByAttributeName[name];
        }

        public override string ToString() {
            return "<Model: " + this.ApiName + ">";
        }

        /******************** Static Methods ********************/
        /// <summary>
        ///     Return the model structure of the given type
        ///     
        ///     Example :
        ///         ModelStructure.Get(typeof(Country));
        /// 
        /// </summary>
        /// <param name="type">Type of the model</param>
        /// <returns></returns>
        public static ModelStructure Get(Type type) {

            // Checking that the type is a model
            if (!type.GetInterfaces().Contains(typeof(IModel)))
                return null;

             return ModelStructure.modelsByFullName[type.FullName];
        }

    }
}