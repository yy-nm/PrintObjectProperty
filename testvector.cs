using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mard.Tools;

namespace TestVector
{
	class Program
	{
		static void Main(string[] args)
		{
			testvector ();
		}

		public static void testvector()
		{
			Dictionary<string, string> testdict = new Dictionary<string, string>() {
				{ "1", "1"},
				{ "2", "1"},
				{ "3", "2"},
				{ "4", "3"},
				{ "5", "5"},
				{ "6", "8"},
			};
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testdict.GetType(), testdict));

			HashSet<string> testhashset = new HashSet<string>() {
				"12", "123", "ad", "fd", "qwe", "23",
			};
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testhashset.GetType(), testhashset));

			List<int> testlist = new List<int>() {
				1, 2, 10, 2, 5, 9, 22, 14, 16,
			};
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testlist.GetType(), testlist));

			LinkedList<int> testlinkedlist = new LinkedList<int>(testlist);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testlinkedlist.GetType(), testlinkedlist));

			Queue<string> testq = new Queue<string>(testhashset);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testq.GetType(), testq));

			Stack<int> tests = new Stack<int>(testlist);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(tests.GetType(), tests));

			SortedDictionary<string, string> testsd = new SortedDictionary<string, string>(testdict);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testsd.GetType(), testsd));

			SortedList<int, int> testsl = new SortedList<int, int>()
			{
				{ 1, 10},
				{ 10, 1},
				{55, 5},
				{ 5, 55},
				{1000, 101},
				{101, 1001},
			};
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testsl.GetType(), testsl));


			Console.WriteLine(ObjectPropertyPrinter.PrintProperty<int>(1));

			Dictionary<string, Dictionary<string, string>> test2dict = new Dictionary<string, Dictionary<string, string>> ();
			test2dict.Add ("test2", testdict);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(test2dict.GetType(), test2dict));

			Dictionary<string, Dictionary<string, Dictionary<string, string>>> test3dict = new Dictionary<string, Dictionary<string, Dictionary<string, string>>> ();
			test3dict.Add ("test3", test2dict);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(test3dict.GetType(), test3dict));

			List<Dictionary<string, string>> testldict = new List<Dictionary<string, string>> ();
			testldict.Add (testdict);
			Console.WriteLine(ObjectPropertyPrinter.PrintProperty(testldict.GetType(), testldict));

			object o = new object ();
			Console.WriteLine (ObjectPropertyPrinter.PrintProperty (o.GetType (), o));
		}
	}
}
