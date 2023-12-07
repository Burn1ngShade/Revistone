using System.ComponentModel;
using System.Net.Http.Headers;
using System.Security.Permissions;
using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            /// <summary> Object within a console environment. </summary>
            public class ConsoleObject
            {
                static int currentID;

                public string name;
                public int id { get; private set; }

                public Transform transform;

                List<ConsoleComponent> components = new List<ConsoleComponent>();

                // --- CONSTRUCTORS ---

                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name, Transform transform, params ConsoleComponent[] components)
                {
                    this.name = name;
                    this.transform = transform;

                    for (int i = 0; i < components.Length; i++) AddComponent(components[i]);

                    AssignID();
                }


                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name, params ConsoleComponent[] components) : this(name, new Transform((0, 0)), components) { }
                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name = "New Object") : this(name, new Transform((0, 0)), new ConsoleComponent[0]) { }

                /// <summary> Assigns unique ID to obj, should not be called after inits</summary>
                void AssignID()
                {
                    this.id = currentID++;
                }

                // --- Components ---

                public bool AddComponent(ConsoleComponent component)
                {
                    int index = components.FindIndex(c => c.GetType() == typeof(Component));
                    if (index != -1) return false;
                    components.Add(component);
                    return true;
                }

                public bool RemoveComponent<T>() where T : ConsoleComponent
                {
                    int index = components.FindIndex(c => c.GetType() == typeof(T));
                    if (index == -1) return false;
                    components.RemoveAt(index);
                    return true;
                }

                public T? GetComponent<T>() where T : ConsoleComponent
                {
                    int index = components.FindIndex(c => c.GetType() == typeof(T));
                    if (index == -1) return null;
                    return components[index] as T;
                }

                public T GetComponentNonNullable<T>() where T : ConsoleComponent
                {
                    int index = components.FindIndex(c => c.GetType() == typeof(T));
                    if (index == -1) throw new InvalidOperationException($"Component of type {typeof(T)} not found.");
                    return components[index] as T ?? throw new InvalidOperationException($"Component of type {typeof(T)} not found.");
                }

                public bool HasComponent<T>() where T : ConsoleComponent
                {
                    return components.FindIndex(c => c.GetType() == typeof(T)) != -1;
                }

                // --- STRUCTS ---

                public struct Transform
                {
                    public (double x, double y) position;

                    public Transform((double x, double y) position)
                    {
                        this.position = position;
                    }
                }
            }
        }
    }
}