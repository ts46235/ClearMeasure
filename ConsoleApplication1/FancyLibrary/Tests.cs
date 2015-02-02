using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace FancyLibrary
{
	// ReSharper disable UnusedMember.Global

	[TestFixture]
	internal class Tests
	{
		private IPairMatcher _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new PairMatcher();
		}

		[Test]
		public void Match_WithNullPairCollection_ShouldThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _sut.Match(null, 0).ToList());
		}
		
		[Test]
		public void Match_WithPairsHavingZeroForMatchNumber_ShouldNotThrowException()
		{
			var pairs = new MatchPair[]
			{
				new MatchPair(0, "Fizz"),
				new MatchPair(){ TextContent = "Fizz2" }
			};

			Assert.DoesNotThrow(() =>_sut.Match(pairs, 1).ToList());
		}

		[Test]
		public void Match_WithPairHavingNegativeMatchNumber_ShouldNotThrowException()
		{
			var pairs = new MatchPair[]
			{
				new MatchPair(2, "Fizz"),
				new MatchPair(-1, "Fizzer"),
			};

			Assert.DoesNotThrow(() => _sut.Match(pairs, 1).ToList());
		}

		[Test]
		public void Match_WithPairHavingNegativeMatchNumber_ShouldMatch()
		{
			const string invalidText = "Fizzer";
			var pairs = new MatchPair[]
			{
				new MatchPair(2, "Fizz"),
				new MatchPair(-1, invalidText),
			};

			var replacements = _sut.Match(pairs, 2).ToList();
			Assert.AreEqual(2, replacements.Count(r => r.Contains(invalidText)));
		}

		[Test]
		public void Match_WithTwoValidPairs_ShouldReturnPopulatedList()
		{
			var pairs = BuildMatchPairs(new[] { 3, 7 }, new[] { "Fizz", "Boy" });
			IEnumerable<string> iterator = _sut.Match(pairs, 1);
			Assert.IsNotNull(iterator);
			Assert.IsNotEmpty(iterator.ToList());
		}
		
		[Test]
		public void Match_WithNoPairs_ShouldShowOnlyNumbers()
		{
			const int max = 6;
			var replacements = _sut.Match(new MatchPair[0], max).ToList();

			var results = new List<bool>();
			for (int i = 1; i <= max; i++)
				results.Add(replacements[i - 1] == i.ToString(CultureInfo.InvariantCulture));

			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithNoPairs_ShouldReturnCorrectlySizedList()
		{
			const int max = 6;
			var replacements = _sut.Match(new MatchPair[0], max).ToList();

			var results = new List<bool>();
			for (int i = 1; i <= max; i++)
				results.Add(replacements[i - 1] == i.ToString(CultureInfo.InvariantCulture));

			Assert.AreEqual(results.Count, max);
		}

		[Test]
		public void Match_WithOneValidPair_ShouldReturnCorrectlySizedList()
		{
			const int iterations = 4;
			Assert.IsTrue(_sut.Match(new[] { new MatchPair(3, "Foo") }, iterations).Count() == iterations);
		}

		[Test]
		public void Match_WithTwoValidPairs_ShouldReturnCorrectlySizedList()
		{
			var pairs = BuildMatchPairs(new[] { 3, 7 }, new[] { "Foo", "Bar" });
			const int iterations = 4;
			Assert.IsTrue(_sut.Match(pairs, iterations).Count() == iterations);
		}
		
		[Test]
		public void Match_WithTwoValidPairs_ShouldReplaceNumbersInPairs()
		{
			var pairs = BuildMatchPairs(new[] { 3, 4 }, new[] { "Foo", "Barred" });
			var results = CallMatchAndTestMultiples(pairs);
			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithTwoValidPairs_ShouldNotReplaceUnmatchedPairs()
		{
			var pairs = BuildMatchPairs(new[] { 2, 3 }, new[] { "Soda", "Pop" });
			const int max = 6;
			var replacements = _sut.Match(pairs, max).ToList();

			var results = new List<bool>();
			for (int i = 1; i <= max; i++)
			{
				results.AddRange(from pair in pairs where i%pair.Divisor != 0 select !replacements[i - 1].Contains(pair.TextContent));
			}
		
			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithThreePairsFirstNull_ShouldReplaceNumbersInPairs()
		{
			var pairs = BuildMatchPairs(new[] { 2, 3, 4 }, new[] { null, "Fizz", "Buzz" });
			var results = CallMatchAndTestMultiples(pairs);
			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithThreePairsMiddleNull_ShouldReplaceNumbersInPairs()
		{
			var pairs = BuildMatchPairs(new[] { 2, 3, 4 }, new[] { "Fizz", null, "Buzz" });
			var results = CallMatchAndTestMultiples(pairs);
			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithThreePairsLastNull_ShouldReplaceNumbersInPairs()
		{
			var pairs = BuildMatchPairs(new[] { 2, 3, 4 }, new[] { "Fizz", "Buzz", null });
			var results = CallMatchAndTestMultiples(pairs);
			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithDuplicateKeysInPairs_ShouldNotThrowException()
		{
			var pairs = BuildMatchPairs(new[] { 2, 2 }, new[] { "Fizz", "Buzz" });
			Assert.DoesNotThrow(() => _sut.Match(pairs, 0).ToList());
		}
		
		[Test]
		public void Match_WithDuplicateKeysInPairs_ShouldIgnoreDuplicates()
		{
			var pairs = BuildMatchPairs(new[] { 2, 2 }, new[] { "Fizz", "Buzz" });
			const int max = 4;
			var replacements = _sut.Match(pairs, max).ToList();

			var results = new List<bool>();
			int multiplicationFactor = 1;
			int multiple;
			while ((multiple = pairs[0].Divisor * multiplicationFactor) <= max)
			{
				results.Add(replacements[multiple - 1] == pairs[0].TextContent);
				multiplicationFactor++;
			}

			Assert.IsTrue(results.TrueForAll(r => r));
		}

		[Test]
		public void Match_WithValidPairs_WithIterationsToZero_ShouldReturnEmptyCollection()
		{
			var pairs = BuildMatchPairs(new[] {2, 3}, new[] {"Fizz", "Buzz"});
			var replacements = _sut.Match(pairs, 0).ToList();
			Assert.IsNotNull(replacements);
			Assert.IsEmpty(_sut.Match(pairs, 0).ToList());
		}

		[Test]
		public void Match_WithValidPairs_WithIterationsToNegativeNumber_ShouldReturnEmptyCollection()
		{
			var pairs = BuildMatchPairs(new[] { 2, 3 }, new[] { "Fizz", "Buzz" });
			var replacements = _sut.Match(pairs, 0).ToList();
			Assert.IsNotNull(replacements);
			Assert.IsEmpty(_sut.Match(pairs, -1).ToList());
		}

		[Test]
		public void Match_WithCustomStrategy_WithValidPairs_ShouldUseCustomStrategy()
		{
			var pairs = BuildMatchPairs(new[] { 3 }, new[] { "Buzzez" });
			const int max = 10;

			Func<int, int, bool> matchWhenProductIsAMultipleOfFive = (i, matchNum) => (i * matchNum) % 5 == 0;
			var replacements = _sut.Match(pairs, max, matchWhenProductIsAMultipleOfFive).ToList();

			var results = new Dictionary<int,bool>();
			for (int i = 1; i <= 10; i++)
			{
				if(matchWhenProductIsAMultipleOfFive(i, pairs[0].Divisor))
					results.Add(i, replacements[i - 1] == pairs[0].TextContent);
			}

			Assert.IsTrue(results.Values.ToList().TrueForAll(r => r));
			Assert.AreEqual(results.Count(), 2);
			// should match at 5 and 10 only
			Assert.IsTrue(results.Keys.Any(k => k == 5));
			Assert.IsTrue(results.Keys.Any(k => k == 10));
		}





		private static MatchPair[] BuildMatchPairs(int[] numbers, string[] text)
		{
			return numbers.Select((t, i) => new MatchPair(t, text[i])).ToArray();
		}

		private List<bool> CallMatchAndTestMultiples(MatchPair[] pairs)
		{
			const int max = 12;
			var replacements = _sut.Match(pairs, max).ToList();

			var results = new List<bool>();
			foreach (var pair in pairs)
			{
				int multiplicationFactor = 1;
				int multiple;
				while ((multiple = pair.Divisor * multiplicationFactor) <= max)
				{
					results.Add(replacements[multiple - 1].Contains(pair.TextContent ?? ""));
					multiplicationFactor++;
				}
			}
			return results;
		}
	}
}
