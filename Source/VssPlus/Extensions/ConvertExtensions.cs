#region Summay

// =============================================================================================
// 
// File: ConvertExtensions.cs
// Description: TODO
// Author: ArBing
// 
// =============================================================================================
// 001  2014/08/23  ArBing      初版新建
// 
// =============================================================================================

#endregion

namespace VssPlus.Extensions
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    #endregion

    /// <summary>
    ///     提供类型转换扩展方法
    /// </summary>
    public static class ConvertExtensions
    {
        #region Constructors and Destructors

        static ConvertExtensions()
        {
            Convertor<short>.CastMethod = Convert.ToInt16;
            Convertor<int>.CastMethod = Convert.ToInt32;
            Convertor<long>.CastMethod = Convert.ToInt64;
            Convertor<byte>.CastMethod = Convert.ToByte;
            Convertor<ushort>.CastMethod = Convert.ToUInt16;
            Convertor<uint>.CastMethod = Convert.ToUInt32;
            Convertor<ulong>.CastMethod = Convert.ToUInt64;
            Convertor<sbyte>.CastMethod = Convert.ToSByte;
            Convertor<float>.CastMethod = Convert.ToSingle;
            Convertor<double>.CastMethod = Convert.ToDouble;
            Convertor<decimal>.CastMethod = Convert.ToDecimal;
            Convertor<bool>.CastMethod = Convert.ToBoolean;
            Convertor<DateTime>.CastMethod = Convert.ToDateTime;
            Convertor<string>.CastMethod = Convert.ToString;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     将对象强制类型转换为 T 类型。
        /// </summary>
        /// <typeparam name="T">要转换的目标类型</typeparam>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的结果</returns>
        public static T CastTo<T>(this object obj)
        {
            return (T)obj;
        }

        /// <summary>
        ///     尝试将对象转换为指定的类型
        /// </summary>
        /// <typeparam name="T">要转换的目标类型</typeparam>
        /// <param name="value">要转换的对象</param>
        /// <returns>转换后的结果</returns>
        public static T ConvertTo<T>(this object value)
        {
            if (Convertor<T>.CastMethod != null)
            {
                return Convertor<T>.CastMethod(value);
            }

            return (T)value;
        }

        /// <summary>
        ///     若值是 null 则使用指定值代换
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="value">原始值</param>
        /// <param name="defaultValue">当值为 null 时用于代换的默认值</param>
        /// <returns>原始值，当原始值不为 null 或 DBbNull，否则使用代换的默认值</returns>
        public static T IfNull<T>(this T value, T defaultValue)
        {
            if (value == null
                || Convert.IsDBNull(value))
            {
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        ///     若值是 null 则使用指定值代换
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="defaultValue">当值为 null 时用于代换的默认值</param>
        /// <returns>当原始值不为 null 或 DBbNull，返回原始值，否则返回代换的默认值</returns>
        public static object IfNull(this object value, object defaultValue)
        {
            if (value == null
                || Convert.IsDBNull(value))
            {
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        ///     若值是 null 则使用指定值代换
        /// </summary>
        /// <typeparam name="TInput">值类型</typeparam>
        /// <typeparam name="TOut">输出值类型</typeparam>
        /// <param name="value">原始值</param>
        /// <param name="defaultValue">当值为 null 时用于代换的默认值</param>
        /// <param name="converter">当值不为空时，对值进行类型转换的转换器。</param>
        /// <returns>当原始值不为 null 或 DBbNull，返回转换后的原始值，否则返回代换的默认值</returns>
        public static TOut IfNull<TInput, TOut>(this TInput value, TOut defaultValue, Func<TInput, TOut> converter)
        {
            if (value == null
                || Convert.IsDBNull(value))
            {
                return defaultValue;
            }

            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            return converter(value);
        }

        /// <summary>
        ///     将对象所有属性转换为对象图
        /// </summary>
        /// <param name="obj">要转换为对象图的对象</param>
        /// <returns>对象图</returns>
        public static IDictionary<string, string> ToPropertiesMap(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var dictionary = new Dictionary<string, string>();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj))
            {
                var key = property.Name;
                var o = property.GetValue(obj);

                string value = null;

                if (o != null)
                {
                    value = o.ToString();
                }

                dictionary.Add(key, value);
            }

            return dictionary;
        }

        #endregion

        private class Convertor<T>
        {
            #region Static Fields

            public static Func<object, T> CastMethod;

            #endregion
        }
    }
}