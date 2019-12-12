using FastExpressionCompiler;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastExpressionPlayGround
{
    public class MemberHelper
    {
        private static readonly Type typeOfObject = typeof(object);

        private static readonly ConcurrentDictionary<Type, MemberHelper[]> cache = new ConcurrentDictionary<Type, MemberHelper[]>();

        public string Name { get; set; }

        public Func<object, object> Getter { get; set; }

        // Object obj => obj.Property

        public static MemberHelper[] GetMembers(Type type)
        {
            return cache.GetOrAdd(type, _ =>
            {

                return type
                     .GetProperties()
                     .Cast<MemberInfo>()
                     .Concat(type.GetFields().Cast<MemberInfo>())
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

                         return new MemberHelper
                         {
                             Name = pr.Name,
                             Getter = propertyGetterFunc
                         };
                     }).ToArray();
            });

        }
    }

}
