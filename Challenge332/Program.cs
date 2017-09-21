using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Challenge332
{
    class Program
    {

        static IEnumerable<int> Parse(string input) => input.Split(' ').Select(s => int.Parse(s));
        static void Print(IEnumerable<int> values) => Debug.WriteLine(String.Join(' ', values));
        static void Print(int first, IEnumerable<int> values) => Debug.WriteLine(first.ToString("D2") + ": " + String.Join(' ', values));
        
        static IEnumerable<int> Peek(int previous, IEnumerable<int> input) =>
            Enumerable.Range(0, input.Count())
            .Select(i => input.Skip(i))
            .Where(lst => lst.First() > previous)
            .OrderByDescending(lst => lst.Distinct().Where(el => el > lst.First()).Count())
            .ThenBy(lst => lst.First())
            .FirstOrDefault() ?? new List<int>();
        static IEnumerable<int> GetBestPeek(string str)
        {
            var input = Parse(str);
            var newPeek = -1;
            var result = new List<int>();
            while (input.Any())
            {
                input = Peek(newPeek, input);
                if (input.Any())
                {
                    newPeek = input.First();
                    input = input.Skip(1);
                    result.Add(newPeek);
                }
            }
            return result;
        }
        static void PrintBestPeek(string input) => Print(GetBestPeek(input));
        static void Main(string[] args)
        {
            PrintBestPeek("0 8 4 12 2 10 6 14 1 9 5 13 3 11 7 15");
            PrintBestPeek("1 2 2 5 9 5 4 4 1 6");
            PrintBestPeek("4 9 4 9 9 8 2 9 0 1");
            PrintBestPeek("0 5 4 6 9 1 7 6 7 8");
            PrintBestPeek("1 2 20 13 6 15 16 0 7 9 4 0 4 6 7 8 10 18 14 10 17 15 19 0 4 2 12 6 10 5 12 2 1 7 12 12 10 8 9 2 20 19 20 17 5 19 0 11 5 20");
        }
    }
}
