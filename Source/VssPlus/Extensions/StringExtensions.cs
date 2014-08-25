#region Summay

// =============================================================================================
// 
// File: StringExtensions.cs
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
    using System.IO;

    using Newtonsoft.Json;

    #endregion

    /// <summary>
    ///     提供字符串相关的扩展方法
    /// </summary>
    public static class StringExtensions
    {
        #region Public Methods and Operators

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        ///     对字符串执行不区分大小写的比较
        /// </summary>
        /// <param name="str1">要比较的第一个字符串</param>
        /// <param name="str2">要比较的第二个字符串</param>
        /// <returns>两个字符串除了大小写是否存在其他区别</returns>
        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static T JsonDeserialize<T>(this string path)
        {
            try
            {
                using (var sr = new StreamReader(path))
                {
                    var data = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                    sr.Close();
                    return data;
                }
            }
            catch
            {
                return default(T);
            }
        }

        public static string JsonSerialize<T>(this T data, string path)
        {
            try
            {
                using (var sw = new StreamWriter(path))
                {
                    var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                    sw.Write(jsonData);
                    sw.Close();
                    return jsonData;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}