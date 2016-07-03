<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>


// Anton: ok, pretty good in general, though specifically types structures like List<> and IComparable<T> would make it even better

static class ExtensionMethods
{
	public static List<object> RemoveDuplicates(this List<object> list)
	{
		return new List<object>(new HashSet<object>(list));
	}
}

class Parser
{
// Anton: there is not much use of List<object>. You'd better use a List<T> of specific type T.
	public delegate List<object> Subparser(string text);

	public static List<object> Parse(string text, params Subparser[] subparsers)
	{
		var results = new List<object>();
		foreach (var subparser in subparsers)
			results.AddRange(subparser(text));
		return results;
	}

	public static string ToString(List<object> results)
	{
		var sb = new StringBuilder();
		foreach (var item in results)
			sb.AppendLine(item.ToString());
		return sb.ToString();
	}
}

// Anton: IComparable<T> is preferred
class Person : IComparable
{
	public enum GenderType {
		Male,
		Female
	}

	public string FirstName;
	public string LastName;

	public DateTime BirthDate;
	public int Age => (DateTime.Now - BirthDate).Days / 365;

	public GenderType Gender;

	public Person(string firstName, string lastName,
		DateTime birthDate, GenderType gender)
	{
		FirstName = firstName;
		LastName = lastName;
		BirthDate = birthDate;
		Gender = gender;
	}

	public override string ToString() {
		return $"{FirstName} {LastName} ({Gender}, Age: {Age})";
	}

	public override bool Equals(object obj) {
		return obj != null && CompareTo(obj) == 0;
	}

	public override int GetHashCode() {
		return Tuple.Create(FirstName, LastName, Gender, Age).GetHashCode();
	}

	public int CompareTo(object other) {
		// why not? :)
		// Anton: because it'd be silly :)
		return ToString().CompareTo(other.ToString());
	}

	private static readonly string _regex =
		@"(\w+)\s(\w+)\s\((\d{4}/\d\d/\d\d),\s(male|female)\)";
	
	public static List<object> Parse(string text)
	{
		var results = new List<object>();
		foreach (Match match in Regex.Matches(text, _regex)) {
			try {
				results.Add(new Person(
					match.Groups[1].Value,
					match.Groups[2].Value,
					DateTime.Parse(match.Groups[3].Value),
					(GenderType) Enum.Parse(typeof(GenderType),
						match.Groups[4].Value, true)
				));
			} catch (FormatException) {} // DateTime.Parse
		}
		return results;
	}
}

class Company : IComparable
{
	public string Name;
	public string Field;
	public RegionInfo Country;

	public Company(string name, string field, RegionInfo country)
	{
		Name = name;
		Field = field;
		Country = country;
	}

	public override string ToString() {
		return $"{Name} ({Field}, {Country.EnglishName})";
	}

	public override bool Equals(object obj) {
		return obj != null && CompareTo(obj) == 0;
	}

	public override int GetHashCode() {
		return Tuple.Create(Name, Field, Country).GetHashCode();
	}

	public int CompareTo(object other) {
		return ToString().CompareTo(other.ToString());
	}

	private static readonly string _regex =
		@"([A-Z][\w-]*(\s+[A-Z][\w-]*)*)\s\(([\w\s]+),\s([A-Z]{2})\)";

	public static List<object> Parse(string text)
	{
		var results = new List<object>();
		foreach (Match match in Regex.Matches(text, _regex)) {
			try {
				results.Add(new Company(
					match.Groups[1].Value,
					match.Groups[3].Value,
					new RegionInfo(match.Groups[4].Value)
				));
			} catch (ArgumentException) {} // RegionInfo
		}
		return results;
	}
}

class FootballClub : IComparable
{
	public string Name;
	public string City;
	public string Country;
	public int EstYear;

	public FootballClub(string name, string city, string country, int estYear)
	{
		Name = name;
		City = city;
		Country = country;
		EstYear = estYear;
	}

	public override string ToString() {
		return $"{Name} ({City}, {Country}, est. {EstYear})";
	}

	public override bool Equals(object obj) {
		return obj != null && CompareTo(obj) == 0;
	}

	public override int GetHashCode() {
		return Tuple.Create(Name, City, Country, EstYear).GetHashCode();
	}

	public int CompareTo(object other) {
		return ToString().CompareTo(other.ToString());
	}

	private static readonly string _regex =
		@"(FC\s[\w]+)\s\(([\w\s]+),\s([\w\s]+),\ses?t\.\s([\d]{4})\)";

	public static List<object> Parse(string text)
	{
		var results = new List<object>();
		foreach (Match match in Regex.Matches(text, _regex)) {
			results.Add(new FootballClub(
				match.Groups[1].Value,
				match.Groups[2].Value,
				match.Groups[3].Value,
				int.Parse(match.Groups[4].Value)
			));
		}
		return results;
	}
}

class Program
{
	public static void Main(string[] args)
	{
		var text = @"Last year winner FC Dynamo (Kyiv, Ukraine, et. 1927) lost the game with 0:2 to FC Kryvbas (Kryvyi Rih, Ukraine, est. 1959).
		Share price of Microsoft (Information Technology, US) dropped to $100
		John Smith (1983/11/05, male) has been sentenced to a 3 year imprisonment for corruption

		John Smith (1983/11/05, male)
		FC Dynamo (Kyiv, Ukraine, est. 1927)
		Microsoft (Information Technology, US)
		Volkswagen Group (Automotive, DE)
		Auchan (Retail, FR)

		Entities with invalid dates or country codes are just omitted (thx to try/catch):
		Joe Bloggs (1234/56/78, male)
		Umbrella Corporation (Pharma, ZZ)
		";
		
		var results = Parser.Parse(text,
			new Parser.Subparser(Person.Parse),
			new Parser.Subparser(Company.Parse),
			new Parser.Subparser(FootballClub.Parse)
		).RemoveDuplicates();

		results.Sort();

		Console.Write(Parser.ToString(results));
	}
}
