using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReflectionHelperTool
{
/// <summary>
/// 反射操作工具类，提供对类型成员的安全访问和修改
/// </summary>
    public static class ReflectionHelper
    {
        #region 字段操作
        //----------------------------------------- 实例字段 -----------------------------------------
        
        /// <summary>
        /// 设置实例的私有字段值
        /// </summary>
        /// <typeparam name="T">字段类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="value">要设置的值</param>
        /// <returns>是否设置成功</returns>
        /// <remarks>
        /// 适用于非public实例字段，若字段为readonly将自动尝试解除约束
        /// </remarks>
        public static bool SetPrivateField<T>(object target, string fieldName, T value)
        {
            return SetField(target, fieldName, value, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 设置实例的公共字段值
        /// </summary>
        /// <typeparam name="T">字段类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="value">要设置的值</param>
        /// <returns>是否设置成功</returns>
        /// <remarks>
        /// 适用于public实例字段，若字段为readonly将自动尝试解除约束
        /// </remarks>
        public static bool SetPublicField<T>(object target, string fieldName, T value)
        {
            return SetField(target, fieldName, value, BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 通用字段设置方法
        /// </summary>
        private static bool SetField<T>(object target, string fieldName, T value, BindingFlags flags)
        {
            if (target == null)
            {
                LogError("目标对象为null");
                return false;
            }

            Type type = target.GetType();
            FieldInfo field = type.GetField(fieldName, flags);

            if (field == null)
            {
                LogError($"找不到字段: {fieldName}");
                return false;
            }

            try
            {
                HandleReadonlyField(field);
                field.SetValue(target, value);
                LogSuccess($"设置字段 [{fieldName}] 成功，新值: {value}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"设置字段时发生异常: {ex.Message}");
                return false;
            }
        }

        //----------------------------------------- 静态字段 -----------------------------------------
        
        /// <summary>
        /// 设置静态只读字段值
        /// </summary>
        /// <typeparam name="TClass">包含该字段的类类型</typeparam>
        /// <typeparam name="TValue">字段值类型</typeparam>
        /// <param name="fieldName">字段名称</param>
        /// <param name="value">要设置的值</param>
        /// <returns>是否设置成功</returns>
        /// <remarks>
        /// 适用于public static readonly字段，通过反射修改字段约束
        /// </remarks>
        public static bool SetStaticReadonlyField<TClass, TValue>(string fieldName, TValue value)
        {
            Type type = typeof(TClass);
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

            if (field == null)
            {
                LogError($"找不到静态字段: {fieldName}");
                return false;
            }

            try
            {
                HandleReadonlyField(field);
                field.SetValue(null, value);
                LogSuccess($"设置静态字段 [{fieldName}] 成功，新值: {value}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"设置静态字段时发生异常: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 字段获取
        //----------------------------------------- 实例字段 -----------------------------------------
        
        /// <summary>
        /// 获取实例的私有字段值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>字段值，如果失败返回默认值</returns>
        public static T GetPrivateField<T>(object target, string fieldName)
        {
            return GetField<T>(target, fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 获取实例的公共字段值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>字段值，如果失败返回默认值</returns>
        public static T GetPublicField<T>(object target, string fieldName)
        {
            return GetField<T>(target, fieldName, BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 通用字段获取方法
        /// </summary>
        private static T GetField<T>(object target, string fieldName, BindingFlags flags)
        {
            if (target == null)
            {
                LogError("目标对象为null");
                return default;
            }

            Type type = target.GetType();
            FieldInfo field = type.GetField(fieldName, flags);

            if (field == null)
            {
                LogError($"找不到字段: {fieldName}");
                return default;
            }

            try
            {
                object value = field.GetValue(target);
                LogSuccess($"获取字段 [{fieldName}] 成功，值: {value}");
                return (T)value;
            }
            catch (Exception ex)
            {
                LogError($"获取字段时发生异常: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region 方法调用
        //----------------------------------------- 实例方法 -----------------------------------------
        
        /// <summary>
        /// 调用实例的私有方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">方法参数数组</param>
        /// <returns>方法返回值，如果失败返回默认值</returns>
        public static T InvokePrivateMethod<T>(object target, string methodName, params object[] parameters)
        {
            return InvokeMethod<T>(target, methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance, parameters);
        }

        /// <summary>
        /// 调用实例的公共方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="target">目标对象实例</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">方法参数数组</param>
        /// <returns>方法返回值，如果失败返回默认值</returns>
        public static T InvokePublicMethod<T>(object target, string methodName, params object[] parameters)
        {
            return InvokeMethod<T>(target, methodName, 
                BindingFlags.Public | BindingFlags.Instance, parameters);
        }

        /// <summary>
        /// 通用方法调用实现
        /// </summary>
        private static T InvokeMethod<T>(object target, string methodName, 
            BindingFlags flags, object[] parameters)
        {
            if (target == null)
            {
                LogError("目标对象为null");
                return default;
            }

            Type type = target.GetType();
            Type[] paramTypes = parameters?.Select(p => p.GetType()).ToArray();

            MethodInfo method = paramTypes != null ? 
                type.GetMethod(methodName, flags, null, paramTypes, null) :
                type.GetMethod(methodName, flags);

            if (method == null)
            {
                LogError($"找不到方法: {methodName}");
                return default;
            }

            try
            {
                object result = method.Invoke(target, parameters);
                LogSuccess($"调用方法 [{methodName}] 成功，返回值: {result}");
                return (T)result;
            }
            catch (Exception ex)
            {
                LogError($"方法调用异常: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region 辅助方法
        /// <summary>
        /// 处理只读字段约束
        /// </summary>
        private static void HandleReadonlyField(FieldInfo field)
        {
            if (!field.IsInitOnly) return;

            try
            {
                // 通过反射修改字段的只读属性
                FieldInfo attributesField = typeof(FieldInfo).GetField(
                    "m_fieldAttributes", 
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (attributesField != null)
                {
                    FieldAttributes attributes = (FieldAttributes)attributesField.GetValue(field);
                    attributes &= ~FieldAttributes.InitOnly;
                    attributesField.SetValue(field, attributes);
                }
            }
            catch (Exception ex)
            {
                LogError($"解除只读约束失败: {ex.Message}");
            }
        }

        private static void LogSuccess(string message) => 
            MyDebug.Instance.WriteDebug($"[ReflectionHelper] {message}");

        private static void LogError(string message) => 
            MyDebug.Instance.WriteDebug($"[ReflectionHelper] 错误: {message}");
        #endregion
    }
    
    internal class MyDebug
    {
        private static MyDebug _instance;
        private static readonly object _lock = new object();
        
        private readonly string _filePath = Path.Combine(Environment.CurrentDirectory, "MTF_SCP_Debug.txt");

        private MyDebug()
        {
            try
            {
                // 初始化时清空文件内容
                File.WriteAllText(_filePath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static MyDebug Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MyDebug();
                        }
                    }
                }
                return _instance;
            }
        }

        public void WriteDebug(string msg)
        {
            try
            {
                using (var sw = new StreamWriter(_filePath, true, Encoding.UTF8))
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}