<Query Kind="Expression" />

// Anton: well done

void Main()
{
	string text = @"Mr. John Smith was born on 1999/12/12 bla hihi haha
	hoho was born Arsenal from London, founded in 1897, Mr. John Smith was born on 1999/12/12";
	List<IParseable> entities = Parser.ParseText(text, 
		new TextParser[] {Person.ParseFromText, FootballClub.ParseFromText});
	entities.RemoveDuplicates();
	foreach (var entity in entities)
		Console.WriteLine(entity.ToString());
}

static class ListHelper
{
// Anton: I suggest not to modify the original collection but return a copy
	public static void RemoveDuplicates<T>(this List<T> list)
	{
		List<T> unique = new List<T>();
		for (int i = 0; i < list.Count; i++)
		{
			bool was = false;
			foreach (T past_item in unique)
			{
				if (list[i].Equals(past_item))
				{
					was = true;
				}
			}
			if (!was)
			{
				unique.Add(list[i]);
			}
		}
		list.Clear();
		foreach (T item in unique)
			list.Add(item);
	}
}

interface IParseable {}

delegate IParseable[] TextParser(string text);

class Person : IParseable
{
	private string FirstName { get; set; }
	private string LastName { get; set; }
	
	public string FullName { get; private set; }
	public DateTime BirthDate { get; private set; }
	public int Age { get; private set; }
	public char Gender { get; private set; }

	public Person(string firstName, string lastName, string genderTitle, DateTime birthDate)
	{
		FirstName = firstName;
		LastName = lastName;
		
		Gender = (genderTitle == "Mr." ? 'm' : 'f');
		FullName = String.Concat(firstName, lastName);
		
		BirthDate = birthDate;
		Age = DateTime.Now.Year - birthDate.Year;
		DateTime date2 = new DateTime(birthDate.Year, DateTime.Now.Month, DateTime.Now.Day);
		if (birthDate > date2)
		{
			Age--;
		}
	}
	
	public override string ToString() 
	{
		return $"{LastName}, {FirstName} ({Gender}, {Age})";
	}
	
	public override bool Equals(object obj)
	{
		Person other = obj as Person;
		if (other == null)
			return false;
		else
			return (Gender == other.Gender && FullName == other.FullName && BirthDate == other.BirthDate);
	}
	
	public static Person[] ParseFromText(string text) 
	{
		const string regexPerson = @"(Mr\.|Mrs\.|Ms\.)\s(\w+)\s(\w+)\swas\sborn\son\s(\d{4}/\d\d/\d\d)";
		var personsMatches = Regex.Matches(text, regexPerson);
		Person[] people = new Person[personsMatches.Count];

		for (int i = 0; i < personsMatches.Count; i++) 
		{
			var match = personsMatches[i];
			
			try 
			{
				string genderTitle = match.Groups[1].Value;

				string firstName = match.Groups[2].Value;
				string lastName = match.Groups[3].Value;

				DateTime birthDate = DateTime.Parse(match.Groups[4].Value);

				people[i] = new Person(firstName, lastName, genderTitle, birthDate);
			}
			catch (IndexOutOfRangeException)
			{
				Console.Write("Check indexes of matches");
			}
			catch (ArgumentNullException)
			{
				Console.Write("No text to parse for date");
			}
			catch (FormatException)
			{
				Console.Write("Date could not be parsed");
			}
		}

		return people;
	}
}

class FootballClub : IParseable
{
	public string Name { get; private set; }
	public string OriginCity { get; private set; }
	public uint FoundationYear { get; private set; }

	public FootballClub(string name, string originCity, uint foundationYear)
	{
		Name = name;
		OriginCity = originCity;
		FoundationYear = foundationYear;
	}
	
	public override string ToString() 
	{
		return $"{Name}, {OriginCity}, est. {FoundationYear}";
	}
	
	public override bool Equals(object obj)
	{
		FootballClub other = obj as FootballClub;
		if (other == null)
			return false;
		else
			return (Name == other.Name && OriginCity == other.OriginCity && FoundationYear == other.FoundationYear);
	}
	
	public static FootballClub[] ParseFromText(string text) 
	{
		const string regexPerson = @"(\w+)\sfrom\s(\w+),\sfounded\sin\s(\d{4})";
		var clubsMatches = Regex.Matches(text, regexPerson);
		FootballClub[] clubs = new FootballClub[clubsMatches.Count];

		for (int i = 0; i < clubsMatches.Count; i++) 
		{
			var match = clubsMatches[i];
			
			try 
			{
				string name = match.Groups[1].Value;

				string originCity = match.Groups[2].Value;
				uint foundationYear = UInt32.Parse(match.Groups[3].Value);

				clubs[i] = new FootballClub(name, originCity, foundationYear);
			}
			catch (IndexOutOfRangeException)
			{
				Console.Write("Check indexes of matches");
			}
			catch (ArgumentNullException)
			{
				Console.Write("No text to parse for year");
			}
			catch (FormatException)
			{
				Console.Write("Year could not be parsed");
			}
		}

		return clubs;
	}
}

class Parser 
{
	public static List<IParseable> ParseText(string text, TextParser[] parsers)
	{
		List<IParseable> result = new List<IParseable>();
		foreach (var parser in parsers)
		{
			IParseable[] instances = parser(text);
			foreach (var instance in instances)
			{
				result.Add(instance);
			}
		}
		return result;
	}
}