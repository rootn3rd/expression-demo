using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace FastExpressionPlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            var someText = "Something";
            Expression<Func<Cat, string>> expression = cat => cat.SayMeow(someText);

            var stopWatch = Stopwatch.StartNew();
            var runCounter = 10000;
            var list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var body = expression.Body as MethodCallExpression;

                var argument = body.Arguments[0];

                //()=> (object)test
                var converted = Expression.Convert(argument, typeof(object));

                var lambda = Expression.Lambda<Func<object>>(converted);

                var compiled = lambda.Compile();

                list.Add(compiled() as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as normal expression and convert");
            Console.WriteLine($"{list.Count}");


            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var body = expression.Body as MethodCallExpression;

                var argument = body.Arguments[0];

                //()=> (object)test
                var converted = Expression.Convert(argument, typeof(object));

                var lambda = Expression.Lambda<Func<object>>(converted);

                var compiled = lambda.CompileFast();

                list.Add(compiled() as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as fast compiled");
            Console.WriteLine($"{list.Count}");



            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var body = expression.Body as MethodCallExpression;

                var argument = body.Arguments[0];

                var argumentMember = argument as MemberExpression;

                var closureClass = argumentMember.Expression as ConstantExpression;

                var closureClassValue = closureClass.Value;

                var fieldInfo = argumentMember.Member as FieldInfo;

                var fieldValue = fieldInfo.GetValue(closureClassValue);

                list.Add(fieldValue as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as reflection");
            Console.WriteLine($"{list.Count}");


            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var body = expression.Body as MethodCallExpression;

                var argument = body.Arguments[0];

                var argumentMember = argument as MemberExpression;

                var closureClass = argumentMember.Expression as ConstantExpression;

                var closureClassValue = closureClass.Value;

                var members = MemberHelper.GetMembers(closureClassValue.GetType());

                var value = members[0].Getter(closureClassValue);
                
                list.Add(value as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as reflection and compiled delegates");
            Console.WriteLine($"{list.Count}");


            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var slowExpression = new SlowExpression();

                var value = slowExpression.BuildSlowExpression();

                list.Add(value as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as slow expression builder");
            Console.WriteLine($"{list.Count}");

            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var slowExpression = new SlowExpressionWithFastCompile();

                var value = slowExpression.BuildSlowExpressionWithFastCompile();

                list.Add(value as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as slow expression builder with fastcompile");
            Console.WriteLine($"{list.Count}");


            stopWatch = Stopwatch.StartNew();
            list = new List<string>();

            for (int i = 0; i < runCounter; i++)
            {
                var slowExpression = new FastExpression();

                var value = slowExpression.BuildFastExpression();

                list.Add(value as string);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Extraction as fast expression");
            Console.WriteLine($"{list.Count}");


        }
    }

}
