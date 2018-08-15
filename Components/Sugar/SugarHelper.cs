using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Components.Sugar
{
    /// <summary>
    /// Sugar帮助信息
    /// </summary>
    public class SugarHelper
    {
        public static readonly SugarHelper Instance;
        static SugarHelper()
        {
            Instance = new SugarHelper();
        }
        /// <summary>
        /// 根据数据库创建实体类
        /// </summary>
        public void CreateClassFiles()
        {

            var fileurl = "e:/demo";
            using (SqlSugarClient db = SqlSugarDbContext.Instance.ExecutedSql())
            {
                //将库里面所有表都生成实体类文件
                db.DbFirst.CreateClassFile(fileurl);
            }
        }
        /// <summary>
        /// 通过指定程序集实体类生成数据库表
        /// </summary>
        /// <param name="strName">程序集名称</param>
        public void CreateDataBaseTable(string strName)
        {
            using (SqlSugarClient db = SqlSugarDbContext.Instance.ExecutedSql())
            {
                //加载程序集
                // var assembly = this.GetType().GetTypeInfo().Assembly;
                var assemblys = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo theFolder = new DirectoryInfo(assemblys);
                foreach (var item in theFolder.GetFiles("*.dll"))
                {
                    if (item.FullName.Contains("MP.Models"))
                    {
                        var assembly = Assembly.Load(item.Name.Replace(".dll", ""));//加载程序集
                        foreach (var itemAssembly in assembly.ExportedTypes)
                        {
                            Type type = assembly.GetType(itemAssembly.FullName, true, true);
                            //将库里面所有表都生成实体类文件
                            db.CodeFirst.InitTables(type);
                        }
                    }

                }

            }
        }
        /// <summary>
        /// 通过指定实体类名称生成数据库表
        /// </summary>
        /// <param name="className">类名称</param>
        public void CreateDataBaseTable(Type className)
        {
            using (SqlSugarClient db = SqlSugarDbContext.Instance.ExecutedSql())
            {
                db.CodeFirst.InitTables(className);
            }
        }

    }
}
