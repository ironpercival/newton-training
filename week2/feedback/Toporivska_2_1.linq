<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>

//Anton: good job! very impressive! see some comments below.

namespace CustomExtensions
{
	public static class ListExtension
	{
		public static List<T> RemoveDuplicates<T>(this List<T> list)
		{
			return list.Distinct().ToList();; //Anton: Cool!
		}
	}
}

namespace Parser
{
	using CustomExtensions;
	class MainClass
	{
		public static void Main (string[] args)
		{
		
			string text = "Last year winner FC Dynamo (Kyiv, Ukraine, est. 1927) lost the game with 0:2 to FC " +
				"Kryvbas (Kryvyi Rih, Ukraine, est. 1959). Share price of Microsoft (Information Technology, " +
				"US) Microsoft (Information Technology, US) dropped to FC Dynamo (Kyiv, Ukraine, est. 1927) Miss " +
				"Lana Webb was born on 1979/07/13 $100 Mr. John Smith was born on 1983/11/05, male)  Miss Lana " +
				"Webb was born on 1979/07/13 has been sentenced to a 3 year imprisonment for corruption " +
				"Microsoft (Information Technology, Miss Jessica White was born on 1879/12/31 US)\n\n// Mr. " +
				"John Smith was born on 1983/11/05 Example: Volkswagen Group (Automotive, DE) Example: " +
				"Auchan (Retail, FR) Auchan ( , fdb)  Wallmart (Retail, USA) Auchan (Retail, FR) Auchan " +
				"(Retail, FR) Auchan (Retail, FR) Ana Webb was born on 2020/31/06";
		
			Func<string,List<object>> parsers = ParserUtils.ParseAllOccurrences<CompanyParser>;
			parsers += ParserUtils.ParseAllOccurrences<FootballClubParser>;
			parsers += ParserUtils.ParseAllOccurrences<PersonParser>;
			List<object> res = ParserUtils.ParseEverything (text, parsers);

			Console.WriteLine ("\tBefore removing duplicates:");
			foreach (object i in res) {
				Console.WriteLine (i);
			}

			res = res.RemoveDuplicates ();

			Console.WriteLine ("\n\tAfter removing duplicates:");
			foreach (object i in res) {
				Console.WriteLine (i);
			}
		}
	}

// Anton: hardcore with delegates - that's impressive! A bit overkill of course, but really impressive! I mean that's good, but it could be done simpler :) Anyway, great you've learned and used all that stuff!
	public static class ParserUtils
	{
		public static List<object> ParseAllOccurrences<T>(string input) 
			where T : Parser, new(){
			T obj = new T (); 
			List<object> result = new List<object> ();
			foreach (Match match in Regex.Matches(input,obj.GetPattern())) {
				try{
					result.Add(obj.Parse(match.Value));
				} catch(Exception e){
					Console.WriteLine (e);
				}
			}
			return result;
		}

		public static List<object> ParseEverything(string input, Func<string, List<object>> parsers)
		{
			List<object> list = new List<object>();
			foreach (Func<string, List<object>> p in parsers.GetInvocationList()) {
				list.AddRange (p(input));
			}
			return new List<object>(list);
		}
	}

		// Anton: abstract class is ok here, though interface should be preferred for the case when you want to give more freedom to the implementers. For example, not to restric them to use Regexes. Anyway - you did it nice!
		// also, you could have made it generic, to parse and return a concrete type, not an object. Check my version.
	public abstract class Parser
	{	
		public abstract string Pattern {
			get;
		}

		public abstract object Parse (string input);

		public bool TryParse(string input)=> Regex.IsMatch (input, Pattern);

		public string GetPattern () => Pattern;

		protected void validateParam(string input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			if (!TryParse(input))
				throw new FormatException ("The input string does not match with the pattern.");
		}
	}

	public sealed class CompanyParser : Parser
	{
		public override string Pattern 
		{ 
			get
			{
				return "(\\w+)\\s\\(([\\w][\\w ]+),\\s(\\w+)\\)";
			}
		}		

		public override object Parse(string input)
		{
			validateParam (input);
			Match match = Regex.Match(input, Pattern);
			try{
				return new Company(match.Groups[1].Value,match.Groups[2].Value,match.Groups[3].Value);
			} catch(Exception){
				throw;
			}
		}
	}

	public sealed class FootballClubParser : Parser
	{
		public override string Pattern 
		{ 
			get
			{
				return "FC\\s(\\w+)\\s\\(([\\w][\\w ]+),\\s([\\w][\\w ]+),\\sest\\.\\s(\\d{4})\\)";	
			}
		}

		public override object Parse(string input)
		{
			validateParam (input);
			Match match = Regex.Match(input, Pattern);
			try{
				return new FootballClub(match.Groups[1].Value,match.Groups[2].Value,
					match.Groups[3].Value,Int32.Parse(match.Groups[4].Value));
			} catch(Exception){
				throw;
			}
		}
	}

	public sealed class PersonParser : Parser
	{
		public override string Pattern 
		{ 
			get
			{
				return "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )(\\w+)\\s(\\w+)\\swas\\sborn\\son\\s(\\d{4})/(\\d{2})/(\\d{2})";
			}
		}

		public override object Parse(string input)
		{
			validateParam (input);
			Match match = Regex.Match(input, Pattern);
			try {
				Gender g = match.Groups [1].Value == "Mr. " ? Gender.m : Gender.f;
				DateTime date = new DateTime (Int32.Parse(match.Groups[4].Value), 
					Int32.Parse(match.Groups[5].Value),Int32.Parse(match.Groups[6].Value));
				return new Person(match.Groups[2].Value, match.Groups[3].Value,date,g);
			} catch(Exception){
				throw;
			}
		}
	}

