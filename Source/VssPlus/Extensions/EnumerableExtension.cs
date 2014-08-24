#region Summay

// =============================================================================================
// 
// File: EnumerableExtension.cs
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
    using System.Collections.ObjectModel;
    using System.Linq;

    #endregion

    /// <summary>
    ///     提供 IEnumerable 对象的扩展方法
    /// </summary>
    public static class EnumerableExtension
    {
        #region Static Fields

        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     创建只读列表封装，避免集合类型强制类型转换后被修改
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">要创建只读列表封装的集合</param>
        /// <returns>只读列表封装</returns>
        public static IList<T> AsReadOnly<T>(this IList<T> list)
        {
            if (list == null)
            {
                return null;
            }

            return new ReadOnlyCollection<T>(list);
        }

        /// <summary>
        ///     将源集合的每一项按顺序绑定到集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TTarget> BindFrom<TSource, TTarget>(this IEnumerable<TTarget> targets,
                                                                      IEnumerable<TSource> source,
                                                                      Action<TSource, TTarget> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            return BindFrom(targets, source, (s, t, i) => binder(s, t));
        }

        /// <summary>
        ///     将源集合的每一项按顺序绑定到集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TTarget> BindFrom<TSource, TTarget>(this IEnumerable<TTarget> targets,
                                                                      IEnumerable<TSource> source,
                                                                      Action<TSource, TTarget, int> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            BindCore(source, targets, binder);

            return targets;
        }

        /// <summary>
        ///     将源集合的每一项按顺序绑定到集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="defaultValue">当源集合元素不够时所采用的默认元素</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TTarget> BindFrom<TSource, TTarget>(this IEnumerable<TTarget> targets,
                                                                      IEnumerable<TSource> source,
                                                                      TSource defaultValue,
                                                                      Action<TSource, TTarget> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            return BindFrom(targets, source, defaultValue, (s, t, i) => binder(s, t));
        }

        /// <summary>
        ///     将源集合的每一项按顺序绑定到集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="defaultValue">当源集合元素不够时所采用的默认元素</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TTarget> BindFrom<TSource, TTarget>(this IEnumerable<TTarget> targets,
                                                                      IEnumerable<TSource> source,
                                                                      TSource defaultValue,
                                                                      Action<TSource, TTarget, int> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            BindCore(source, targets, defaultValue, binder);

            return targets;
        }

        /// <summary>
        ///     将集合的每一项按顺序绑定到另一个集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TSource> BindTo<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                    IEnumerable<TTarget> targets,
                                                                    Action<TSource, TTarget> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            return BindTo(source, targets, (d, t, i) => binder(d, t));
        }

        /// <summary>
        ///     将集合的每一项按顺序绑定到另一个集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TSource> BindTo<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                    IEnumerable<TTarget> targets,
                                                                    Action<TSource, TTarget, int> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            BindCore(source, targets, binder);

            return source;
        }

        /// <summary>
        ///     将集合的每一项按顺序绑定到另一个集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="defaultValue">当源集合元素不够时所采用的默认元素</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TSource> BindTo<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                    IEnumerable<TTarget> targets,
                                                                    TSource defaultValue,
                                                                    Action<TSource, TTarget> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            return BindTo(source, targets, defaultValue, (d, t, i) => binder(d, t));
        }

        /// <summary>
        ///     将集合的每一项按顺序绑定到另一个集合
        /// </summary>
        /// <typeparam name="TSource">源集合元素类型</typeparam>
        /// <typeparam name="TTarget">目标集合元素类型</typeparam>
        /// <param name="source">源集合</param>
        /// <param name="targets">目标集合</param>
        /// <param name="defaultValue">当源集合元素不够时所采用的默认元素</param>
        /// <param name="binder">绑定方法</param>
        /// <returns>源集合</returns>
        public static IEnumerable<TSource> BindTo<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                    IEnumerable<TTarget> targets,
                                                                    TSource defaultValue,
                                                                    Action<TSource, TTarget, int> binder)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            BindCore(source, targets, defaultValue, binder);

            return source;
        }

        /// <summary>
        ///     将数组复制一份
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array">源数组</param>
        /// <returns>复制的数组</returns>
        public static T[] Copy<T>(this T[] array)
        {
            var result = new T[array.Length];
            array.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        ///     对序列每一项执行指定操作
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要执行操作的序列</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>源序列</returns>
        public static IEnumerable<T> ForAll<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        /// <summary>
        ///     对序列每一项执行指定操作
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要执行操作的序列</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>源序列</returns>
        public static IEnumerable<T> ForAll<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var i = 0;

            foreach (var item in source)
            {
                action(item, i++);
            }

            return source;
        }

        /// <summary>
        ///     对序列首项执行指定操作
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要执行操作的序列</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>源序列</returns>
        public static IEnumerable<T> ForFirst<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
                break;
            }

            return source;
        }

        /// <summary>
        ///     对序列末项执行指定操作
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要执行操作的序列</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>源序列</returns>
        public static IEnumerable<T> ForLast<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var lastItem = default(T);
            var assigned = false;

            foreach (var item in source)
            {
                lastItem = item;
                assigned = true;
            }

            if (assigned)
            {
                action(lastItem);
            }

            return source;
        }

        /// <summary>
        ///     对序列唯一项执行指定操作，若序列包含不止一个项，则抛出异常
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要执行操作的序列</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>源序列</returns>
        public static IEnumerable<T> ForSingle<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var singleItem = default(T);
            var assigned = false;

            foreach (var item in source)
            {
                if (assigned)
                {
                    throw new InvalidOperationException("序列包含一个以上的元素");
                }
                singleItem = item;
                assigned = true;
            }

            if (assigned)
            {
                action(singleItem);
            }

            return source;
        }

        /// <summary>
        ///     检查序列是否为 null 或者为空。
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要检测的序列</param>
        /// <returns>是否为 null 或者空序列</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return true;
            }

            return !source.Any();
        }

        /// <summary>
        ///     确定序列是否确实有且只有一个元素
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要检测的序列</param>
        /// <returns></returns>
        public static bool IsSingle<T>(this IEnumerable<T> source)
        {
            T obj;
            return IsSingle(source, out obj);
        }

        /// <summary>
        ///     确定序列是否确实有且只有一个元素
        /// </summary>
        /// <typeparam name="T">序列元素类型</typeparam>
        /// <param name="source">要检测的序列</param>
        /// <param name="element">如果有，则返回唯一的元素</param>
        /// <returns></returns>
        public static bool IsSingle<T>(this IEnumerable<T> source, out T element)
        {
            element = default(T);

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var onlyone = false;

            foreach (var item in source)
            {
                if (onlyone)
                {
                    return false;
                }

                onlyone = true;
                element = item;
            }

            return onlyone;
        }

        /// <summary>
        ///     从序列中去除为空（null）的项。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Where(item => item != null);
        }

        /// <summary>
        ///     随机获取一个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">元素集</param>
        /// <returns>随机挑选的一个元素</returns>
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!source.Any())
            {
                throw new ArgumentException("序列不能为空", "source");
            }

            return source.ElementAt(Random.Next(source.Count()));
        }

        #endregion

        #region Methods

        private static void BindCore<TSource, TTarget>(IEnumerable<TSource> source,
                                                       IEnumerable<TTarget> targets,
                                                       Action<TSource, TTarget, int> binder)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                using (var targetIterator = targets.GetEnumerator())
                {
                    var index = 0;

                    while (sourceIterator.MoveNext()
                           && targetIterator.MoveNext())
                    {
                        binder(sourceIterator.Current, targetIterator.Current, index++);
                    }
                }
            }
        }

        private static void BindCore<TSource, TTarget>(IEnumerable<TSource> source,
                                                       IEnumerable<TTarget> targets,
                                                       TSource defaultValue,
                                                       Action<TSource, TTarget, int> binder)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                using (var targetIterator = targets.GetEnumerator())
                {
                    var sourceEnded = false;

                    while (targetIterator.MoveNext())
                    {
                        var index = 0;

                        if (!sourceEnded)
                        {
                            sourceEnded = !sourceIterator.MoveNext();
                        }

                        var dataItem = sourceEnded ? defaultValue : sourceIterator.Current;
                        var targetItem = targetIterator.Current;

                        binder(dataItem, targetItem, index++);
                    }
                }
            }
        }

        #endregion
    }
}