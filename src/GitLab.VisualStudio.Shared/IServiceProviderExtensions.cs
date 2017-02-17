using System;
using System.Diagnostics;

namespace GitLab.VisualStudio.Shared
{
    public static class IServiceProviderExtensions
    {
        static IUIProvider cachedUIProvider = null;

        /// <summary>
        /// Safe variant of GetService that doesn't throw exceptions if the service is
        /// not found.
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static object TryGetService(this IServiceProvider serviceProvider, Type type)
        {
            if (cachedUIProvider != null && type == typeof(IUIProvider))
                return cachedUIProvider;

            var ui = serviceProvider as IUIProvider;
            return ui != null
                ? ui.TryGetService(type)
                : GetServiceAndCache(serviceProvider, type, ref cachedUIProvider);
        }

        static object GetServiceAndCache<CacheType>(IServiceProvider provider, Type type, ref CacheType cache)
        {
            object ret = null;
            try
            {
                ret = provider.GetService(type);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                //VisualStudio.VsOutputLogger.WriteLine("GetServiceAndCache: Could not obtain instance of '{0}'", type);
            }
            if (ret != null && type == typeof(CacheType))
                cache = (CacheType)ret;
            return ret;
        }


        /// <summary>
        /// Safe generic variant that calls <see cref="TryGetService(IServiceProvider, Type)"/>
        /// so it doesn't throw exceptions if the service is not found
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static T TryGetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.TryGetService(typeof(T)) as T;
        }

        /// <summary>
        /// Safe generic variant of GetService that doesn't throw exceptions if the service
        /// is not found (calls <see cref="TryGetService(IServiceProvider, Type)"/>)
        /// </summary>
        /// <returns>The service, or null if not found</returns>
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.TryGetService(typeof(T)) as T;
        }
    }
}
