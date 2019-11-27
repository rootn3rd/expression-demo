using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionDemo
{
    public static class New<T> where T : new()
    {
        // ()=> new T();
        public static Func<T> Instance =
            Expression
                .Lambda<Func<T>>
                    (Expression.New(typeof(T)))
            .Compile();
    }
}
