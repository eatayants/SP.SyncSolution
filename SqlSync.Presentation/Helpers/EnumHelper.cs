using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Roster.Presentation.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);
            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static IEnumerable<string> GetEnumDescriptions<T>()
        {
            Type enumType = typeof(T);

            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            foreach (T val in EnumToList<T>())
            {
                var enumVal = (Enum)(object)val;
                yield return GetEnumDescription(enumVal);
            }
        }

        public static T GetEnumByDescription<T>(string desc)
        {
            Type enumType = typeof(T);

            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            foreach (T val in EnumToList<T>())
            {
                var enumVal = (Enum)(object)val;
                var enumDescription = GetEnumDescription(enumVal);
                if (enumDescription == desc)
                {
                    return val;
                }
            }

            throw new ArgumentException(desc + " not found.");
        }
    }
}