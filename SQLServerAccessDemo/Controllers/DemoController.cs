﻿using DBAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SQLServerAccessDemo.Controllers
{
    public class DemoController : Controller
    {
        static readonly int pageRow = 100; // 一頁 Take 筆數
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查詢SQL結果頁面
        /// </summary>
        [HttpGet]
        public ActionResult Result(int? page, int? totalPage)
        {
            if (Session["tempTable"] != null)
            {
                DataTable dt = (DataTable)Session["tempTable"];

                IEnumerable<DataRow> enumerableDt = new List<DataRow>();
                if (dt != null)
                {
                    //DataSet 做分頁動作
                    if (dt.Rows.Count > 0)
                    {
                        enumerableDt = dt.AsEnumerable()
                        .Skip(((page ?? 1) - 1) * pageRow)
                        .Take(pageRow);
                    }
                    if (totalPage == null)
                        totalPage = (dt.Rows.Count % pageRow == 0) ? (int)(dt.Rows.Count / pageRow) : (int)(dt.Rows.Count / pageRow) + 1; //總頁數

                    ViewBag.TotalPage = totalPage;
                    ViewBag.TotalRow = dt.Rows.Count;

                }
                DataTable tableNew = dt;
                if (enumerableDt.Any()) // 先確認有DataRow 否則會Exception
                    tableNew = enumerableDt.CopyToDataTable();



                return View(tableNew);
            }

            return View();
        }


        /// <summary>
        /// 查詢SQL Submit送出到此
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Result(PostResource resource)
        {

            // *** 步驟(1) 建立SQL Server 的 Adapter
            IDbEngineAdapter db = DbEngineAdapter.CreateSQLServerDB(resource.TimeOut);

            // *** 步驟(2-1) 建立SQL ParameterBuilder 
            var builder = new SQLParameterBuilder();

            if (resource.SqlParameterResourceList != null)
            {
                // 迴圈 加入SQL參數
                foreach (var parameter in resource.SqlParameterResourceList)
                {
                    string pName = parameter?.Name.FirstOrDefault() == '@' ? parameter?.Name : '@' + parameter?.Name;

                    if (pName == null)
                    {
                        // Model State Set Error ...
                        return View();
                    }

                    // *** 步驟(2-2) Add Input 或 Output 參數
                    if (parameter.Direction.ToLower() == "input")
                        builder.Add_Input_Parameter(pName, parameter.Value, parameter.SqlDbType);
                    else if (parameter.Direction.ToLower() == "output")
                        if (parameter.Scale == 0 && parameter.Precision == 0)
                            builder.Add_Output_Parameter(pName, parameter.SqlDbType, parameter.Size);// 無精確度
                        else
                            builder.Add_Output_Parameter(pName, parameter.SqlDbType, parameter.Precision, parameter.Scale, parameter.Size);// 有精確度
                }
            }
            // *** 步驟(2-3) 將builder 轉成 SqlParameter 陣列
            SqlParameter[] pArrar = builder.ToArray();

            // *** 步驟(3) 執行 SQL 敘述 => 丟入 Sql 或 預存程序名稱 、 CommandType 、 SqlParameter[]
            var ds = db.Excute(
                resource.Sql,
                resource.CommandType,
                pArrar
            );

            // *** 步驟(4) (非必要) 可將SqlParameter轉Class
            //                     或是DataSet內的Table轉List<>

            // IList<YourClass> list = bag.DataSet.Tables[0].ToList<YourClass>();
            // YourClass para = bag.OutputParameters.ToClass<YourClass>();
            // .....

            // 先清除舊的 Session
            Session["tempParameters"] = null;
            Session["tempTable"] = null;

            DataTable dt = null;
            if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }

            if (pArrar.Any(p => p.Direction == ParameterDirection.Output))
            {
                Session["tempParameters"] = pArrar;
            }

            Session["tempTable"] = dt;

            DataTable tableNew = dt;
            //DataSet 做分頁動作
            if (tableNew != null)
            {
                if (tableNew.Rows.Count > 0)
                {
                    tableNew = dt.AsEnumerable()
                       .Skip(0)
                       .Take(pageRow)
                       .CopyToDataTable();
                }

                ViewBag.TotalPage = (dt.Rows.Count % pageRow == 0) ? (int)(dt.Rows.Count / pageRow) : (int)(dt.Rows.Count / pageRow) + 1; //總頁數
                ViewBag.TotalRow = dt.Rows.Count;
            }



            return View(tableNew);

        }



        /// <summary>
        /// 取得DB列表
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDataBaseList()
        {
            // *** 步驟(1) 建立SQL Server 的 Adapter
            IDbEngineAdapter db = DbEngineAdapter.CreateSQLServerDB();
            // *** 步驟(3) 執行 SQL 敘述 => 丟入 Sql 或 預存程序名稱 、 CommandType 、 SqlParameter[]
            var ds = db.Excute(
                "SELECT name FROM master.dbo.sysdatabases WHERE dbid > 4 ORDER BY name",
                CommandType.Text,
                null
            );
            var nameList = ds.Tables[0].ToList<Names>();



            return Json(nameList);
        }


        /// <summary>
        /// 取得View、SP列表
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetTableViewAndSPList(string dbName)
        {
            // *** 步驟(1) 建立SQL Server 的 Adapter
            IDbEngineAdapter db = DbEngineAdapter.CreateSQLServerDB();
            // *** 步驟(3) 執行 SQL 敘述 => 丟入 Sql 或 預存程序名稱 、 CommandType 、 SqlParameter[]
            var dsTable = db.Excute(
                $@"SELECT (TABLE_NAME) AS name FROM
                               {dbName}.INFORMATION_SCHEMA.TABLES
                               WHERE TABLE_TYPE = 'BASE TABLE'
                               ORDER BY TABLE_NAME",
                CommandType.Text,
                null
            );

            var dsView = db.Excute(
                "SELECT (TABLE_NAME) AS name FROM " + dbName + ".INFORMATION_SCHEMA.VIEWS ORDER BY TABLE_NAME",
                CommandType.Text,
                null
            );


            var dsSP = db.Excute(
                "SELECT (SPECIFIC_NAME) AS name FROM " + dbName + ".INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY SPECIFIC_NAME",
                CommandType.Text,
                null
            );


            var viewList = dsView.Tables[0].ToList<Names>();
            var spList = dsSP.Tables[0].ToList<Names>();
            var tableList = dsTable.Tables[0].ToList<Names>();


            return Json(new ViewAndSP()
            {
                Tables = tableList,
                Views = viewList,
                SPs = spList
            });
        }


        /// <summary>
        /// 取得SP參數
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSpNeedParameters(string dbName, string spName)
        {
            // *** 步驟(1) 建立SQL Server 的 Adapter
            IDbEngineAdapter db = DbEngineAdapter.CreateSQLServerDB();
            // *** 步驟(3) 執行 SQL 敘述 => 丟入 Sql 或 預存程序名稱 、 CommandType 、 SqlParameter[]
            var ds = db.Excute(
                $@" SELECT 
                        p.name AS Parameter,        
                        t.name AS Type,
                        p.is_output AS OutPut,
	                    p.max_length as Size,
	                    p.precision as Precision,
	                    p.scale as Scale
                    FROM {dbName}.sys.procedures sp
                    JOIN {dbName}.sys.parameters p 
                        ON sp.object_id = p.object_id
                    JOIN {dbName}.sys.types t
                        ON p.system_type_id = t.system_type_id
                    WHERE sp.name = '{spName}'
                    ORDER BY p.name
                  ",
                CommandType.Text,
                 null
            );

            var pList = ds.Tables[0].ToList<ParameterAndType>()
                        .Where(p => p.Type != "sysname").ToList(); // 不需要sysname


            return Json(pList);
        }


        /// <summary>
        /// 查詢SP或View的Code
        /// </summary>
        /// <param name="Type"> P: StoreProcedure  V: View </param>
        /// <returns></returns>
        public ActionResult SPOrViewCodes(string dbName, string spOrViewName, string type)
        {
            // *** 步驟(1) 建立SQL Server 的 Adapter
            IDbEngineAdapter db = DbEngineAdapter.CreateSQLServerDB();

            // *** 步驟(2-1) 建立SQL ParameterBuilder 
            var builder = new SQLParameterBuilder();
            builder.Add_Input_Parameter("@name", spOrViewName, SqlDbType.VarChar);
            builder.Add_Input_Parameter("@type", type, SqlDbType.VarChar);

            // *** 步驟(2-2) Add Input 或 Output 參數
            SqlParameter[] pArray = builder.ToArray();
            // *** 步驟(3) 執行 SQL 敘述 => 丟入 Sql 或 預存程序名稱 、 CommandType 、 SqlParameter[]
            var ds = db.Excute(
                $@" SELECT *,definition 
                    FROM {dbName}.sys.sql_modules 
                    WHERE object_id = (
	                    SELECT object_id
	                    FROM {dbName}.sys.objects 
	                    WHERE type=@type AND name= @name
                    )
                  ",
                CommandType.Text,
                 pArray
            );
            string codes = "<span style='color:red'>查無資料</span>";
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                codes = ds.Tables[0].Rows[0]["definition"].ToString().Replace("\n", "<br />");
            }
            ViewBag.Code = codes;
            return View();
        }



        /// <summary>
        /// 例外 Handler 回傳錯誤Json
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true; // <= 解決IIS回傳Html
                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Error = filterContext.Exception.Message
                    }
                };
                filterContext.ExceptionHandled = true;
            }

        }

    }







    #region Resource資源 (Model)

    public class PostResource
    {
        public string Sql { get; set; }
        public CommandType CommandType { get; set; }
        public int TimeOut { get; set; }
        public List<SqlParameterResource> SqlParameterResourceList { get; set; }
    }
    public class SqlParameterResource
    {
        public string Direction { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }

    }



    public class ParameterAndType
    {
        public string Parameter { get; set; }
        public string Type { get; set; }
        public int OutPut { get; set; }
        public int Size { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }

    }
    public class ViewAndSP
    {
        public List<Names> Tables { get; set; }
        public List<Names> Views { get; set; }
        public List<Names> SPs { get; set; }

    }

    public class Names
    {
        public string Name { get; set; }
    }
    #endregion
}