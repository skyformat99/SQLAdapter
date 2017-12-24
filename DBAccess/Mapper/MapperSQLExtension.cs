using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess
{
    public static class MapperSQLExtension
    {
        /// <summary>
        ///  功能 : Reflaction 把Direction:Output 的 SqlParameter -> Maping成 class
        /// </summary>
        /// <typeparam name="TClass">要Map的Class</typeparam>
        /// <param name="parameters">SqlParameter陣列</param>
        /// <returns></returns>
        public static TClass ToClass<TClass>(this SqlParameter[] parameters) where TClass : new()
        {
            var t = new TClass();
            Type myClassType = t.GetType();
            PropertyInfo[] properties = myClassType.GetProperties();
            foreach (PropertyInfo property in properties) // class屬性 讀取
            {
                foreach (var p in parameters) // SqlParameters 讀取
                {
                    if (p.Direction == ParameterDirection.Output && 
                        p.ParameterName.ToLower() == string.Format("@{0}", property.Name.ToLower()))
                    {
                        dynamic value = p.Value;
 
                        try
                        {
                            property.SetValue(t, value);
                            break;  //找到設定後就可跳至下個屬性
                        }
                        catch (Exception) // 可能發生無法轉型之錯誤 (屬性型別設定錯誤)
                        {
                            continue;
                        }
                    }
                }
                // 偵錯查看
                string Debug = property.Name + " , " + property.GetValue(t, null);
            }
            return t;

        }


    }
}
