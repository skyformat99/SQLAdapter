using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DBAccess
{
    public class SQLParameterBuilder
    {
        private List<SqlParameter> _parameterContainer;

        public SQLParameterBuilder()
        {
            _parameterContainer = new List<SqlParameter>();
        }
        /// <summary>
        /// 取回 SqlParameter[]
        /// </summary>
        /// <returns></returns>
        public SqlParameter[] ToArray()
        {
            return _parameterContainer.ToArray();
        }

        /// <summary>
        /// 增加Input參數
        /// </summary>
        /// <param name="name">@參數名稱</param>
        /// <param name="value">值</param>
        public SQLParameterBuilder Add_Input_Parameter(string name, object value,SqlDbType dbType)
        {
            try
            {
                var parameter = new SqlParameter(name, dbType);
                parameter.Value = value ?? DBNull.Value;
                _parameterContainer.Add(parameter);
                return this;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        /// <summary>
        /// 增加Output參數 (一般)
        /// </summary>
        /// <param name="name">@參數名稱</param>
        /// <param name="dbType">輸出型別</param>
        /// <param name="size">輸出大小</param>
        public SQLParameterBuilder Add_Output_Parameter(string name, SqlDbType dbType, int size = -1)
        {
            return Add_Output_Parameter(name, dbType,  0, 0 , size);
        }


        /// <summary>
        /// 增加Output參數 (Decimal型別專用)
        /// </summary>
        /// <param name="name">@參數名稱</param>
        /// <param name="dbType">輸出型別</param>
        /// <param name="precision">精確度</param>
        /// <param name="scale">刻度</param>
        /// <param name="size">輸出大小</param>
        /// <returns></returns>
        public SQLParameterBuilder Add_Output_Parameter(string name, SqlDbType dbType, byte precision, byte scale, int size =-1)
        {
            try
            {
                var parameter = new SqlParameter(name, dbType, size);
                parameter.Direction = ParameterDirection.Output;
                if (dbType == SqlDbType.Decimal) //特殊Decimal條件
                {
                    parameter.Precision = precision;
                    parameter.Scale = scale;
                }

                _parameterContainer.Add(parameter);
                return this;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }

}