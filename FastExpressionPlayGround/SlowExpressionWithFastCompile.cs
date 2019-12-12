using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FastExpressionPlayGround
{
    public class SlowExpressionWithFastCompile
    {
        public string BuildSlowExpressionWithFastCompile()
        {
            //cat => cat.SayMeow(someText);

            var param = Expression.Parameter(typeof(Cat), "cat");

            var variable = Expression.Constant("Something");

            var body = Expression.Call(param, typeof(Cat).GetMethod(nameof(Cat.SayMeow)), variable);

            var lambda = Expression.Lambda<Func<Cat, string>>(body, param);

            var func = lambda.CompileFast();

            return func(new Cat());
        }
    }
}
