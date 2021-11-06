using System;
using System.IO;

namespace CommonAPI
{
    public class Pool<T> : ISerializeState
        where T : IPoolable, new()
    {
        public T[] pool;
        public int poolCursor = 1;
        protected int poolCapacity;
        protected int[] poolRecycle;
        protected int recycleCursor;

        protected virtual T GetNewInstance()
        {
            return new T();
        }

        protected virtual void InitPoolItem(T item, object[] data)
        {
            
        }

        protected virtual void RemovePoolItem(T item)
        {
            item.Free();
        }

        protected virtual Action<T> InitUpdate()
        {
            return poolable => { };
        }

        public virtual void Free()
        {
            foreach (T item in pool)
            {
                if (item != null)
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

        public virtual void Export(BinaryWriter w)
        {
            w.Write(0);
            w.Write(poolCapacity);
            w.Write(poolCursor);
            w.Write(recycleCursor);
            for (int i = 1; i < poolCursor; i++)
            {
                if (pool[i] != null)
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

        public int AddPoolItem(object[] data)
        {
            return AddPoolItem(default, data);
        }
        
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
        
        public void RemovePoolItem(int id)
        {
            if (pool[id]?.GetId() != 0)
            {
                RemovePoolItem(pool[id]);

                poolRecycle[recycleCursor++] = id;
            }
        }
        
        public void UpdatePool(Func<Action<T>> initFunc = null)
        {
            Action<T> update = (initFunc ?? InitUpdate)();
            
            for (int i = 1; i < poolCursor; i++)
            {
                if (pool[i]?.GetId() != i) continue;
                
                update(pool[i]);
            }
        }
        
        public void UpdatePoolMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount, Func<Action<T>> initFunc = null)
        {
            Action<T> update = (initFunc ?? InitUpdate)();
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