	public class Company : IComparable<Company>
	{
		public string Name { get; set; }
		public string Country { get; set; }
		public string FieldOfActivity { get; set; }

		public Company (string name, string field, string country)
		{
			Name = name;
			Country = country;
			FieldOfActivity = field;
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			var other = obj as Company;
			return Name.Equals(other.Name) && Country.Equals(other.Country) 
				&& FieldOfActivity.Equals(other.FieldOfActivity);
		}

		public override int GetHashCode(){
			int hash = 17;
			hash = hash * 31 + Name.GetHashCode();
			hash = hash * 31 + Country.GetHashCode();
			hash = hash * 31 + FieldOfActivity.GetHashCode();
			return hash;
		}

		public int CompareTo(Company other)
		{
			if (!Name.Equals (other.Name))
				return Name.CompareTo (other.Name);
			if (!Country.Equals (other.Country))
				return Country.CompareTo (other.Country);
			return FieldOfActivity.CompareTo (other.FieldOfActivity);
		}

		public override string ToString(){
			return string.Format ("{0} ({1}, {2})", Name, FieldOfActivity, Country);
		}
	}

	public class FootballClub : IComparable<FootballClub>
	{
		int yearOfFoundation;

		public string Name{ get; set;}
		public string City{ get; set;}
		public string Country{ get; set;}
		public int YearOfFoundation{ 
			get{ return yearOfFoundation;} 
			set{ 
				if (value < 0 || value > DateTime.Now.Year) {
					throw new ArgumentOutOfRangeException ("year of foundation");
				} else {
					yearOfFoundation = value;
				}
			}
		}

		public FootballClub (string name,string city,string country,int yearOfFoundation)
		{
			Name = name;
			City = city;
			Country = country;
			YearOfFoundation = yearOfFoundation;
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			var other = obj as FootballClub;
			return Name.Equals(other.Name) && Country.Equals(other.Country) 
				&& City.Equals(other.City) && YearOfFoundation.Equals(other.YearOfFoundation);
		}

		public override int GetHashCode(){
			int hash = 17;
			hash = hash * 31 + Name.GetHashCode();
			hash = hash * 31 + Country.GetHashCode();
			hash = hash * 31 + City.GetHashCode();
			hash = hash * 31 + YearOfFoundation.GetHashCode();
			return hash;
		}

		public int CompareTo(FootballClub other)
		{
			if (!Name.Equals (other.Name))
				return Name.CompareTo (other.Name);
			if (!Country.Equals (other.Country))
				return Country.CompareTo (other.Country);
			if (!City.Equals (other.City))
				return City.CompareTo(other.City);
			return YearOfFoundation.CompareTo (other.YearOfFoundation);
		}

		public override string ToString ()
		{
			return string.Format ("FC {0} ({1}, {2}, est. {3})", Name, City, Country, YearOfFoundation);
		}
	}
		
	public enum Gender {f, m};
	
	// Anton: IComparable - cool!
	public class Person : IComparable <Person>
	{
		string firstname, lastname;
		DateTime dateOfBirth;
		Gender gender;

		public string Firstname
		{
			get
			{ 
				return firstname;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException ("firstname");
				string trimmedValue = value.Trim ();
				if(trimmedValue.Length == 0) 
					throw new ArgumentOutOfRangeException ("firstname");
				firstname = trimmedValue;
			}
		}

		public string Lastname
		{
			get
			{ 
				return lastname;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException ("lastname");
				string trimmedValue = value.Trim ();
				if(trimmedValue.Length == 0) 
					throw new ArgumentOutOfRangeException ("lastname");
				lastname = trimmedValue;
			}
		}

		public DateTime DateOfBirth
		{
			get
			{ 
				return dateOfBirth;
			}
			set
			{
				if (value > DateTime.Now)
					throw new ArgumentOutOfRangeException ("dateOfBirth");
				dateOfBirth = value;
			}
		}

		public Gender Gender
		{
			get
			{
				return gender;
			}
			set
			{
				if ((int)value >= Enum.GetNames(typeof(Gender)).Length || (int)value < 0)
					throw new ArgumentOutOfRangeException ("gender");
				gender = value;
			}
		}

		public Person (string firstname, string lastname, DateTime dateOfBirth, Gender gender)
		{
			Firstname = firstname;
			Lastname = lastname;
			DateOfBirth = dateOfBirth;
			Gender = gender;
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;
			var other = obj as Person;
			return Gender.Equals(other.Gender) && Firstname.Equals(other.Firstname) 
				&& Lastname.Equals(other.Lastname) && DateOfBirth.Equals(other.DateOfBirth);
		}

		public override int GetHashCode()
		{	
			int hash = 17;
			hash = hash * 31 + firstname.GetHashCode();
			hash = hash * 31 + lastname.GetHashCode();
			hash = hash * 31 + gender.GetHashCode();
			hash = hash * 31 + dateOfBirth.GetHashCode();
			return hash;
		}

		public int CompareTo(Person other) {
			if (!Gender.Equals (other.Gender))
				return Gender.CompareTo (other.Gender);
			if (!Lastname.Equals (other.Lastname))
				return Lastname.CompareTo (other.Lastname);
			if (!Firstname.Equals (other.Firstname))
				return Firstname.CompareTo (other.Firstname);
			return DateOfBirth.CompareTo (other.DateOfBirth);
		}

		public override string ToString ()
		{
			int age = DateTime.Now.Year - DateOfBirth.Year;
			return string.Format ("{0}, {1} ({2}, {3})", Lastname, Firstname, Gender, age);
		}
	}
}
