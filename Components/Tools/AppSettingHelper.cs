using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace Components.Tools
{
    #region 获取json文件
    /// <summary>
    /// 获取json配置文件
    /// </summary>
    public class ConfigurationAppSetting
    {
        private static IConfigurationRoot config = null;
        /// <summary>
        ///  Microsoft.Extensions.Configuration.Json扩展包提供的    
        /// </summary>
        static ConfigurationAppSetting()
        {
            config = new ConfigurationBuilder().Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }

        public static IConfigurationRoot AppSettings
        {
            get
            {
                return config;
            }
        }

        public static string Get(string key)
        {
            return config[key];
        }

    }
    /// <summary>
    /// 获取配置文件
    /// </summary>
    public class UtilConf {

        private static IConfiguration config;
        public static IConfiguration Configuration//加载配置文件
        {
            get
            {
                if (config != null) return config;
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                return config;
            }
            set => config = value;
        }
    }
    #endregion

    /// <summary>
    /// 获取config配置文件
    /// </summary>
    public class AppSettingHelper
    {
        /// <summary>
        /// 新浪AppKey
        /// </summary>
        static public string SinaAppKey = ConfigurationAppSetting.AppSettings["SinaAppKey"];
        /// <summary>
        /// 新浪AppSecret
        /// </summary>
        static public string SinaAppSecret = ConfigurationAppSetting.AppSettings["SinaAppSecret"];
        /// <summary>
        /// 新浪授权成功后的回调地址
        /// </summary>
        static public string SinaCallbackUrl = ConfigurationAppSetting.AppSettings["SinaCallbackUrl"];
    }


}
