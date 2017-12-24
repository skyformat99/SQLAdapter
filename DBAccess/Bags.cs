using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess
{
    public enum ConnectionMode
    {
        Develop = 1, // 開發
        Test = 2, // 測試
        Published = 3 // 正式
    }




    #region 參考使用而已
    /// <summary>
    /// 傳送資料
    /// </summary>
    [Obsolete("可不須使用到")]
    public class ConnectionBag
    {
        public string Sql { get; set; }
        public CommandType CommandType { get; set; }
        public SqlParameter[] Parameters { get; set; }

    }
    /// <summary>
    /// 回傳資料
    /// </summary>
    [Obsolete("可不須使用到")]
    public class ReturnBag
    {
        public DataSet DataSet { get; set; }
        public SqlParameter[] OutputParameters { get; set; }
    }

    #endregion
}
