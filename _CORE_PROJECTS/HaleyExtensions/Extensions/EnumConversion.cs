﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.ComponentModel;

namespace Haley.Utils
{
    public static class EnumConversion
    {
        public static string getKey(this Enum @enum)
        {
            try
            {
                string enum_type_name = @enum.GetType().ToString();
                string enum_value_name = @enum.ToString();
                string enum_key = enum_type_name + "###" + enum_value_name; //Concatenated value for storing as key
                return enum_key;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string getDescription(this Enum @enum)
        {
            FieldInfo fi = @enum.GetType().GetField(@enum.ToString());
            var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? @enum.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}