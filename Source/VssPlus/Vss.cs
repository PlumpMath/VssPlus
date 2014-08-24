#region Summay

// =============================================================================================
// 
// File: Vss.cs
// Description: Visual SourceSafe交互操作类
// Author: ArBing
// 
// =============================================================================================
// 001  2014/08/23  ArBing      初版新建
// 
// =============================================================================================

#endregion

namespace VssPlus
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.SourceSafe.Interop;

    using VssPlus.Extensions;

    #endregion

    /// <summary>Visual SourceSafe交互操作类</summary>
    public class Vss
    {
        #region Constants

        /// <summary>递归选项</summary>
        internal const VSSFlags RecursiveFlag = VSSFlags.VSSFLAG_RECURSYES | VSSFlags.VSSFLAG_FORCEDIRNO;

        /// <summary> 文件修改时间选项 /// </summary>
        internal const VSSFlags TimestampFlag = VSSFlags.VSSFLAG_TIMEMOD;

        #endregion

        #region Fields

        /// <summary>当前项目</summary>
        private VSSItem currentItem;

        /// <summary>VSS库实例</summary>
        private VSSDatabase database;

        /// <summary>本地根路径</summary>
        private string localDir = string.Empty;

        /// <summary>起始项目</summary>
        private string startPath = "$/";

        #endregion

        #region Constructors and Destructors

        /// <summary>实例化VSS <see cref="Vss" /> 对象</summary>
        private Vss()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     工厂方法创建VSS实例
        /// </summary>
        /// <param name="config">设置选项</param>
        /// <returns>VSS实例</returns>
        public static Vss Create(IReadOnlyDictionary<string, string> config)
        {
            var vss = new Vss();
            var result = vss.Init(config);
            if (result)
            {
                return vss;
            }
            return null;
        }

        /// <summary>
        ///     下载服务器项目至本地
        /// </summary>
        /// <param name="item">项目检索名</param>
        /// <param name="config">选项和版本号</param>
        /// <returns>是否成功</returns>
        public bool Get(string item, Dictionary<string, string> config)
        {
            string path;

            // 项目全路径指定时，直接下载
            if (item.StartsWith("$/"))
            {
                path = item;
            }
            else
            {
                // 递归获取项目全路径
                var root = this.database.VSSItem[this.startPath, false];
                path = this.Find(root, item);
            }

            if (!string.IsNullOrEmpty(path))
            {
                var result = this.GetItem(path, config);
                return result;
            }

            History.Factory.Push(string.Format("[Error]Item not found : {0}", item));

            return false;
        }

        /// <summary>
        ///     异步下载服务器项目至本地
        /// </summary>
        /// <param name="item">项目检索名</param>
        /// <param name="config">选项和版本号</param>
        /// <returns>是否成功</returns>
        public Task<bool> GetAsync(string item, Dictionary<string, string> config)
        {
            return Task.Run(
                () =>
                this.Get(item, config)
                );
        }

        #endregion

        #region Methods

        private void DeleteFiles(string path)
        {
            var attr = File.GetAttributes(path);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var paths = Directory.EnumerateFileSystemEntries(
                    path,
                    "*",
                    SearchOption.TopDirectoryOnly);

                foreach (var p in paths)
                {
                    this.DeleteFiles(p);
                }

                if (Directory.Exists(path))
                {
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(path, attr | FileAttributes.ReadOnly);
                    }

                    Directory.Delete(path);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    //attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(path, attr | FileAttributes.ReadOnly);
                    }

                    File.Delete(path);
                }
            }
        }

        /// <summary>
        ///     递归寻找项目全路径
        /// </summary>
        /// <param name="root">根项目</param>
        /// <param name="pattern">项目检索名</param>
        /// <returns>项目全路径</returns>
        private string Find(IVSSItem root, string pattern)
        {
            // 当前项目名，直接返回
            if (root.Name.EndsWith(pattern)
                || root.Spec.EndsWith(pattern))
            {
                return root.Spec;
            }

            // 当前项目为目录，循环寻找
            if (root.Type == VSSItemType.VSSITEM_PROJECT.ConvertTo<int>())
            {
                var items = root.Items[false];
                foreach (var o in items)
                {
                    var vssItem = o as VSSItem;
                    if (vssItem != null)
                    {
                        // 当前项目名，直接返回
                        if (vssItem.Name.EndsWith(pattern)
                            || vssItem.Spec.EndsWith(pattern))
                        {
                            return vssItem.Spec;
                        }

                        // 继续向下递归寻找
                        var result = this.Find(vssItem, pattern);

                        // 有匹配项返回，否则继续循环
                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }
                }
            }

            // 未找到
            return null;
        }

        /// <summary>下载服务器项目至本地</summary>
        /// <param name="path">项目名</param>
        /// <param name="config">选项和版本号</param>
        /// <returns>是否成功</returns>
        private bool GetItem(string path, IReadOnlyDictionary<string, string> config)
        {
            // 是否递归子目录
            var isRecursive = true;

            // 是否设置本地文件为可写
            var isWritable = false;

            // 是否覆盖本地已存在文件
            var isReplace = true;

            // 是否在下载前清空本地目录
            var isDelete = true;

            // 特定版本号
            var version = string.Empty;

            if (config != null)
            {
                if (config.ContainsKey("Recursive"))
                {
                    isRecursive = config["Recursive"].ConvertTo<bool>();
                }

                if (config.ContainsKey("Writable"))
                {
                    isWritable = config["Writable"].ConvertTo<bool>();
                }

                if (config.ContainsKey("Replace"))
                {
                    isReplace = config["Replace"].ConvertTo<bool>();
                }

                if (config.ContainsKey("Delete"))
                {
                    isDelete = config["Delete"].ConvertTo<bool>();
                }

                if (config.ContainsKey("Version"))
                {
                    version = config["Version"].ConvertTo<string>();
                }
            }

            try
            {
                // 取得VSS项目
                this.currentItem = this.database.VSSItem[path, false];
                if (!string.IsNullOrEmpty(version))
                {
                    // 取得特定版本号VSS项目
                    this.currentItem = this.currentItem.Version[version];
                }

                History.Factory.Push(string.Format("Getting : {0}", this.currentItem.Spec));

                // 设定本地下载路径
                var relativePath = path.Replace("$", string.Empty).Replace('/', '\\');
                var localPath = this.localDir + relativePath;

                // 计算Get操作选项
                var flags = (isRecursive ? RecursiveFlag.ConvertTo<int>() : 0)
                            | (isWritable
                                   ? VSSFlags.VSSFLAG_USERRONO.ConvertTo<int>()
                                   : VSSFlags.VSSFLAG_USERROYES.ConvertTo<int>())
                            | (isReplace ? VSSFlags.VSSFLAG_REPREPLACE.ConvertTo<int>() : 0)
                            | TimestampFlag.ConvertTo<int>();

                // 下载VSS项目至本地
                switch (this.currentItem.Type)
                {
                case (int)VSSItemType.VSSITEM_PROJECT:
                    if (isDelete && Directory.Exists(localPath))
                    {
                        //DeleteFiles(localPath);
                    }

                    this.currentItem.Get(ref localPath, flags);
                    break;
                case (int)VSSItemType.VSSITEM_FILE:
                    this.currentItem.Get(ref localPath, flags);
                    break;
                }

                History.Factory.Push(string.Format("Success for get the item : {0}", this.currentItem.Spec));

                return true;
            }
            catch (Exception)
            {
                History.Factory.Push(string.Format("[Error]Failed to get the item : {0}", this.currentItem.Spec));
                return false;
            }
        }

        /// <summary>实例化VSS <see cref="Vss" /> 对象</summary>
        /// <param name="config">设置选项</param>
        private bool Init(IReadOnlyDictionary<string, string> config)
        {
            try
            {
                // srcsafe.ini路径
                var databasePath = config["DatabasePath"];

                // 用户名
                var userName = config["UserName"];

                // 用户密码
                var password = config["Password"];

                this.startPath = config["StartPath"];
                this.localDir = config["LocalPath"];

                this.database = new VSSDatabase();
                this.database.Open(
                    new FileInfo(databasePath).FullName,
                    userName,
                    password);

                History.Factory.Push("Successfully connect VSS repository");
                return true;
            }
            catch (Exception)
            {
                History.Factory.Push("[Error]Failed to connect VSS repository");
                return false;
            }
        }

        #endregion
    }
}