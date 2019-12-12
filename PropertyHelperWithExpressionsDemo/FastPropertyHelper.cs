using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using FastExpressionCompiler;
//using FastExpressionCompiler.LightExpression;

namespace PropertyHelperWithExpressionsDemo
{
    public class FastPropertyHelper
    {

        private static readonly Type typeOfObject = typeof(object);

        private static readonly ConcurrentDictionary<Type, PropertyHelper[]> cache = new ConcurrentDictionary<Type, PropertyHelper[]>();

        public string Name { get; set; }

        public Func<object, object> Getter { get; set; }

        // Object obj => obj.Property

        public static PropertyHelper[] GetProperties(Type type)
        {
            return cache.GetOrAdd(type, _ =>
            {

                return type
                 .GetProperties()
                 .Select(pr =>
                 {
                     //Object obj
                     var parameter = Expression.Parameter(typeOfObject, "obj");

                     // (T)obj
                     var parameterConvert = Expression.Convert(parameter, type);

                     // ((T)obj).Property
                     var body = Expression.MakeMemberAccess(parameterConvert, pr);

                     // (object)(((T)object).Property)
                     var convertedBody = Expression.Convert(body, typeOfObject);

                     // Object obj => ((T)obj).Property
                     var lamdba = Expression.Lambda<Func<object, object>>(convertedBody, parameter);

                     var propertyGetterFunc = lamdba.CompileFast();

                     return new PropertyHelper
                     {
                         Name = pr.Name,
                         Getter = propertyGetterFunc
                     };
                 }).ToArray();
            });
        }
    }

}
