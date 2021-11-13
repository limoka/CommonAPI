using UnityEngine;

namespace CommonAPI.Systems
{
    public class LodMaterials
    {
        public Material[] this[int key]
        {
            get => materials[key];
            set => materials[key] = value;
        }

        public LodMaterials()
        {
            materials = new Material[4][];
        }

        public LodMaterials(int lod, Material[] lod0)
        {
            materials = new Material[4][];
            AddLod(lod, lod0);
        }

        public LodMaterials(Material[] lod0) : this(0, lod0) { }

        public void AddLod(int lod, Material[] mats)
        {
            if (lod >= 0 && lod < 4)
            {
                materials[lod] = mats;
            }
        }

        public bool HasLod(int lod)
        {
            return materials[lod] != null;
        }

        public Material[][] materials;
    }
}