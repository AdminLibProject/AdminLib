using DjangoSharp.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Reflection;

namespace DjangoSharp.Field {
    public class ListField : BaseField, IListField {

        /******************** Model ********************/
        /// <summary>
        ///     Complete API name of the referenced column.
        /// </summary>
        public string         refColumn { get; private set; }

        /// <summary>
        ///     Field corresponding to the refColumn.
        /// </summary>
        public BaseField      refField  { get; private set; }

        /// <summary>
        ///     Model referenced.
        /// </summary>
        public ModelStructure refModel  { get; private set; }

        public Type           refType   { get; private set; }

        /******************** Constructors ********************/
        /// <summary>
        ///     API name of the field.
        /// </summary>
        /// <param name="apiName">  API name of the field.</param>
        /// <param name="apiGroup"> API name of the group</param>
        /// <param name="refColumn">Complete API name of the field in the referenced model</param>
        public ListField ( string apiName      = null
                         , string apiGroup     = null
                         , string refColumn    = null)

            :   base ( dbColumn   : null
                     , apiName    : apiName
                     , apiGroup   : apiGroup)
        {
            this.refColumn = refColumn;
        }

        /******************** Static methods ********************/

        public override object FromDbValue(object value) {
            return value;            
        }

        /// <summary>
        ///     Initialize the foreign key.
        ///     The foreign key need to retreive the structure of
        ///     the referenced model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="attribute"></param>
        public override void Initialize(AStructure model) {

            base.Initialize(model);

            // Variable declaration
            BaseField    field;
            ForeignKey[] fields;
            ForeignKey   foreignKey;
            Type         refType;

            // Finding referenced model
            if (this.Attribute.MemberType == MemberTypes.Field)
                refType = ((FieldInfo) this.Attribute).FieldType;
            else if (this.Attribute.MemberType == MemberTypes.Property)
                refType = ((PropertyInfo) this.Attribute).PropertyType;
            else
                throw new Exception(this.ToString() + " : The field is neither a C# field nor property");

            // The type of a ListField field must be an array
            if (!refType.IsArray)
                throw new Exception(this.ToString() + " : The ListField field \"" + this + "\" is not an Array");

            this.refType = refType;

            this.refModel = ModelStructure.Get(refType.GetElementType());

            if (this.refModel == null)
                throw new Exception(this.ToString() + " : Invalid model referenced");

            if (this.refColumn == null) {

                fields = this.refModel.GetFields<ForeignKey>();

                if (fields.Length == 0)
                    throw new Exception(this.ToString() + " : No suitable foreign key found");

                foreignKey = null;

                foreach (ForeignKey foreign in fields) {
                    if (foreign.refModel != this.model)
                        continue;

                    if (foreignKey != null)
                        throw new Exception(this.ToString() + " : More than one foreign key found");

                    foreignKey = foreign;
                }

                if (foreignKey == null)
                    throw new Exception(this.ToString() + " : No suitable foreign key found");

                this.refColumn = foreignKey.CompleteName;
                this.refField  = foreignKey;
            }
            else {
                field = this.refModel.GetField(this.refColumn);
               
                if (field == null)
                    throw new Exception(this.ToString() + "The field \"" + this.refColumn + "\" don't exists");

                if (!(field is IForeignKey))
                    throw new Exception(this.ToString() + "The field is not a valid foreign key");

                this.refField = field;
            }
        }

        public BaseField GetRefField() {
            return this.refField;
        }

        public AStructure GetRefModel() {
            return this.refModel;
        }

        public override OracleDbType GetDbType() {
            return OracleDbType.Int32;
        }

    }
}