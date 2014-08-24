#region Summay

// =============================================================================================
// 
// File: History.cs
// Description: TODO
// Author: ArBing
// 
// =============================================================================================
// 001  2014/08/24  ArBing      初版新建
// 
// =============================================================================================

#endregion

namespace VssPlus
{
    #region Using

    using System;
    using System.Collections.ObjectModel;

    #endregion

    /// <summary>历史信息记录类</summary>
    public class History
    {
        #region Static Fields

        private static readonly Lazy<History> LazyObject = new Lazy<History>();

        #endregion

        #region Fields

        #endregion

        #region Constructors and Destructors

        public History()
        {
        }

        #endregion

        #region Delegates

        public delegate void PushedEventHandler(object sender, string e);

        #endregion

        #region Public Events

        public event PushedEventHandler Pushed;

        #endregion

        #region Public Properties

        public static History Factory
        {
            get
            {
                return LazyObject.Value;
            }
        }

        #endregion

        #region Public Methods and Operators

        public void Push(string message)
        {
            if (this.Pushed != null)
            {
                this.Pushed(this, message);
            }
        }

        #endregion
    }
}