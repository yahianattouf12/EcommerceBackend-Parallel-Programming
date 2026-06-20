
using Castle.DynamicProxy;
using System.Text.Json;
using ECommerceBackend.Services.Interfaces;
using System;
using System.Linq;
using ECommerceBackend.Services;

namespace ECommerceBackend.AOP;



public class CacheAspect : IInterceptor
{
    private readonly ICacheService _cache;

    public CacheAspect(ICacheService cache)
    {
        _cache = cache;
    }

    public void Intercept(IInvocation invocation)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;

        var cacheAttr = method.GetCustomAttributes(typeof(CacheAttribute), false)
                              .FirstOrDefault() as CacheAttribute;

        if (cacheAttr == null)
        {
            invocation.Proceed();
            return;
        }

        var key = $"{method.Name}-{string.Join("-", invocation.Arguments)}";

        // نوع القيمة المرجعة
        var returnType = method.ReturnType;

        // هل القيمة المرجعة هي Task<T> ؟
        var isTask = returnType.IsGenericType &&
                     returnType.GetGenericTypeDefinition() == typeof(Task<>);

        var innerType = isTask ? returnType.GetGenericArguments()[0] : returnType;

        // محاولة جلب القيمة من الكاش
        var cachedString = _cache.GetStringAsync(key).Result;

        if (cachedString != null)
        {
            var deserialized = JsonSerializer.Deserialize(cachedString, innerType);

            if (isTask)
            {
                var taskResult = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(innerType)
                    .Invoke(null, new[] { deserialized });

                invocation.ReturnValue = taskResult;
            }
            else
            {
                invocation.ReturnValue = deserialized;
            }

            return;
        }

        // تنفيذ الميثود الأصلي
        invocation.Proceed();

        // بعد التنفيذ، أخذ النتيجة
        var result = isTask
            ? invocation.ReturnValue.GetType().GetProperty("Result")!.GetValue(invocation.ReturnValue)
            : invocation.ReturnValue;

        // تخزين في الكاش
        var json = JsonSerializer.Serialize(result);
        _cache.SetStringAsync(key, json, TimeSpan.FromSeconds(cacheAttr.Duration)).Wait();
    }
}
