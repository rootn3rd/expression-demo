using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExpressionTutorial1();

            var list = new List<Cat>();
            var maxCount = 1000000;


            var sw = Stopwatch.StartNew();

            for (int i = 0; i < maxCount; i++)
            {
                var cat = new Cat();
                list.Add(cat);
            }

            Console.WriteLine($"{sw.Elapsed} - Normal ");

            list = new List<Cat>();
            sw = Stopwatch.StartNew();

            for (int i = 0; i < maxCount; i++)
            {
                var c = Activator.CreateInstance<Cat>();
                list.Add(c);
            }

            Console.WriteLine($"{sw.Elapsed} - Activator No parameters ");

            list = new List<Cat>();
            var x = New<Cat>.Instance();

            sw = Stopwatch.StartNew();
            var catType = typeof(Cat);

            catType.CreateInstance<Cat>();

            for (int i = 0; i < maxCount; i++)
            {
                //var cat = New<Cat>.Instance();
                var cat = catType.CreateInstance<Cat>();
                list.Add(cat);
            }
            Console.WriteLine($"{sw.Elapsed} - ExpressionTree No parameters ");


            list = new List<Cat>();
            sw = Stopwatch.StartNew();

            for (int i = 0; i < maxCount; i++)
            {
                var c = (Cat)Activator.CreateInstance(typeof(Cat), "Dummy", 12);
                list.Add(c);
            }

            Console.WriteLine($"{sw.Elapsed} - Activator parameters ");

            list = new List<Cat>();
            sw = Stopwatch.StartNew();
           ObjectFactory.CreateInstance(typeof(Cat), "Dummy", 12);

            for (int i = 0; i < maxCount; i++)
            {
                var c = (Cat)ObjectFactory.CreateInstance(typeof(Cat), "Dummy", 12);
                list.Add(c);
            }

            Console.WriteLine($"{sw.Elapsed} - Expression parameters ");


            Console.WriteLine();
            Console.ReadLine();

        }

        private static void ExpressionTutorial1()
        {
            MyClass instance = new MyClass();

            Expression<Func<MyClass, string>> expr = c => c.MyMethod(435, "Something");
            Expression<Func<MyClass, bool>> exprProperty = c => c.MyProperty;

            //Analyze(expr);
            //Analyze(exprProperty);

            var numberConstant = Expression.Constant(42);
            var stringConstant = Expression.Constant("Something new");

            var myClassType = typeof(MyClass);

            var parameterExpression = Expression.Parameter(myClassType, "c");

            var methodInfo = myClassType.GetMethod(nameof(MyClass.MyMethod));

            var callExpression = Expression.Call(parameterExpression, methodInfo, numberConstant, stringConstant);

            var lambdaExpress = Expression.Lambda<Func<MyClass, string>>(callExpression, parameterExpression);

            var x = lambdaExpress.Compile();

            Console.WriteLine(x(instance));
        }

        public static void Analyze(Expression expression)
        {

            if (expression.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)expression;

                Console.WriteLine($"Lambda - {lambda.Body}");

                Console.WriteLine($"Lambda Parameters - {lambda.Parameters[0].Name}");

                Analyze(lambda.Body);
            }
            else if (expression.NodeType == ExpressionType.Call)
            {
                var functionCall = (MethodCallExpression)expression;

                Console.WriteLine($"Method - {functionCall.Method.Name}");

                for (int i = 0; i < functionCall.Arguments.Count; i++)
                {
                    Analyze(functionCall.Arguments[i]);
                }
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberAccess = (MemberExpression)expression;

                Console.WriteLine($"Property - {memberAccess.Member.Name}");
            }
            else if (expression.NodeType == ExpressionType.Constant)
            {
                var constExpression = (ConstantExpression)expression;

                Console.WriteLine($"ConstExpression - {constExpression.Value}");
            }

        }
    }

    public class Cat
    {
        public Cat()
        {

        }
        public Cat(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; set; }
        public int Age { get; set; }
    }

    class MyClass
    {
        public bool MyProperty { get; set; }

        public string MyMethod(int number, string text) => number + text;
    }
}
