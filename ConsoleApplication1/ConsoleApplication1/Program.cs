using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FancyLibrary;

namespace ConsoleApplication1
{
	public class FizzBuzz
	{
		static void Main(string[] args)
		{
			IPairMatcher matcher = new PairMatcher();
			List<MatchPair> matches = new List<MatchPair>()
			{
				new MatchPair(3, "Fizz"),
				new MatchPair(5, "Buzz"),
			};

			foreach (var output in matcher.Match(matches, 100))
				Console.WriteLine(output);
		
			Console.ReadLine();
		}
	}
}
