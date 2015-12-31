using DjangoSharp.Field;
using System;
using System.Collections.Generic;

namespace DjangoSharp.Model {
    public class VirtualStructure : AStructure {

        /******************** Constructors ********************/
        private VirtualStructure(string dbTable) {
            
            Meta meta;

            meta = new Meta ( table   : dbTable
                            , apiName : VirtualStructure.CalcModelApiName(dbTable));

            this.DefineMeta(meta);
            this.DefineType(typeof(Dictionary<string, Object>));
        }

        /******************** Method ********************/
        public new void AddField(BaseField field) {

            BaseField existingField;

            if (field.dbColumn == null)
                throw new Exception("Field don't have dbColumn");

            existingField = this.GetFieldByDbColumn(field.dbColumn);

            if (existingField != null) {
                if (existingField.GetType() != field.GetType())
                    throw new Exception("A field already exists with a different type");

                return;
            }

            base.AddField(field);
        }

        public override void Initialize() {

            // Some fields may have been already initialized (such as Foreign Key)
            // We don't initialize them again
            foreach (BaseField field in this.fields) {
                if (field.Initialized)
                    continue;

                field.Initialize ( model : this);
            }

        }

        public override void InitializeListField() {
            foreach (BaseField field in this.fields) {
                if (!(field is IListField) || field.Initialized)
                    continue;

                field.Initialize(model: this);
            }
        }


        /******************** Static method ********************/

        /// <summary>
        ///     Build a API name usable for a virtual field.
        /// </summary>
        /// <param name="dbColumn">DB Column name of the field</param>
        /// <returns></returns>
        public static string CalcFieldApiName(string dbColumn) {
            return "#" + dbColumn;
        }

        /// <summary>
        ///     Build a API name usable for a virtual model
        ///     for the given dbTable.
        /// </summary>
        /// <param name="dbTable"></param>
        /// <returns></returns>
        public static string CalcModelApiName(string dbTable) {
            return "#" + dbTable;
        }

        public static VirtualStructure GetOrCreate(string dbTable) {

            AStructure structure;
            string     apiName;

            apiName = VirtualStructure.CalcModelApiName(dbTable);

            structure = AStructure.Get(apiName);

            if (structure != null)
                return (VirtualStructure) structure;

            return new VirtualStructure(dbTable);
        }

    }
}