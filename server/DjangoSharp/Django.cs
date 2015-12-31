using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using DjangoSharp.Model;

namespace DjangoSharp {
    public class Django {

        /******************** Static attributes ********************/
        private static Dictionary<string, ModelStructure> models = new Dictionary<string,ModelStructure>();
        private static bool initialized = false;
        
        /******************** Static methods ********************/
        public static void Initialize() {

            Type           modelType;
            Type[]         types;

            if (Django.initialized)
                throw new Exception("Already initialized");

            modelType = typeof(IModel);

            // Retreiving all models in the assembly
            types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => modelType.IsAssignableFrom(p)).ToArray();

            // Creating model structure for all classes
            for (int t = 0; t < types.Length; t++) {
                Django.InitializeModel(types[t]);
            }

            // Initializing each model structure
            // Note that list fields needs all foreign key to be initialized first
            foreach (KeyValuePair<string, ModelStructure> entry in Django.models) { 
                entry.Value.Initialize();
            }

            // Initializing list fields
            foreach (KeyValuePair<string, ModelStructure> entry in Django.models) { 
                entry.Value.InitializeListField();
            }

        }

        /// <summary>
        ///     Initialize a model
        /// </summary>
        /// <param name="model"></param>
        private static void InitializeModel(Type model) {

            MethodInfo     initializeMethod;
            ModelStructure modelStructure;

            if (model.IsAbstract || model.IsInterface)
                return;

            // Calling the "Initialize" method of the Model.
            initializeMethod = model.GetMethod("Initialize", BindingFlags.FlattenHierarchy|BindingFlags.InvokeMethod|BindingFlags.Static|BindingFlags.Public);

            if (initializeMethod == null)
                return;

            modelStructure = (ModelStructure) initializeMethod.Invoke(model, null);
            Django.models[model.FullName] = modelStructure;
        }
    }
}