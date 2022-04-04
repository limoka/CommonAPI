using System;
using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Data structure that allows to store objects with persistant ID's.<br/>
    /// When an item is removed it's <see cref="ISerializeState.Free"/> is called and ID added to free ID's list<br/>
    /// When adding new items free list is checked first to fill in the gaps.
    /// </summary>
    /// <typeparam name="T">Base class all items must inherit</typeparam>
    public class Pool<T> : ISerializeState
        where T : IPoolable, new()
    {
        public T[] pool;
        public int poolCursor = 1;
        protected int poolCapacity;
        protected int[] poolRecycle;
        protected int recycleCursor;

        /// <summary>
        /// Returns default instance for type T
        /// </summary>
        /// <returns>New Instance</returns>
        protected virtual T GetNewInstance()
        {
            return new T();
        }

        /// <summary>
        /// Init newly added items here
        /// </summary>
        /// <param name="item">Added item</param>
        /// <param name="data">Additional parameters passed to <see cref="AddPoolItem(object[])"/></param>
        protected virtual void InitPoolItem(T item, object[] data)
        {
            
        }

        /// <summary>
        /// Handle removing items here. Passed item is still valid at the time of the call. Don't forget to <see cref="ISerializeState.Free"/> it.
        /// </summary>
        /// <param name="item">Removed pool item</param>
        protected virtual void RemovePoolItem(T item)
        {
            item.Free();
        }

        /// <summary>
        /// Free the pool and all of items contained within
        /// </summary>
        public virtual void Free()
        {
            foreach (T item in pool)
            {
                if (item != null && item.GetId() != 0)
                {
                    item.Free();
                }
            }
            
            pool = null;
            poolCursor = 1;
            poolCapacity = 0;
            poolRecycle = null;
            recycleCursor = 0;
        }
        
        /// <summary>
        /// Import data of pool and all item's
        /// </summary>
        /// <param name="r">Binary Reader</param>
        public virtual void Import(BinaryReader r)
        {
            r.ReadInt32();
            int num = r.ReadInt32();
            Init(num);
            poolCursor = r.ReadInt32();
            recycleCursor = r.ReadInt32();
            for (int i = 1; i < poolCursor; i++)
            {
                if (r.ReadByte() != 1) continue;
                
                pool[i] = GetNewInstance();
                pool[i].Import(r);
            }

            for (int j = 0; j < recycleCursor; j++)
            {
                poolRecycle[j] = r.ReadInt32();
            }
        }

        /// <summary>
        /// Save pool and all of item's data
        /// </summary>
        /// <param name="w"></param>
        public virtual void Export(BinaryWriter w)
        {
            w.Write(0);
            w.Write(poolCapacity);
            w.Write(poolCursor);
            w.Write(recycleCursor);
            for (int i = 1; i < poolCursor; i++)
            {
                if (pool[i] != null && pool[i].GetId() != 0)
                {
                    w.Write((byte)1);
                    pool[i].Export(w);
                }
                else
                {
                    w.Write((byte)0);
                }
            }

            for (int j = 0; j < recycleCursor; j++)
            {
                w.Write(poolRecycle[j]);
            }
        }
        
        /// <summary>
        /// Prepare pool for set size. Also allows to resize pool as needed
        /// </summary>
        /// <param name="newSize"></param>
        public void Init(int newSize)
        {
            T[] array = pool;
            pool = new T[newSize];
            poolRecycle = new int[newSize];
            if (array != null)
            {
                Array.Copy(array, pool, (newSize <= poolCapacity) ? newSize : poolCapacity);
            }

            poolCapacity = newSize;
        }
        
        public T this[int index] => pool[index];

        /// <summary>
        /// Add default item
        /// </summary>
        /// <param name="data">Additional parameters</param>
        /// <returns>item's ID</returns>
        public int AddPoolItem(object[] data)
        {
            return AddPoolItem(default, data);
        }
        
        /// <summary>
        /// Add item
        /// </summary>
        /// <param name="data">Additional parameters</param>
        /// <returns>item's ID</returns>
        public int AddPoolItem(T item, object[] data)
        {
            int num;
            if (recycleCursor > 0)
            {
                num = poolRecycle[--recycleCursor];
            }
            else
            {
                num = poolCursor++;
                if (num == poolCapacity)
                {
                    Init(poolCapacity * 2);
                }
            }

            if (item != null)
            {
                pool[num] = item;
            }else if (pool[num] == null)
            {
                pool[num] = GetNewInstance();
            }
            
            pool[num].SetId(num);
            InitPoolItem(pool[num], data);
            
            return num;
        }
        
        /// <summary>
        /// Remove item with specified ID from pool
        /// </summary>
        /// <param name="id">item ID</param>
        public void RemovePoolItem(int id)
        {
            if (pool[id]?.GetId() != 0)
            {
                RemovePoolItem(pool[id]);

                poolRecycle[recycleCursor++] = id;
            }
        }

        private Action<T> _cachedInitUpdate = poolable => { };
        
        /// <summary>
        /// Define Update behavior here. Code in gets called only once. Returned lambda is called for each valid item
        /// </summary>
        /// <returns>Lambda that will be called for each valid item</returns>
        protected virtual Action<T> InitUpdate() => _cachedInitUpdate;
        
        /// <summary>
        /// Update all pool items in single thread. Appropriate function will be called depending on player settings.
        /// </summary>
        /// <param name="initFunc">Optional Update logic function</param>
        public void UpdatePool(Action<T> initFunc = null)
        {
            Action<T> update = (initFunc ?? InitUpdate());
            
            for (int i = 1; i < poolCursor; i++)
            {
                if (pool[i]?.GetId() != i) continue;
                
                update(pool[i]);
            }
        }
        
        /// <summary>
        /// Update all pool items using multithreading. Appropriate function will be called depending on player settings.
        /// </summary>
        /// <param name="initFunc">Optional Update logic function</param>
        public void UpdatePoolMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount, Action<T> initFunc = null)
        {
            Action<T> update = (initFunc ?? InitUpdate());
            if (WorkerThreadExecutor.CalculateMissionIndex(1, poolCursor - 1, usedThreadCount, currentThreadIdx, minimumCount, out int start, out int end))
            {
                for (int i = start; i < end; i++)
                {
                    if (pool[i]?.GetId() != i) continue;

                    update(pool[i]);
                }
            }
        }
    }
}