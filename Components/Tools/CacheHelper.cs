using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Components.Tools
{
    /// <summary>
    /// IMemoryCache缓成助手
    /// </summary>
    public class CacheHelper
    {
        private static readonly CacheHelper _instance = new CacheHelper();
        static CacheHelper()
        {
        }

        private CacheHelper()
        {
        }

        public static CacheHelper GetInstance { get { return _instance; } }

        public IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 缓存绝对过期时间
        /// </summary>
        ///<param name="key">Cache键值</param>
        ///<param name="value">给Cache[key]赋的值</param>
        ///<param name="minute">minute分钟后绝对过期</param>
        public void CacheInsertAddMinutes(string key, object value, int minute)
        {
            if (value == null) return;
            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(minute)));
        }


        /// <summary>
        /// 缓存相对过期，最后一次访问后minute分钟后过期
        /// </summary>
        ///<param name="key">Cache键值</param>
        ///<param name="value">给Cache[key]赋的值</param>
        ///<param name="minute">滑动过期分钟</param>
        public void CacheInsertFromMinutes(string key, object value, int minute)
        {
            if (value == null) return;
            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(minute)));
        }


        /// <summary>
        ///写入缓成
        /// </summary>
        ///<param name="key">Cache键值</param>
        ///<param name="value">给Cache[key]赋的值</param>
        public void CacheInsert(string key, object value)
        {
            _memoryCache.Set(key, value);
        }

        /// <summary>
        ///清除Cache[key]的值
        /// </summary>
        ///<param name="key"></param>
        public void CacheNull(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <summary>
        ///根据key值，返回Cache[key]的值
        /// </summary>
        ///<param name="key"></param>
        public object GetCacheValue(string key)
        {
            return _memoryCache.Get(key);
        }
        /// <summary>
        /// 根据key值，返回obj对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result">obj对象</param>
        public void GetCacheValue(string key, out object result)
        {
            _memoryCache.TryGetValue(key, out result);
        }

        #region 缓成示例
        //public IActionResult Index()
        //{
        //    string cacheKey = "key";
        //    string result;
        //    if (!_memoryCache.TryGetValue(cacheKey, out result))
        //    {
        //        result = $"LineZero{DateTime.Now}";
        //        _memoryCache.Set(cacheKey, result);
        //        //设置相对过期时间2分钟
        //        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
        //            .SetSlidingExpiration(TimeSpan.FromMinutes(2)));
        //        //设置绝对过期时间2分钟
        //        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
        //            .SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
        //        //移除缓存
        //        _memoryCache.Remove(cacheKey);
        //        //缓存优先级 （程序压力大时，会根据优先级自动回收）
        //        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
        //            .SetPriority(CacheItemPriority.NeverRemove));
        //        //缓存回调 10秒过期会回调
        //        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
        //            .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
        //            .RegisterPostEvictionCallback((key, value, reason, substate) =>
        //            {
        //                Console.WriteLine($"键{key}值{value}改变，因为{reason}");
        //            }));
        //        //缓存回调 根据Token过期
        //        var cts = new CancellationTokenSource();
        //        _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
        //            .AddExpirationToken(new CancellationChangeToken(cts.Token))
        //            .RegisterPostEvictionCallback((key, value, reason, substate) =>
        //            {
        //                Console.WriteLine($"键{key}值{value}改变，因为{reason}");
        //            }));
        //        cts.Cancel();
        //    }
        //    ViewBag.Cache = result;
        //    return View();
        //}
        #endregion
    }
}