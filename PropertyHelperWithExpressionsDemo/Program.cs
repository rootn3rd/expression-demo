using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PropertyHelperWithExpressionsDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            var stopWatch = Stopwatch.StartNew();

            var maxRunCount = 1000000;
            var dict = new Dictionary<string, object>();


            var obj = new { id = 12, query = "Text" };

            for (int i = 0; i < maxRunCount; i++)
            {
                obj
                    .GetType()
                    .GetProperties()
                    .Select(x => new
                    {
                        x.Name,
                        Value = x.GetValue(obj)
                    })
                    .ToList()
                    .ForEach(c => dict[c.Name] = c.Value);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Reflection property getters");
            Console.WriteLine($"{dict.Count}");


            dict = new Dictionary<string, object>();
            PropertyHelper.GetProperties(obj.GetType());

            for (int i = 0; i < maxRunCount; i++)
            {
                PropertyHelper
                    .GetProperties(obj.GetType())
                    .Select(x => new
                    {
                        x.Name,
                        Value = x.Getter(obj)
                    })
                    .ToList()
                    .ForEach(c => dict[c.Name] = c.Value);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Expression property getters");
            Console.WriteLine($"{dict.Count}");



            dict = new Dictionary<string, object>();
            FastPropertyHelper.GetProperties(obj.GetType());

            for (int i = 0; i < maxRunCount; i++)
            {
                FastPropertyHelper
                    .GetProperties(obj.GetType())
                    .Select(x => new
                    {
                        x.Name,
                        Value = x.Getter(obj)
                    })
                    .ToList()
                    .ForEach(c => dict[c.Name] = c.Value);
            }

            Console.WriteLine($"{stopWatch.Elapsed} - Fast Expression property getters");
            Console.WriteLine($"{dict.Count}");


        }
    }

}
