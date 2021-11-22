namespace CommonAPI
{
    public static class PrefabDescExtensions
    {
        
        public static bool HasProperty(this PrefabDesc desc, string name)
        {
            return desc.customData.ContainsKey(name);
        }
        
        public static void SetProperty<T>(this PrefabDesc desc, string name, T value)
        {
            if (desc.customData.ContainsKey(name))
            {
                desc.customData[name] = value;
            }
            else
            {
                desc.customData.Add(name, value);
            }
        }
        
        public static T GetProperty<T>(this PrefabDesc desc, string name)
        {
            
            
            if (desc.customData.ContainsKey(name))
            {
                return (T)desc.customData[name];
            }

            return default;
        }
        
        public static T GetOrAddProperty<T>(this PrefabDesc desc, string name) where T : new()
        {
            if (desc.customData.ContainsKey(name))
            {
                return (T) desc.customData[name];
            }

            T result = new T();
            desc.customData.Add(name, result);

            return result;
        }
    }
}