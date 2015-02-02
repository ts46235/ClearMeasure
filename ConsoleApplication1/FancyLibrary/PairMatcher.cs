using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/* I made this class support any number of match pairs instead of just two since that wasn't very flexible
 * 
 * Version 2 could make the Match method support generics (would support a seemingly common scenario of working with floating point types):
 *		IEnumerable<string> Match<T>(IEnumerable<MatchPair<T,string> matchPairs, T upperBounds = 100, Func<T, T, bool> matchStrategy = null);
 * 
 * */

namespace FancyLibrary
{
	public interface IPairMatcher
	{
		/// <summary>
		/// Converts numbers into strings when they match
		/// </summary>
		/// <param name="matchPairs">Collection of <see cref="MatchPair"/>s used in the <paramref name="matchStrategy"/>.</param>
		/// <param name="upperBounds">Optional maximum number to interate.</param>
		/// <param name="matchStrategy">The algorithm to use to determine a match.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		IEnumerable<string> Match(IEnumerable<MatchPair> matchPairs, int upperBounds = 100, Func<int, int, bool> matchStrategy = null);
	}

	public class PairMatcher : IPairMatcher
	{
		public IEnumerable<string> Match(IEnumerable<MatchPair> matchPairs, int upperBounds = 100, Func<int, int, bool> matchStrategy = null)
		{
			if (matchPairs == null)
				throw new ArgumentNullException("matchPairs");

			// use a safe default strategy if none provided
			if(matchStrategy == null)
				matchStrategy = (num, divisor) => divisor != 0 && num % divisor == 0;

			//Remove duplicates
			var pairs = matchPairs.Distinct(new MatchPairComparer()).ToArray(); 

			for (var i = 1; i <= upperBounds; i++)
			{
				// Let any exceptions from a custom strategy bubble up to the client
				var matches = pairs.Where(mp => matchStrategy(i, mp.Divisor)).ToList();
				
				if (matches.Any())
					yield return string.Join("", matches.Select(mp => mp.TextContent).ToArray());
				else
					yield return i.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	public class MatchPair
	{
		public MatchPair()
		{
		}
		public MatchPair(int divisor, string textContent)
		{
			Divisor = divisor;
			TextContent = textContent;
		}

		public int Divisor { get; set; }
		public string TextContent { get; set; }
	}

	internal class MatchPairComparer : IEqualityComparer<MatchPair>
	{
		public bool Equals(MatchPair x, MatchPair y)
		{
			return x.Divisor.Equals(y.Divisor);
		}

		public int GetHashCode(MatchPair obj)
		{
			return obj.Divisor.GetHashCode();
		}
	}
}
