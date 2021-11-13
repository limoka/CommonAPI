using UnityEngine;

namespace CommonAPI.Systems
{
    public abstract class CustomDesc : MonoBehaviour
    {
        public abstract void ApplyProperties(PrefabDesc desc);
    }
}