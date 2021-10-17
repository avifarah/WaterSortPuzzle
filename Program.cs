using System;
using System.Collections.Generic;

namespace WaterSortPuzzle
{
    class Program
    {
        static void Main(string[] args)
        {
	        try
	        {
		        TubeSolution sol = new();
		        var (done, tubes) = sol.Solve();

		        Console.WriteLine(done ? "Success!!!" : "Cannot solve");
		        Console.WriteLine(ToString(tubes));
	        }
			catch (Exception e)
	        {
		        Console.WriteLine(e.ToString());
	        }
		}

		public static string ToString(IEnumerable<Tube> tubes) => string.Join(Environment.NewLine, tubes);
    }
}
