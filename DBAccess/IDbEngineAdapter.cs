using System.Data;
using System.Data.SqlClient;

namespace DBAccess
{
    public interface IDbEngineAdapter
    {

        /// <summary>
        /// 使用 DataAdapter 存取資料庫
        /// </summary>
        /// <param name="sql">sql敘述</param>
        /// <param name="commandType">連線CommandType</param>
        /// <param name="parameters">Sql參數</param>
        /// <returns>int</returns>
        DataSet Excute(string sql, CommandType commandType, params SqlParameter[] parameters);


        /// <summary>
        /// 使用 ExcuteNonQuery 存取資料庫
        /// </summary>
        /// <param name="sql">sql敘述</param>
        /// <param name="commandType">連線CommandType</param>
        /// <param name="parameters">Sql參數</param>
        /// <returns>DataSet</returns>
        int ExcuteNonQuery(string sql, CommandType commandType, params SqlParameter[] parameters);


        /// <summary>
        /// 執行使用 ExcuteReader 存取資料庫，
        /// 注意: 要手動使用 dr.Close(); 關閉原先的Connection
        /// </summary>
        /// <param name="sql">sql敘述</param>
        /// <param name="type">CommandType類型</param>
        /// <param name="parameters">陣列參數</param>
        /// <returns>IDataReader</returns>
        IDataReader ExcuteReader(string sql, CommandType commandType, params SqlParameter[] parameters);

    }
}