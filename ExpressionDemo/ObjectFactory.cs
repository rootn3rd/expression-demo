using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionDemo
{
    public static class ObjectFactory
    {
        public static T CreateInstance<T>(this Type type) where T : new()
            => New<T>.Instance();


        public static object CreateInstance<TArg>(this Type type, TArg arg)
            => CreateInstance<TArg, TypeToIgnore>(type, arg, null);

        public static object CreateInstance<TArg1, TArg2>(this Type type, TArg1 arg1, TArg2 arg2)
            => CreateInstance<TArg1, TArg2, TypeToIgnore>(type, arg1, arg2, null);

        //(TArg1 arg1,TArg2 arg2,TArg3 arg3) => new Type(arg1, arg2, arg3)
        public static object CreateInstance<TArg1, TArg2, TArg3>(this Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            => ObjectFactoryCreator<TArg1, TArg2, TArg3>.CreateInstance(type, arg1, arg2, arg3);

        private class TypeToIgnore { }

        private static class ObjectFactoryCreator<TArg1, TArg2, TArg3>
        {
            //public static Func<Type, TArg1, TArg2, TArg3, object> 
            public static Type TypeToIgnore = typeof(TypeToIgnore);

            private static ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>> objectFactoryCache
                = new ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>>();
            public static object CreateInstance(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {

                var objectFactoryFunc = objectFactoryCache.GetOrAdd(type, _ =>
                {


                    var argumentTypes = new[]
                    {
                        typeof(TArg1), typeof(TArg2), typeof(TArg3)
                    };

                    var constructorArgs = argumentTypes.Where(t => t != TypeToIgnore).ToArray();


                    var constructor = type.GetConstructor(constructorArgs);

                    if (constructor == null)
                    {
                        throw new InvalidOperationException($"{type.Name} does not contain constructor for argument types {string.Join(", ", constructorArgs.Select(x => x.Name))}");
                    }

                    var expressionParameters = argumentTypes
                        .Select((t, i) => Expression.Parameter(t, $"arg{i}"))
                        .ToArray();

                    var expressionConstructorParameters = expressionParameters
                        .Take(constructorArgs.Length)
                        .ToArray();

                    var newExpression = Expression.New(constructor, expressionConstructorParameters);

                    var lambdaExpression = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(newExpression, expressionParameters);

                    return lambdaExpression.Compile();

                });

                return objectFactoryFunc(arg1, arg2, arg3);
            }
        }
    }
}
