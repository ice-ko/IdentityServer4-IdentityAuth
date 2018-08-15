using Components.Tools;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Components.Sugar
{
    /// <summary>
    /// 数据操作类
    /// </summary>
    public class SqlSugarHelper
    {
        //可以直接用SimpleClient也可以扩展一个自个的类 
        //推荐直接用 SimpleClient 
        //为了照顾需要扩展的朋友，我们就来扩展一个SimpleClient，取名叫DbSet
        public class DbSet<T> : SimpleClient<T> where T : class, new()
        {
            public DbSet(SqlSugarClient context) : base(context)
            {

            }
            //SimpleClient中的方法满足不了你，你可以扩展自已的方法
            public List<T> GetByIds(dynamic[] ids)
            {
                return Context.Queryable<T>().In(ids).ToList(); ;
            }
        }

    }
    /// <summary>
    /// SqlSugar操作类
    /// </summary>
    public class SqlSugarDbContext
    {
        public static readonly SqlSugarDbContext Instance;
        static SqlSugarDbContext()
        {
            Instance = new SqlSugarDbContext();
        }
        private SqlSugarClient Db;
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public SqlSugarClient ExecutedSql()
        {
            if (Db == null)
            {
                Db = new SqlSugarClient(
               new ConnectionConfig()
               {
                   ConnectionString = ConfigurationAppSetting.AppSettings["ConnectionString:DefaultConnection"],
                   DbType = DbType.MySql,
                   IsAutoCloseConnection = true,
                   InitKeyType = InitKeyType.Attribute // Attribute用于DbFirst  从数据库生成model的
               });
            }
            Db.Aop.OnLogExecuting = (sql, pars) => //SQL执行前事件
            {
                Db.TempItems = new Dictionary<string, object>();
                Db.TempItems.Remove("logTime");
                Db.TempItems.Add("logTime", DateTime.Now);

            };
            Db.Aop.OnLogExecuted = (sql, pars) => //SQL执行完事件
            {
                var startingTime = Db.TempItems["logTime"];
                Db.TempItems.Remove("time");
                var completedTime = DateTime.Now;
            };
            Db.Aop.OnError = (exp) =>//执行SQL 错误事件
            {

            };
            Db.Aop.OnExecutingChangeSql = (sql, pars) => //SQL执行前 可以修改SQL
            {
                return new KeyValuePair<string, SugarParameter[]>(sql, pars);
            };
            return Db;
        }
    }
}