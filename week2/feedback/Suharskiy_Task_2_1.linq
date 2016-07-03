<Query Kind="Program" />


// Anton: pretty good overall, this is ok in general. But Func<> or lamdas are preferred over Delegate class. Also, it is not safe and convenient to get a List<object> when parsed, List<IParsable> or List<ParsedEntity> would be better.

void Main()
{
	const string clubReg = @"(FC\.|AS\.)\s(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\d{4})";
	const string personReg = @"(Mr\.|Mrs\.|Ms\.)\s(\w+)\s(\w+)\swas\sborn\son\s(\d{4}/\d{2}/\d{2})";
	const string companyReg=@"(\w+\s+)*?(\w+)\s+\(([^,]*)[^)]([^)]*)\)(\w+\s+)*?";
	
	
	var objectsParser = new Parser();
	
	var clubsParser=new FindClub(FootballClub.CreateClub);
	var personParser=new FindPerson(Person.CreatePerson);
	var companyParser=new FindCompany(Company.CreateCompany);
	
	Dictionary<string, System.Delegate> delDictionary = new Dictionary<string, System.Delegate>{
		[personReg]=personParser,
		[clubReg]=clubsParser,
		[companyReg]=companyParser
	};
	
	string text="Mr. John Smith was born on 2001/11/03. Mrs. Jessica Brown was born on 1999/12/31. Mr. John Smith was born on 2001/11/03. Last year winner FC. Dynamo from Kyiv Ukraine 1927 won against AS. Milan from Milan Italy 1899. Company Microsoft (Information Technology, US) was closed. Share price of Microsoft (Information Technology, US) dropped to $100. Company Google (IT, US) bought a new product."
	;
	
	var objects = objectsParser.Parse(text, delDictionary);
	
	Console.WriteLine("Before:");
	ShowArr(objects);
	
	objects=objectsParser.RemoveD(objects);
	Console.WriteLine("\nAfter:");
	ShowArr(objects);
}

delegate Person FindPerson(Match match);
delegate FootballClub FindClub(Match match);
delegate Company FindCompany(Match match);

class Parser
{
	public List<object> Parse(string text, Dictionary<string, Delegate> delDictionary)
	{
		var objectsList = new List<object>();
		foreach(KeyValuePair<string, Delegate> entry in delDictionary)
		{
    		var objectsMatches = Regex.Matches(text, entry.Key);
			for (int i = 0; i < objectsMatches.Count; i++) {
				Match match = objectsMatches[i];
				objectsList.Add(delDictionary[entry.Key].DynamicInvoke(match));
			}
		}
		return objectsList;
	}
	public List<object> RemoveD(List<object> list)
	{
		var objectsList=new List<object>();
		objectsList.Add(list.First());
		foreach(object obj in list)
		{
			if(!IsIn(objectsList,obj))
				objectsList.Add(obj);
		}
		return objectsList;
	}
	public bool IsIn(List<object> list, object obj)
	{
		foreach(object item in list)
		{
			if(obj.Equals(item))
			{
				return true;
			}
		}
		return false;
	}
}


void ShowArr(List<object> arr)
{
	foreach (object i in arr)
	{
		Console.WriteLine("{0}", i);
    }
}

class Name:IComparable<Name>
{
	private string firstName;
	private string lastName;
	public Name(string fN, string lN)
	{
		firstName=fN;
		lastName=lN;
	}
	public Name()
	{
		firstName="";
		lastName="";
	}
	public string fName
    {
        get	{  return firstName; }
        set { firstName = value; }
    }
	public string lName
    {
        get	{  return lastName; }
        set { lastName = value; }
    }
	public int CompareTo(Name other) {
		var thisName = $"{lName}{fName}".ToLower(); 
		var otherName = $"{other.lName}{other.fName}".ToLower();
		return thisName.CompareTo(otherName); 
	}
	public bool Equals(Name other) {
		return fName == other.fName && lName == other.lName;
	}
	public override bool Equals(object obj) {
		if (obj == null || !(obj is Name))
			return false;
		var other = (Name)obj;
		return Equals(other);
	}
	public override int GetHashCode() {
		return Tuple.Create(fName, lName).GetHashCode();
	}
	
	
}
class Person: IComparable<Person>
{
    public Name fullName;
    private char myGender = 'm';
	private DateTime birthDate;
	private int myAge = 0;
	
	
	public Person(string firstName, string lastName, char gender, DateTime data)
	{
		fullName=new Name(firstName, lastName);
		birthDate=new DateTime(data.Year,data.Month,data.Day);
		myGender=gender;
		myAge=getAge(data);
	}
	
