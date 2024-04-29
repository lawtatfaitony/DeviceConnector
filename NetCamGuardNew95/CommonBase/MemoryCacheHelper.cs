using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Common
{
    /// <summary>
    /// MemoryCache缓存
    /// </summary>
    /// <remarks>2018-9-27 15:28:32 wanghaoli 添加</remarks>
    public class MemoryCacheHelper
    {
        private static readonly Object _locker = new object(), _locker2 = new object();

        /// <summary>
        /// 取缓存项，如果不存在则返回空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetCacheItem<T>(String key)
        {
            try
            {
                return (T)MemoryCache.Default[key];
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 是否包含指定键的缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Contains(string key)
        {
            return MemoryCache.Default.Contains(key);
        }
        /// <summary>
        ///  使用键和值将某个缓存项插入缓存中，并指定基于时间的过期详细信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpiration"> 缓存项的固定的过期日期和时间。</param>
        public static void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            MemoryCache.Default.Set(key, value, absoluteExpiration);
        }

        /// <summary>
        /// 取缓存项,如果不存在则新增缓存项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cachePopulate"></param>
        /// <param name="slidingExpiration">在多少时间内没有被访问将被删除</param>
        /// <param name="absoluteExpiration">绝对过期时间，在设置的时间后过期，然后被删除</param>
        /// <returns></returns>
        public static T GetOrAddCacheItem<T>(String key, Func<T> cachePopulate, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid cache key");
            if (cachePopulate == null) throw new ArgumentNullException("cachePopulate");
            if (slidingExpiration == null && absoluteExpiration == null) throw new ArgumentException("Either a sliding expiration or absolute must be provided");

            if (MemoryCache.Default[key] == null)
            {
                lock (_locker)
                {
                    if (MemoryCache.Default[key] == null)
                    {
                        T cacheValue = cachePopulate();
                        if (!typeof(T).IsValueType && ((object)cacheValue) == null) //如果是引用类型且为NULL则不存缓存
                        {
                            return cacheValue;
                        }
                        var item = new CacheItem(key, cacheValue);
                        var policy = CreatePolicy(slidingExpiration, absoluteExpiration);
                        MemoryCache.Default.Add(item, policy);
                    }
                }
            }
            return (T)MemoryCache.Default[key];
        }

        /// <summary>
        /// 取缓存项,如果不存在则新增缓存项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cachePopulate"></param>
        /// <param name="dependencyFilePath"></param>
        /// <returns></returns>
        public static T GetOrAddCacheItem<T>(String key, Func<T> cachePopulate, string dependencyFilePath)
        {
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentException("Invalid cache key");
            if (cachePopulate == null) throw new ArgumentNullException("cachePopulate");

            if (MemoryCache.Default[key] == null)
            {
                lock (_locker2)
                {
                    if (MemoryCache.Default[key] == null)
                    {
                        T cacheValue = cachePopulate();
                        if (!typeof(T).IsValueType && ((object)cacheValue) == null) //如果是引用类型且为NULL则不存缓存
                        {
                            return cacheValue;
                        }

                        var item = new CacheItem(key, cacheValue);
                        var policy = CreatePolicy(dependencyFilePath);

                        MemoryCache.Default.Add(item, policy);
                    }
                }
            }

            return (T)MemoryCache.Default[key];
        }

        /// <summary>
        /// 移除指定键的缓存项
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            try
            {
                MemoryCache.Default.Remove(key);
            }
            catch
            { }
        }

        private static CacheItemPolicy CreatePolicy(TimeSpan? slidingExpiration, DateTime? absoluteExpiration)
        {
            var policy = new CacheItemPolicy();

            if (absoluteExpiration.HasValue)
            {
                policy.AbsoluteExpiration = absoluteExpiration.Value;
            }
            else if (slidingExpiration.HasValue)
            {
                policy.SlidingExpiration = slidingExpiration.Value;
            }

            policy.Priority = CacheItemPriority.Default;

            return policy;
        }

        private static CacheItemPolicy CreatePolicy(string filePath)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { filePath }));
            policy.Priority = CacheItemPriority.Default;
            return policy;
        }
    }
}