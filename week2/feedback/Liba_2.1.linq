<Query Kind="Program" />


// Anton: good in general. See some comments below. I will add another version of mine later (using lambdas), so check it later if you are interested.

void Main()
{
	const string s1 = "Last year winner FC Dynamo (Kyiv, Ukraine, et. 1927) lost the game with 0:2 to FC Kryvbas (Kryvyi Rih, Ukraine, est. 1959)." +
	"Share price of Comp. Microsoft (Information Technology, US) dropped to $100" +
	"John Smith (1983/11/05, male) has been sentenced to a 3 year imprisonment for corruption John Smith (1983/11/05, male) FC Dynamo (Kyiv, Ukraine, et. 1927) and of course Comp. Microsoft (Information Technology, US)"; 
	
	AnyItems parsedElements = new AnyItems();
	
	// Delegates
	personDel op1 = new personDel(CreateObject.newPerson);
	footballDel op2 = new footballDel(CreateObject.newFootballClub);
	companyDel op3 = new companyDel(CreateObject.newCompany);
	
	// Set couples
	Dictionary<Regex, Delegate> rules = new Dictionary<Regex,Delegate>();
	rules.Add(Person.regexForm, op1);
	rules.Add(FootballClub.regexForm, op2);
	rules.Add(Company.regexForm, op3);
	
	// Parse
	parsedElements.Parse(s1, rules);
	
	parsedElements.RemoveDuplicates();
	
	parsedElements.PrintValues();
}

delegate Person personDel (string text);
delegate FootballClub footballDel (string text);
delegate Company companyDel (string text);

class AnyItems
{
	private ArrayList all;
	
	public AnyItems()
	{
		all = new ArrayList();
	}
	
	// Anton: you apply regex twice - once in foreach, then again in each delegate method.
	// Why do you use Delegate and not a Func<>/lamdba? 
	// Why do you restrict rule to be Regex -> Delegate? Why not just use a labmda and let the implementer decide how to implement it?
	public void Parse(string text, Dictionary<Regex, Delegate> rules) 
	{
		bool nothing = true;
		
		foreach(KeyValuePair<Regex, Delegate> i in rules)
		{
			Match match = i.Key.Match(text);
			
			if(match.Value != "")
			{
				all.Add(i.Value.DynamicInvoke(match.Value));
				nothing = false;
			}
			
			int start = text.IndexOf(match.Value);
			text = text.Remove(start, match.Value.Length);
		}
		
		if(nothing)
			return;
		else
			Parse(text, rules);
	}
	
	public void RemoveDuplicates()
	{
		if(all.Count == 0 || all.Count == 1) throw new InvalidOperationException("There is nohing to remove");
		for(int i = 0; i < all.Count; i++)
		{
			for(int j = i + 1; j < all.Count; j++)
			{
				//Console.WriteLine(all[i].ToString() + " " + all[i].Equals(all[j]) + " " + all[j].ToString()); 
				if( all[i].Equals(all[j]) ) all.RemoveAt(j);
			}
		}
	}
	
	public void PrintValues()
	{
		foreach (var i in all)
		{
		    Console.WriteLine(i.ToString());
		}
	}
}

// Anton: every time you add a new type of parsed entity, you'll need to modify this class. This is not cool.
// Why don't you put these delegate methods into each entity class?
class CreateObject
{
	protected CreateObject(){}
	
	static public Person newPerson(string text)
	{
		Match match = Person.regexForm.Match(text);
		
		DateTime dateOfBirth = DateTime.Parse(match.Groups[3].Value);
		
		Dictionary<string, GenderEnum> GenderMap = new Dictionary<string, GenderEnum>
		{
			["male"] = GenderEnum.Male,
			["female"] = GenderEnum.Female,
		};
		
		Person temp = new Person(match.Groups[1].Value, match.Groups[2].Value, dateOfBirth, GenderMap[match.Groups[4].Value]);
		return temp;
	}
	
	static public FootballClub newFootballClub(string text)
	{
		Match match = FootballClub.regexForm.Match(text);
		int year = Int32.Parse(match.Groups[4].Value);
		
		FootballClub temp = new FootballClub(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, year);
		return temp;
	}
		
	static public Company newCompany(string text)
	{
		Match match = Company.regexForm.Match(text);
		
		Company temp = new Company(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
		return temp;
	}
}

public struct FullName : IComparable<FullName>, IEquatable<FullName>
{
	public string FirstName { get; set; }
	public string LastName { get; set; }

	public int CompareTo(FullName other)
	{
		var thisName = $"{LastName}{FirstName}".ToLower();

		var otherName = $"{other.LastName}{other.FirstName}".ToLower();

		return thisName.CompareTo(otherName); 
	}

	public bool Equals(FullName other)
	{
		return FirstName == other.FirstName && LastName == other.LastName;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is FullName))
			return false;

		var other = (FullName)obj;

		return Equals(other);
	}

	public override int GetHashCode()
	{
		return Tuple.Create(FirstName, LastName).GetHashCode();
	}

	public static bool operator ==(FullName left, FullName right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FullName left, FullName right)
	{
		return !(left == right);
	}
}

public enum GenderEnum{
	Male,
	Female
}

public struct Gender : IComparable<Gender>, IEquatable<Gender>
{
	public GenderEnum Value { get; private set; }

	public int CompareTo(Gender other)
	{
		if (Value == other.Value)
			return 0;

		if (Value == GenderEnum.Female)
			return -1;

		return 1;
	}

