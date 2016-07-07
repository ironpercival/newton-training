<Query Kind="Expression">
  <Namespace>System.IO</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

// Anton: Good overall approach! Though some implementation details and solutions are bad. Check some comments below.

class Person: IComparable <Person>, IEquatable<Person>{
    public struct Name : IComparable <Name>, IEquatable<Name>{

        public string LastName{get; private set;}
        public string FirstName{get; private set;}
        public Name(string lastName, string firstName){LastName = lastName; FirstName = firstName;}
        public bool Equals(Name other) {
            return FirstName == other.FirstName && LastName == other.LastName;
        }
        public int CompareTo (Name other){
            var thisName = $"{LastName}{FirstName}".ToLower();

            var otherName = $"{other.LastName}{other.FirstName}".ToLower();

            return thisName.CompareTo(otherName);
        }
        public override string ToString(){
            return LastName + " "+ FirstName;
        }
    }
    public Name FullName;
	
	// Anton: bool is not a proper type for this; bool should be used for values that are true/false naturally! Gender is not true false by it's nature. 
	// Male is not more true than female, so here is only your presumption, which might lead to misunderstanding and possible troubles.
    public bool Gender {get; private set;}
	
    private DateTime dateOfBirth;
    private int myAge;
    public Person(Person person){
        FullName = person.FullName;
        Gender = person.Gender;
        dateOfBirth = person.dateOfBirth;
    }
    public Person(){
        FullName  = new Name("", "");
        Gender = true;
        dateOfBirth = new DateTime(1900,1,1);
        Age();
    }
    public Person(string lastName, string firstName, bool gender, DateTime birthDate){
        FullName = new Name (lastName, firstName);
        this.Gender = gender;
        this.dateOfBirth = birthDate;
        Age(); // Anton: why do you set age with a method in constructor? Does not age change after that?
    }
	
	// Anton: this is crazy both for its purpose and for implementation
	// Use a simple getter for age:
	// public int Age => (DateTime.Now - BirthDate).Days / 365;
    private void Age(){
        DateTime now = DateTime.Today;
        myAge = now.Year - dateOfBirth.Year;
        if (now < dateOfBirth.AddYears(myAge)) {
            myAge--;
        }
    }
    public bool Equals (Person other){
        return (dateOfBirth.Equals(other.dateOfBirth) && Gender == other.Gender && FullName.Equals(other.FullName));
    }
    public int CompareTo (Person other){
        if(Gender != other.Gender){
            if(other.Gender){
                return -1;
            }
            else{
                return 1;
            }
        }
        if(!FullName.Equals(other.FullName)){
            return FullName.CompareTo(other.FullName);
        }
        if(dateOfBirth != other.dateOfBirth){
            return dateOfBirth.CompareTo(other.dateOfBirth);
        }
        return 0;
    }
    public override string ToString(){
	// Anton: use string.Format or $"" interpolated strings
        string res = FullName + "(";
            if(Gender){
                res += "m";
            } 
            else{
                res += "f";
            }
            res += ", " + myAge + ")";
return res;
}


public static  Person[] ExtractFromText(string text){
        var personsMatch = Regex.Matches(text, @"(Mrs|Mr)\s(\w+)\s(\w+)\swas\sborn\son\s([0-9]{4})/([0-9]{1,2})/([0-9]{1,2})");//check for Mr/Mrs firstName LastName
        var persons = new Person[personsMatch.Count];
        
        for(int i = 0; i<personsMatch.Count; i++){
            var match = personsMatch[i];
			// Anton: DateTime can be parsed from string
			// See DateTime.Parse()
            string year = match.Groups[4].Value;
            string month = match.Groups[5].Value;
            string day = (match.Groups[6].Value);
            int years = (year[0]-48) * 1000 + (year[1]-48) * 100 + (year[2]-48) * 10 + (year[3]-48);
            int months = 0;
            if(month.Length == 1){ // checking whether month is 2-digital number
                months = month[0] - 48;
            }
            else{
                months = (month[0]-48)*10 + month[1]-48;
            }
            int days = 0;
            if(day.Length == 1){ // checking whether day is 2-digital number
                days = day[0] - 48;
            }
            else{
                days = (day[0]-48)*10 + day[1] - 48;
            }
            DateTime birthDate = new DateTime(years, months, days);
            Person person = new Person(match.Groups[2].Value, match.Groups[3].Value, match.Groups[1].Value == "Mr", birthDate);
            persons[i] = person;
        }
        return persons;
    }

}
class FootballClub: IEquatable<FootballClub>{
    public string Name{get; private set;}
    public string City {get; private set;}
    public string Country {get; private set;}
    public int Year {get; private set;}

    public FootballClub (string name, string city, string country, int year){
        Name = name;
        City = city;
        Country = country;
        Year = year;
    }
    public static FootballClub[] ExtractFromText(string text){
        var footballClubsMatch = Regex.Matches(text, @"FC\s(\w+)\s(\w+)\s(\w+)\s([0-9]{4})");
        var clubs = new FootballClub[footballClubsMatch.Count];
        for(int i = 0; i<footballClubsMatch.Count; i++){
            var match = footballClubsMatch[i];
            string year = match.Groups[4].Value;
            int years = (year[0]-48) * 1000 + (year[1]-48) * 100 + (year[2]-48) * 10 + (year[3]-48);
            FootballClub club = new FootballClub(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, years);
            clubs[i] = club;
        }
        return clubs;
    }
    public bool Equals(FootballClub other){
        return (Name.Equals(other.Name) && City.Equals(other.City) && Country.Equals(other.Country));
    }
    public override string ToString(){
        return "FC " + Name + " " + "(" + City + ", " + Country + ") " + Year;
    }
}

// Anton: object[] is bad; you should use a strictly typed result like abstract class or interface
// You would notice this flaw if you'd implement duplicates removal. In your case, how would you remove duplicates for an arbitrary object?
public delegate object[] Extract(string text);
class Parser{
    public static List<object> Parse(string text, List<Extract> ex){
        List<object>res = new List<object>();
        foreach(Extract e in ex){
            object[] objects = e(text);
            foreach(object o in objects){
                res.Add(o);
            }
        }
        if(res.Count == 0){
            Console.WriteLine("There is no possible objects to extract");
        }
        return res;
    }
}
class Program
{
    static void Main()
    {
         string text1 = "Mrs Anniston Jennifer was born on 1969/2/11, Mr Statham Jason was born on 1967/7/26, Mrs Cruise Peneloppe was born on 1974/5/28, Mr Cruise Tom was born on 1962/3/7, Mr Cobaine Curt was born on 1967/2/20";
        string text2 = " FC Dynamo Kyiv Ukraine 1927, FC Shakhtar Donetsk Ukraine 1956";
        string text = text1 + text2;
        List<Extract> functions =  new List <Extract>{Person.ExtractFromText, FootballClub.ExtractFromText};
        List<object> results = Parser.Parse(text, functions);
        foreach (object p in results){
            Console.WriteLine(p);
        }
    }
}
