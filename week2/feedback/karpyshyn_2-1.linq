<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

namespace UkmaParser
{

// Anton: pretty good idea about the design; but the program looks incomplete.

//Anton: can be, but interface would be better
	public abstract class Parseble : IComparable<Parseble>
	{
		//public abstract string GetPattern();
		public abstract int CompareTo(Parseble another);
	}

	public interface IParser
	{
		Parseble ParseToObject(string text);
	}

	public class Person : Parseble
	{
		private readonly string first_name;
		private readonly string last_name; 

		public Person(string first_name, string last_name)
		{
			this.first_name = first_name;
			this.last_name = last_name;
		}

		/*     public static override string GetPattern()*/
		//{
		//return "My name is %first_name% %last_name%";
		/*}*/

		public override int CompareTo(Parseble another)
		{
			if (another == null)
			{
				return 0;
			}

			Person anotherPerson = another as Person;
			if (this.last_name.CompareTo(anotherPerson.last_name) > 0)
			{
				return 1;
			}
			else if (this.last_name.CompareTo(anotherPerson.last_name) < 0)
			{
				return -1;
			}
			else
			{
				if (this.first_name.CompareTo(anotherPerson.first_name) > 0)
				{
					return 1;
				}
				else if (this.first_name.CompareTo(anotherPerson.first_name) < 0)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}

	}

	public class PersonParser : IParser
	{
		private const string personExpression = "Person [%first_name%, %last_name%]";
		private string[] expressionParts;

		private static PersonParser instance;

		private PersonParser()
		{
			expressionParts = personExpression.Split(new char[]{'%'});
		}

		public static PersonParser Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new PersonParser();
				}
				return instance;
			}
		}

		public Parseble ParseToObject(string text)
		{
			string[] textParts = text.Split(new char[]{'%'});
			string[] personArgs = new string[typeof(Person).GetProperties().Length];
			if (textParts.Length != expressionParts.Length)
			{
				return null;
			}
			for (int i = 0, argsIndex = 0; i < expressionParts.Length; ++i)
			{
				if (textParts[i].Equals("first_name"))
				{
					personArgs[argsIndex] = textParts[i];
					continue;
				}
				if (!textParts[i].Equals(expressionParts[i]))
				{
					return null;
				}
			}
			return new Person(personArgs[0], personArgs[1]);
		}
	}

	public class CarParser : IParser
	{
	// Anton: never ever do this! Use regular expressions instead!
		private const string carExpression = "Car {%producer% %model%} with id = %id%";

		private static CarParser instance;

		private CarParser() {}

		public static CarParser Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CarParser();
				}
				return instance;
			}
		}

		public Parseble ParseToObject(string text)
		{
			return null;
		}
	}

	public class GlobalParser
	{
		public List<Object> ParseTextToObjects(string text, HashSet<IParser> parsers)
		{
			foreach (IParser parser in parsers)
			{
			}
			return null;
		}
	}

	public class ParserTester
	{
		static void Main(string[] args)
		{
			//IParser person = new PersonParser();
			Parseble person = new Person("Luke", "Skywalker");
			Parseble person2 = new Person("Luke", "Skywalker");
			//Console.WriteLine(person.CompareTo(person2));
			string text = "Person %class% yeah!";
			string[] parts = text.Split(new char[]{'%'});
			foreach (string part in parts)
			{
				Console.WriteLine(part);
			}
		}
	}
}
