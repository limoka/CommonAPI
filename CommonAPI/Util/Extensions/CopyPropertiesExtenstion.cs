using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ============================================================================
//    Author: Kenneth Perkins
//    Date:   May 11, 2021
//    Taken From: http://programmingnotes.org/
//    File:  Utils.cs
//    Description: Handles general utility functions
// ============================================================================

namespace CommonAPI
{
    public static class CopyPropertiesExtenstion
    {
        /// <summary>
        /// Copies all the matching properties and fields from 'source' to 'destination'
        /// </summary>
        /// <param name="source">The source object to copy from</param>  
        /// <param name="destination">The destination object to copy to</param>
        public static void CopyPropsTo<T1, T2>(this T1 source, ref T2 destination)
        {
            var sourceMembers = GetMembers(source.GetType());
            var destinationMembers = GetMembers(destination.GetType());

            // Copy data from source to destination
            foreach (MemberInfo sourceMember in sourceMembers)
            {
                if (!CanRead(sourceMember))
                {
                    continue;
                }

                MemberInfo destinationMember = destinationMembers.FirstOrDefault(x => x.Name.ToLower() == sourceMember.Name.ToLower());
                if (destinationMember == null || !CanWrite(destinationMember))
                {
                    continue;
                }

                SetObjectValue(ref destination, destinationMember, GetMemberValue(source, sourceMember));
            }
        }

        private static void SetObjectValue<T>(ref T obj, MemberInfo member, object value)
        {
            // Boxing method used for modifying structures
            object boxed = obj.GetType().IsValueType ? (object) obj : obj;
            SetMemberValue(ref boxed, member, value);
            obj = (T) boxed;
        }

        private static void SetMemberValue<T>(ref T obj, MemberInfo member, object value)
        {
            if (IsProperty(member))
            {
                PropertyInfo prop = (PropertyInfo) member;
                if (prop.SetMethod != null)
                {
                    prop.SetValue(obj, value);
                }
            }
            else if (IsField(member))
            {
                FieldInfo field = (FieldInfo) member;
                field.SetValue(obj, value);
            }
        }

        private static object GetMemberValue(object obj, MemberInfo member)
        {
            object result = null;
            if (IsProperty(member))
            {
                PropertyInfo prop = (PropertyInfo) member;
                result = prop.GetValue(obj, prop.GetIndexParameters().Count() == 1 ? new object[] {null} : null);
            }
            else if (IsField(member))
            {
                FieldInfo field = (FieldInfo) member;
                result = field.GetValue(obj);
            }

            return result;
        }

        private static bool CanWrite(MemberInfo member)
        {
            return IsProperty(member) ? ((PropertyInfo) member).CanWrite : IsField(member);
        }

        private static bool CanRead(MemberInfo member)
        {
            return IsProperty(member) ? ((PropertyInfo) member).CanRead : IsField(member);
        }

        private static bool IsProperty(MemberInfo member)
        {
            return IsType(member.GetType(), typeof(PropertyInfo));
        }

        private static bool IsField(MemberInfo member)
        {
            return IsType(member.GetType(), typeof(FieldInfo));
        }

        private static bool IsType(Type type, Type targetType)
        {
            return type == targetType || type.IsSubclassOf(targetType);
        }

        private static List<MemberInfo> GetMembers(Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public
                                                                         | BindingFlags.NonPublic;
            var members = new List<MemberInfo>();
            members.AddRange(type.GetProperties(flags));
            members.AddRange(type.GetFields(flags));
            return members;
        }
    }
}