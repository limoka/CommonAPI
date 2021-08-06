using System;
using System.Collections.Generic;
using PowerNetworkStructures;

namespace CommonAPI
{
    public static class Algorithms
    {
        public static void NodeDfs(Node n)
        {
            n.flag = true;
            int count = n.conns.Count;
            for (int index1 = 0; index1 < count; index1 += 30)
            {
                int num = 0;
                for (int index2 = 0; index2 < 30; ++index2)
                {
                    int index3 = index1 + index2;
                    if (index3 < count)
                    {
                        if (!n.conns[index3].flag)
                            num |= 1 << index2;
                        n.conns[index3].flag = true;
                    }
                    else
                        break;
                }

                for (int index2 = 0; index2 < 30; ++index2)
                {
                    int index3 = index1 + index2;
                    if (index3 < count)
                    {
                        if ((num & 1 << index2) > 0)
                            NodeDfs(n.conns[index3]);
                    }
                    else
                        break;
                }
            }
        }

        public static void ClearNodeFlags(List<Node> l)
        {
            foreach (Node node in l)
                node.flag = false;
        }

        public static void ListSortedMerge<T>(List<T> a, List<T> b) where T : IComparable<T>
        {
            int count1 = b.Count;
            int index1 = 0;
            for (int index2 = 0; index2 < count1; ++index2)
            {
                int count2 = a.Count;
                T num = b[index2];
                bool flag = false;
                for (; index1 < count2; ++index1)
                {
                    if (a[index1].CompareTo(num) == 0)
                    {
                        flag = true;
                        break;
                    }

                    if (a[index1].CompareTo(num) > 0)
                    {
                        a.Insert(index1, num);
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    a.Add(num);
                    index1 = a.Count;
                }
            }
        }
        
        public static void ListSortedMerge(List<Node> a, List<Node> b)
        {
            int bCount = b.Count;
            int index = 0;
            for (int i = 0; i < bCount; ++i)
            {
                int aCount = a.Count;
                Node num = b[i];
                bool flag = false;
                for (; index < aCount; ++index)
                {
                    if (a[index].id == num.id)
                    {
                        flag = true;
                        break;
                    }

                    if (a[index].id > num.id)
                    {
                        a.Insert(index, num);
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    a.Add(num);
                    index = a.Count;
                }
            }
        }

        public static void ListSortedAdd<T>(List<T> l, T n) where T : IComparable<T>
        {
            int count = l.Count;
            bool flag = false;
            for (int index = 0; index < count; ++index)
            {
                if (l[index].CompareTo(n) == 0)
                {
                    flag = true;
                    break;
                }

                if (l[index].CompareTo(n) > 0)
                {
                    l.Insert(index, n);
                    flag = true;
                    break;
                }
            }

            if (flag)
                return;
            l.Add(n);
        }
        
        public static void ListSortedAdd(List<Node> l, Node n)
        {
            int count = l.Count;
            bool flag = false;
            for (int index = 0; index < count; ++index)
            {
                if (l[index].id == n.id)
                {
                    flag = true;
                    break;
                }

                if (l[index].id > n.id)
                {
                    l.Insert(index, n);
                    flag = true;
                    break;
                }
            }

            if (flag)
                return;
            l.Add(n);
        }
    }
}