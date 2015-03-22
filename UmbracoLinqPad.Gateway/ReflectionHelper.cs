using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UmbracoLinqPad.Gateway
{
    internal static class ReflectionHelper
    {
        public static object GetStaticProperty(this Type type, string propertyName, Func<IEnumerable<PropertyInfo>, PropertyInfo> filter = null)
        {
            var propertyInfo = GetPropertyInfo(type, propertyName, filter);
            if (propertyInfo == null)
                throw new ArgumentOutOfRangeException("propertyName",
                    string.Format("Couldn't find property {0} in type {1}", propertyName, type.FullName));
            return propertyInfo.GetValue(null, null);
        }

        public static object CallStaticMethod(this Type type, string methodName, params object[] parameters)
        {
            var methodInfo = GetMethodInfo(type, methodName);
            if (methodInfo == null)
                throw new ArgumentOutOfRangeException("methodName",
                    string.Format("Couldn't find method {0} in type {1}", methodName, type.FullName));
            return methodInfo.Invoke(null, parameters);
        }

        public static object CallMethod(this object obj, string methodName, params object[] parameters)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type type = obj.GetType();
            var methodInfo = GetMethodInfo(type, methodName);
            if (methodInfo == null)
                throw new ArgumentOutOfRangeException("methodName",
                    string.Format("Couldn't find method {0} in type {1}", methodName, type.FullName));
            return methodInfo.Invoke(obj, parameters);
        }

        public static object CallMethod(this object obj, string methodName, Func<IEnumerable<MethodInfo>, MethodInfo> filter = null, params object[] parameters)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type type = obj.GetType();
            var methodInfo = GetMethodInfo(type, methodName, filter);
            if (methodInfo == null)
                throw new ArgumentOutOfRangeException("methodName",
                    string.Format("Couldn't find method {0} in type {1}", methodName, type.FullName));
            return methodInfo.Invoke(obj, parameters);
        }

        private static MethodInfo GetMethodInfo(Type type, string methodName, Func<IEnumerable<MethodInfo>, MethodInfo> filter = null)
        {
            MethodInfo methodInfo = null;
            do
            {
                try
                {
                    methodInfo = type.GetMethod(methodName,
                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                }
                catch (AmbiguousMatchException)
                {
                    if (filter == null) throw;

                    methodInfo = filter(
                        type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(x => x.Name == methodName));
                }
                type = type.BaseType;
            }
            while (methodInfo == null && type != null);
            return methodInfo;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName, Func<IEnumerable<PropertyInfo>, PropertyInfo> filter = null)
        {
            PropertyInfo propInfo = null;
            do
            {
                try
                {
                    propInfo = type.GetProperty(propertyName,
                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                }
                catch (AmbiguousMatchException)
                {
                    if (filter == null) throw;

                    propInfo = filter(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(x => x.Name == propertyName));
                }
                type = type.BaseType;
            }
            while (propInfo == null && type != null);
            return propInfo;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            PropertyInfo propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
                throw new ArgumentOutOfRangeException("propertyName",
                    string.Format("Couldn't find property {0} in type {1}", propertyName, objType.FullName));
            return propInfo.GetValue(obj, null);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            PropertyInfo propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
                throw new ArgumentOutOfRangeException("propertyName",
                    string.Format("Couldn't find property {0} in type {1}", propertyName, objType.FullName));
            propInfo.SetValue(obj, val, null);
        }
    }
}