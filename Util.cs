using System;
using System.Collections.Generic;
using System.Reflection;
using static DragToDrop.ModBehaviour;

namespace DragToDrop;

public class Util
{
    public static T GetPrivateMember<T>(object obj, string name)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        Type type = obj.GetType();

        // 优先查字段
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
            return (T)field.GetValue(obj);

        // 再查属性
        var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
            return (T)prop.GetValue(obj);

        throw new MissingMemberException($"{type.FullName} 没有字段或属性 {name}");
    }

    public static void SetPrivateMember<T>(object obj, string name, T value)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        Type type = obj.GetType();

        // 优先查字段
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
            return;
        }

        // 再查属性
        var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
            prop.SetValue(obj, value);
            return;
        }

        throw new MissingMemberException($"{type.FullName} 没有字段或属性 {name}");
    }
    
    private static readonly Dictionary<string, MethodInfo> MethodCache = new();

    public static object CallMethod(object target, string methodName, object[] args)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        Type type = target.GetType();
        string cacheKey = $"{type.FullName}.{methodName}.{args?.Length ?? 0}";
        
        if (!MethodCache.TryGetValue(cacheKey, out var method) || method == null)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                                 BindingFlags.Public | BindingFlags.NonPublic;

            // 如果参数类型已知，优先匹配
            Type[] argTypes = args != null ? Array.ConvertAll(args, a => a?.GetType() ?? typeof(object)) : Type.EmptyTypes;
            method = type.GetMethod(methodName, flags, null, argTypes, null);

            // 如果没找到，尝试模糊匹配（Unity 某些类型可能被重写）
            if (method == null)
            {
                foreach (var m in type.GetMethods(flags))
                {
                    if (m.Name != methodName) continue;
                    var parameters = m.GetParameters();
                    if (parameters.Length != (args?.Length ?? 0)) continue;
                    method = m;
                    break;
                }
            }

            if (method == null)
                throw new MissingMethodException($"Method '{methodName}' not found in {type.FullName}");

            MethodCache[cacheKey] = method;
        }

        try
        {
            return method.Invoke(target, args);
        }
        catch (TargetInvocationException e)
        {
            Log($"Error invoking {type.FullName}.{methodName}: {e.InnerException}");
            throw e.InnerException ?? e;
        }
    }
}