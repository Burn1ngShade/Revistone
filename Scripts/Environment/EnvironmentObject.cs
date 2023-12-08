namespace Revistone
{
    namespace Environment
    {
        /// <summary> Object within a console environment. </summary>
        public class EnvironmentObject
        {
            static int currentID;

            public string name;
            List<string> tags;
            public int id { get; private set; }

            public EnvironmentTransform transform;

            List<EnvironmentComponent> components = new List<EnvironmentComponent>();

            // --- CONSTRUCTORS ---

            /// <summary> Object within a console environment. </summary>
            public EnvironmentObject(string name, EnvironmentTransform transform, params EnvironmentComponent[] components)
            {
                this.name = name;
                this.transform = transform;

                for (int i = 0; i < components.Length; i++) AddComponent(components[i]);

                AssignID();
            }

            /// <summary> Object within a console environment. </summary>
            public EnvironmentObject(string name, params EnvironmentComponent[] components) : this(name, new EnvironmentTransform((0, 0)), components) { }
            /// <summary> Object within a console environment. </summary>
            public EnvironmentObject(string name = "New Object") : this(name, new EnvironmentTransform((0, 0)), new EnvironmentComponent[0]) { }

            /// <summary> Assigns unique ID to obj, should not be called after inits</summary>
            void AssignID()
            {
                this.id = currentID++;
            }

            // --- Components ---

            /// <summary> Adds given component to object. </summary>
            public bool AddComponent(EnvironmentComponent component)
            {
                int index = components.FindIndex(c => c.GetType() == typeof(EnvironmentComponent));
                if (index != -1 || component.GetType() == typeof(EnvironmentTransform)) return false;
                components.Add(component);
                return true;
            }

            /// <summary> Removes component of given type from object. </summary>
            public bool RemoveComponent<T>() where T : EnvironmentComponent
            {
                int index = components.FindIndex(c => c.GetType() == typeof(T));
                if (index == -1) return false;
                components.RemoveAt(index);
                return true;
            }

            /// <summary> Trys to get component of given type, returning null if not found. </summary>
            public T? TryGetComponent<T>() where T : EnvironmentComponent
            {
                int index = components.FindIndex(c => c.GetType() == typeof(T));
                if (index == -1) return null;
                return components[index] as T;
            }

            /// <summary> Trys to get component of given type, throwing an exception if not found. </summary>
            public T GetComponent<T>() where T : EnvironmentComponent
            {
                int index = components.FindIndex(c => c.GetType() == typeof(T));
                if (index == -1) throw new InvalidOperationException($"Component of type {typeof(T)} not found.");
                return components[index] as T ?? throw new InvalidOperationException($"Component of type {typeof(T)} not found.");
            }

            /// <summary> Trys to get component of given type, returning null if not found. </summary>
            public EnvironmentComponent? TryGetComponent(Type t)
            {
                int index = components.FindIndex(c => c.GetType() == t);
                if (index == -1) return null;
                return components[index];
            }

            /// <summary> Trys to get component of given type, throwing an exception if not found. </summary>
            public EnvironmentComponent GetComponent(Type t)
            {
                int index = components.FindIndex(c => c.GetType() == t);
                if (index == -1) throw new InvalidOperationException($"Component of type {t} not found.");
                return components[index];
            }

            /// <summary> Checks if object has given component. </summary>
            public bool HasComponent<T>() where T : EnvironmentComponent
            {
                return components.FindIndex(c => c.GetType() == typeof(T)) != -1;
            }

            // --- TAGS ---

            /// <summary> Adds given tag to component, if not already added. </summary>
            public bool AddTag(string tag)
            {
                if (tags.Contains(tag)) return false;
                tags.Add(tag);
                return true;
            }

            /// <summary> Removes given tag from component, if exists. </summary>
            public bool RemoveTag(string tag)
            {
                if (!tags.Contains(tag)) return false;
                tags.Remove(tag);
                return true;
            }

            /// <summary> Checks if a component has a tag. </summary>
            public bool HasTag(string tag)
            {
                return tags.Contains(tag);
            }
        }
    }
}