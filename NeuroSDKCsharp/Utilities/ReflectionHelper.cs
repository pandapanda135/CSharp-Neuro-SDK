using System.ComponentModel;
using System.Reflection;

namespace NeuroSDKCsharp.Utilities;
    
internal static class ReflectionHelpers
    {
        public static IEnumerable<T> GetAllInDomain<T>()
        {
            Logger.Info($"Running GetAllInDomain");

            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.FullName.Contains($"Steamworks")) // We remove steamworks to stop issues
                .SelectMany(asm => asm.GetTypes())
                .Where(type => !type.IsAbstract)
                .Where(type => typeof(T).IsAssignableFrom(type));// steamworks will crash

            foreach (Type type in types)
            {
                if (type.GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public) is { } method)
                {
                    yield return (T) method.Invoke(null, null);
                }
                else
                {
                    yield return (T) Activator.CreateInstance(type);
                }
            }
        }
    }