	public int getAge(DateTime data)
	{	
		DateTime today = DateTime.Today;
		if(today.Month>=data.Month && today.Day >= data.Day)
			return today.Year-data.Year;
		return today.Year-data.Year-1;
	}
	public char Gender
	{
		get { return myGender; }
		set { myGender=value; }
	}
	public string firstName
	{
		get { return fullName.fName; }
		set { fullName.fName=value; }
	}
	public string lastName
	{
		get { return fullName.lName; }
		set { fullName.lName=value; }
	}
	public DateTime Date
	{
		get { return birthDate; }
		set { birthDate=value; }
	}
    public int Age
    {
        get {   return myAge; }
        set {   myAge = value; }
    }
	protected virtual bool EqualPersons(Person i)
	{	
		return firstName==i.firstName && lastName==i.lastName && Gender==i.Gender && Date==i.Date;
	}
	public override bool Equals(object obj) {
		var other = obj as Person;
		
		return other != null && EqualPersons(other); 
	}
	public int CompareTo(Person other) {
		if (!Gender.Equals(other.Gender))
			return Gender.CompareTo(other.Gender);

		if (!fullName.Equals(other.fullName))
			return fullName.CompareTo(other.fullName);

		if (Date != other.Date)
			return Date.CompareTo(other.Date);

		return 0;
	}
	public override int GetHashCode() {
		return Tuple.Create(fullName, Date, Gender).GetHashCode();
	}
	static public Person CreatePerson(Match match)
	{			
		var gender = (match.Groups[1].Value=="Mr.")?'m':'f';
		var firstName = match.Groups[2].Value;
		var lastName = match.Groups[3].Value;
		var birthDate = DateTime.Parse(match.Groups[4].Value);
		
		return new Person(firstName,lastName,gender,birthDate);
	} 
	
	public override string ToString()
    {
        return fullName.lName +", "+ fullName.fName + " ( " + Gender +", "+ Age+ " ) ";
    }
}

class FootballClub: IComparable<FootballClub>
{
	private string shortCode;
	private string name;
	private string country;
	private string city;
	private int year;
	
	public FootballClub(string shortCode,string name, string country, string city, int year)
	{
		ShortCode=shortCode;
		FCName=name;
		Country=country;
		City=city;
		Year=year;
	}
	public string ShortCode
	{
		get{ return this.shortCode; }
		private set{ this.shortCode=value; }
	}
	public string FCName
	{
		get{ return this.name; }
		private set{ this.name=value; }
	}
	public string Country
	{
		get{ return this.country; }
		private set{ this.country=value; }
	}
	public string City
	{
		get{ return this.city; }
		private set{ this.city=value; }
	}
	public int Year
	{
		get{ return this.year; }
		private set{ this.year=value; }
	}
	protected virtual bool EqualClubs(FootballClub i)
	{	
		return FCName==i.FCName && City==i.City && Country==i.Country && Year==i.Year;
	}
	public override bool Equals(object obj) {
		var other = obj as FootballClub;
		
		return other != null && EqualClubs(other); 
	}
	public int CompareTo(FootballClub other) {
		if (!Year.Equals(other.Year))
			return Year.CompareTo(other.Year);

		if (!FCName.Equals(other.FCName))
			return FCName.CompareTo(other.FCName);

		if (Country != other.Country)
			return Country.CompareTo(other.Country);
		if (City != other.City)
			return City.CompareTo(other.City);
		return 0;
	}
	public override int GetHashCode() {
		return Tuple.Create(FCName, Country, City,Year).GetHashCode();
	}
	static public FootballClub CreateClub(Match match)
	{	
		var sc=match.Groups[1].Value;
		var name = match.Groups[2].Value;
		var country = match.Groups[5].Value;
		var city = match.Groups[4].Value;
		var year = Int32.Parse(match.Groups[6].Value);
		
		return new FootballClub(sc,name,country,city,year);
	} 
	public override string ToString()
    {
        return $"{this.ShortCode} {this.FCName} ( {this.City} , {this.Country}, {this.Year} )";
    }
	
}


class Company: IComparable<Company>
{
	private string name;
	private string industry;
	private string code;
	
	public Company(string name, string industry, string code)
	{
		companyName=name;
		industryName=industry;
		companyCode=code;
	}
	public string companyName
	{
		get{ return this.name; }
		private set{ this.name=value; }
	}
	public string industryName
	{
		get{ return this.industry; }
		private set{ this.industry=value; }
	}
	public string companyCode
	{
		get{ return this.code; }
		private set{ this.code=value; }
	}
	
	protected virtual bool EqualCompanies(Company i)
	{	
		return companyName==i.companyName && industryName==i.industryName && companyCode==i.companyCode;
	}
	public override bool Equals(object obj) {
		var other = obj as Company;
		
		return other != null && EqualCompanies(other); 
	}
	
	public int CompareTo(Company other) {
		if (!companyName.Equals(other.companyName))
			return companyName.CompareTo(other.companyName);
		return 0;
	}
	public override int GetHashCode() {
		return Tuple.Create(companyName,industryName , companyCode).GetHashCode();
	}
	static public Company CreateCompany(Match match)
	{	
		var name=match.Groups[2].Value;
		var ind = match.Groups[3].Value;
		var code = match.Groups[4].Value;
		
		return new Company(name,ind,code);
	} 
	
	public override string ToString()
    {
        return $"{this.companyName} ( {this.industryName}, {this.companyCode} )";
    }
	
}