	public Gender(GenderEnum gender)
	{
		Value = gender;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is Gender))
			return false;

		var other = (Gender)obj;

		return Equals(other);
	}

	public override string ToString()
	{
		return Value.ToString()[0].ToString(); 
	}

	public bool Equals(Gender other)
	{
		return Value == other.Value;
	}

	public static bool operator ==(Gender left, Gender right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Gender left, Gender right)
	{
		return !(left.Equals(right));
	}
	
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

class Person : IComparable<Person>
{
	public FullName Name { get; private set; }
	public DateTime BirthDate { get; private set; }
	public Gender Gender { get; private set; }
	static public Regex regexForm = new Regex(@"(\p{Lu}\w+)?\s(\p{Lu}\w+)?\s\((\d{4}/\d{2}/\d{2}),\s(\w+)?\)");

	public Person(string firstName, string lastName, DateTime birthDate, GenderEnum gender) 
	{
		Name = new FullName() { FirstName = firstName, LastName = lastName };
		BirthDate = birthDate;
		Gender = new Gender(gender);
	}
	
	public int Age => (DateTime.Now - BirthDate).Days / 365; 

	public override string ToString()
	{
		return $"{Name.LastName}, {Name.FirstName} ({Gender}, {Age})"; 
	}
	
	protected virtual bool CheckEquality(Person p)
	{
		return Name == p.Name && BirthDate == p.BirthDate && Gender == p.Gender;
	}
	
	public override bool Equals(object obj)
	{
		var other = obj as Person;

		return other != null && CheckEquality(other);
	}
	
	public int CompareTo(Person other)
	{
		if (!Gender.Equals(other.Gender))
			return Gender.CompareTo(other.Gender);

		if (!Name.Equals(other.Name))
			return Name.CompareTo(other.Name);

		if (BirthDate != other.BirthDate)
			return BirthDate.CompareTo(other.BirthDate);

		return 0;
	}
	
	public override int GetHashCode()
	{
		return Tuple.Create(Name, BirthDate, Gender).GetHashCode();
	}
}

public struct Location: IEquatable<Location>, IComparable<Location>
{
	public string City { get; set; }
	public string Country { get; set; }
	
	public bool Equals(Location other)
	{
		return City == other.City && Country == other.Country;
	}
	
	public int CompareTo(Location other)
	{
		if (!City.Equals(other.City))
			return City.CompareTo(other.City);

		if (!Country.Equals(other.Country))
			return Country.CompareTo(other.Country);

		return 0;
	}
	
	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is Location))
			return false;

		var other = (Location)obj;

		return Equals(other);
	}
	
	public override int GetHashCode()
	{
		return Tuple.Create(City, Country).GetHashCode();
	}

	public static bool operator ==(Location left, Location right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Location left, Location right)
	{
		return !(left.Equals(right));
	}
	
}

class FootballClub: IComparable<FootballClub>, IEquatable<FootballClub>
{
	public string Name { get; private set; }
	public int Creation { get; private set; }
	public Location Adress { get; private set; }
	static public Regex regexForm = new Regex(@"FC\s(\p{Lu}\w+)?\s\((\p{Lu}.+?),\s(\p{Lu}\w+).+?(\d{4})");
	
	public FootballClub(string name, string city, string country, int year)
	{
		Name = name;
		Adress = new Location() { City = city, Country = country};
		Creation = year;
	}
	
	public override string ToString()
	{
		return $"FC {Name}, ({Adress.City}, {Adress.Country}, founded in {Creation})"; 
	}
	
	protected virtual bool CheckEquality(FootballClub p)
	{
		return Name == p.Name && Creation == p.Creation && Adress == p.Adress;
	}
	
	public bool Equals(FootballClub other)
	{
		return Name == other.Name && Creation == other.Creation && Adress == other.Adress;
	}
	
	public override bool Equals(object obj)
	{
		var other = obj as FootballClub;

		return other != null && CheckEquality(other);
	}
	
	public int CompareTo(FootballClub other)
	{
		if (Name != other.Name)
			return Name.CompareTo(other.Name);

		if (Creation != other.Creation)
			return Creation.CompareTo(other.Creation);

		if (Adress != other.Adress)
			return Adress.CompareTo(other.Adress);

		return 0;
	}
	public override int GetHashCode()
	{
		return Tuple.Create(Name, Creation, Adress).GetHashCode();
	}
}

class Company : IComparable<Company>, IEquatable<Company>
{
	public string Name { get; private set; }
	public string Area { get; private set; }
	public string CountryCode { get; private set; }
	static public Regex regexForm = new Regex(@"Comp\.\s(\p{Lu}\w+?)\s\((\p{Lu}.+?),\s(\p{Lu}{2})\)");
	
	public Company(string name, string area, string countryCode)
	{
		Name = name;
		Area = area;
		CountryCode = countryCode;
	}	
	
	public bool Equals(Company other)
	{
		return Name == other.Name && Area == other.Area && CountryCode == other.CountryCode;
	}
	
	public int CompareTo(Company other)
	{
		if (Name != other.Name)
			return Name.CompareTo(other.Name);

		if (Area != other.Area)
			return Area.CompareTo(other.Area);

		if (CountryCode != other.CountryCode)
			return CountryCode.CompareTo(other.CountryCode);

		return 0;
	}
	
	protected virtual bool CheckEquality(Company p)
	{
		return Name == p.Name && Area == p.Area && CountryCode == p.CountryCode;
	}
	
	public override bool Equals(object obj)
	{
		var other = obj as Company;

		return other != null && CheckEquality(other);
	}
	
	public override int GetHashCode()
	{
		return Tuple.Create(Name, Area, CountryCode).GetHashCode();
	}
	
	public override string ToString()
	{
		return $"{Name} ({Area}, {CountryCode})"; 
	}
}