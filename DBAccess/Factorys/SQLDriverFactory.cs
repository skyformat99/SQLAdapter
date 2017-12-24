using DBAccess.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DBAccess
{
    internal class SQLAccessFactory
    {
        private Dictionary<string, string> entries =
            new Dictionary<string, string>();
        public SQLAccessFactory(string str)
        {
            entries = LoadData(str); //讀取Xml文檔 填滿到entries
        }

        private Dictionary<string, string> LoadData(string str)
        {
            return XDocument.Load(str).Descendants("entries").
            Descendants("entry").ToDictionary(p => p.Attribute("key").Value,
            p => p.Attribute("value").Value);
        }


        /// <summary>
        /// 實體化連線工具
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal IDbDriver CreateDriver(string key)
        {
            string classname = null;
            entries.TryGetValue(key, out classname);
            if (classname == null)
                return null;

            string fullpackage = classname;

            Type t = Type.GetType(fullpackage);
            if (t == null)
                return null;

            return (IDbDriver)Activator.CreateInstance(t); 
        }






        
    }
}
