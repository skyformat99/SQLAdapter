using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Mapper
{
    public class TheMapper
    {
        /// <summary>
        /// 功能 : Reflaction 把ClassA -> Mapping -> ClassB
        /// </summary>
        /// <typeparam name="ClassB">ClassB的型別</typeparam>
        /// <param name="classA">要被Map的類別</param>
        /// <param name="classB">(選填)若類別內已有值，則ClassB帶入這</param>
        /// <returns></returns>
        public static ClassB Map<ClassB>(Object classA, Object classB = null) where ClassB : new()
        {
            var o = classA;
            var b = new ClassB();

            if (classB != null)
                b = (ClassB)classB;

            PropertyInfo[] properties = b.GetType().GetProperties();

            //抓出NewClass所有屬性
            foreach (var newClassItem in properties)
            {
                //抓出OldClass所有屬性
                foreach (var oldClassItem in o.GetType().GetProperties())
                {

                    //如果屬性一樣的話，就將值塞進NewClass的物件
                    if (newClassItem.Name.ToLower() == oldClassItem.Name.ToLower())
                    {
                        try
                        {
                            newClassItem.SetValue(b, oldClassItem.GetValue(o, null), null);
                            break;
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                    }
                }
            }
            return b;
        }


        /// <summary>
        /// 把 Model 轉成 string 參數 存入DB ErrorLog
        /// </summary>
        /// <typeparam name="T">型別</typeparam>
        /// <param name="model">模型</param>
        /// <param name="filter">篩選</param>
        /// <returns></returns>
        public static string ModelToString<T>(T model, Func<PropertyInfo, bool> filter = null)
        {
            string keyWords = "";

            Type t = model.GetType();
            IEnumerable<PropertyInfo> properties = t.GetProperties().AsEnumerable();

            if (filter != null)
            {
                properties = properties.Where(filter);
            }
            var lastItem = properties.Last();

            foreach (PropertyInfo property in properties) // class屬性 讀取
            {
                try
                {
                    keyWords += $"{property.Name}: {property.GetValue(model, null)}";
                    if (property != lastItem)
                    {
                        keyWords += " | ";
                    }
                }
                catch (Exception)
                {
                    keyWords += "[Error] | ";
                }
            }
            return keyWords;
        }
    }
}
