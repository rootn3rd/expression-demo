using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ExpressionWebProject.Infrastructure
{
    public static class ControllerExtensions
    {
        private static readonly ConcurrentDictionary<string, string> actionNameCache = new ConcurrentDictionary<string, string>();

        public static IActionResult RedirectTo<TController>(this Controller controller, Expression<Action<TController>> redirectExpression)
        {
            if (redirectExpression.Body.NodeType != ExpressionType.Call)
            {
                throw new InvalidOperationException($"Provided expression is not a valid method call - {redirectExpression.Body}");
            }

            var methodCallExpression = (MethodCallExpression)redirectExpression.Body;

            var actionName = GetActionName(methodCallExpression);
            var controllerName = typeof(TController).Name.Replace(nameof(Controller), string.Empty);
            var routeValues = ExtractRouteValues(methodCallExpression);

            return controller.RedirectToAction(actionName, controllerName, routeValues);
        }

        private static string GetActionName(MethodCallExpression methodCallExpression)
        {
            var cacheKey = $"{methodCallExpression.Method.Name}_{methodCallExpression.Object.Type.Name}";

            return actionNameCache.GetOrAdd(cacheKey, (_) =>
            {

                var methodName = methodCallExpression.Method.Name;

                var actionNameAttrib = methodCallExpression.Method
                .GetCustomAttributes(true)
                .OfType<ActionNameAttribute>()
                .FirstOrDefault()
                ?.Name;

                return actionNameAttrib ?? methodName;
            });

        }

        public static RouteValueDictionary ExtractRouteValues(MethodCallExpression methodCallExpression)
        {
            var names = methodCallExpression.Method
                .GetParameters()
                .Select(x => x.Name)
                .ToArray();

            var values = methodCallExpression.Arguments
                .Select(arg =>
                {
                    if (arg.NodeType == ExpressionType.Constant)
                    {
                        var constExpression = (ConstantExpression)arg;
                        return constExpression.Value;
                    }
                    else if (arg.NodeType == ExpressionType.MemberAccess && ((MemberExpression)arg).Member is FieldInfo)
                    {
                        // Expression of type c => c.Action(id)
                        // Value can be extracted without compiling.
                        var memberAccessExpr = (MemberExpression)arg;
                        var constantExpression = (ConstantExpression)memberAccessExpr.Expression;
                        if (constantExpression != null)
                        {
                            var innerMemberName = memberAccessExpr.Member.Name;
                            var compiledLambdaScopeField = constantExpression.Value.GetType().GetField(innerMemberName);
                            return compiledLambdaScopeField.GetValue(constantExpression.Value);
                        }
                    }


                    //()=> (object)arg
                    var convertExpr = Expression.Convert(arg, typeof(object));

                    //()=> arg which would essentially be Func<object>

                    var funcExprs = Expression.Lambda<Func<object>>(convertExpr);
                    return funcExprs.Compile().Invoke();
                }).ToArray();

            var routeValueDict = new RouteValueDictionary();

            for (int i = 0; i < names.Length; i++)
            {
                routeValueDict.Add(names[i], values[i]);
            }

            return routeValueDict;
        }

    }
}
