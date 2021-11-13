namespace CommonAPI.Systems
{
    /// <summary>
    /// Default implementation for KeyBind press type.
    /// Defines what keypresses should be detected.
    /// Reacts only when key is pressed
    /// </summary>
    public class PressKeyBind
    {
        /// <summary>
        /// Is KeyBind activated?
        /// </summary>
        public bool keyValue
        {
            get
            {
                if (!VFInput.override_keys[defaultBind.id].IsNull())
                {
                    return ReadKey(VFInput.override_keys[defaultBind.id]);
                }

                return ReadDefaultKey();
            }
        }

        /// <summary>
        /// Default KeyBind
        /// </summary>
        public BuiltinKey defaultBind;

        public void Init(BuiltinKey defaultBind)
        {
            this.defaultBind = defaultBind;
        }

        /// <summary>
        /// Defines how this type of KeyBind should check default KeyBind
        /// </summary>
        /// <returns>If KeyBind is activated</returns>
        protected virtual bool ReadDefaultKey()
        {
            return ReadKey(defaultBind.key);
        }

        /// <summary>
        /// Defines how this type of KeyBind should check provided KeyBind
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>If KeyBind is activated</returns>
        protected virtual bool ReadKey(CombineKey key)
        {
            return key.GetKeyDown();
        }
    }
    
    /// <summary>
    /// Alternate implementation of KeyBind. Reacts only when key is held
    /// </summary>
    public class HoldKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKey();
        }
    }

    /// <summary>
    /// Alternate implementation of KeyBind. Reacts only when key is released
    /// </summary>
    public class ReleaseKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKeyUp();
        }
    }
}