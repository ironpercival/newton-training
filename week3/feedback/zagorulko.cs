<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

// Anton: good!

public interface IParsee : IEquatable<IParsee> { }

static class ExtensionMethods
{
    public static List<IParsee> RemoveDuplicates(this List<IParsee> list)
    {
        return new List<IParsee>(new HashSet<IParsee>(list));
    }
}

public class Parser
{
    public delegate List<IParsee> Subparser(string text);

    public static string ToString(List<IParsee> results)
    {
        var sb = new StringBuilder();
        foreach (var item in results)
            sb.AppendLine(item.ToString());
        return sb.ToString();
    }

    public static List<IParsee> Parse(string text, params Subparser[] subparsers)
    {
        var results = new List<IParsee>();
        foreach (var subparser in subparsers)
            results.AddRange(subparser(text));
        return results;
    }

    public static void WriteXmlDir(string path, List<IParsee> results)
    {
        foreach (var item in results) {
            var dirPath = Path.Combine(path, item.GetType().FullName);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var filePath = Path.Combine(dirPath,
                DateTime.Now.Ticks.ToString() + ".xml");

            using (var s = new FileStream(filePath, FileMode.Create)) {
                new XmlSerializer(item.GetType()).Serialize(s, item);
            }
        }
    }

    public static List<IParsee> ReadXmlDir(string path)
    {
        var results = new List<IParsee>();
        foreach (var dirPath in Directory.EnumerateDirectories(path)) {
            var type = Type.GetType(Path.GetFileName(dirPath));
            var xml = new XmlSerializer(type);
            foreach (var filePath in Directory.EnumerateFiles(dirPath)) {
                using (var s = new FileStream(filePath, FileMode.Open)) {
                    results.Add((IParsee)xml.Deserialize(s));
                }
            }
        }
        return results;
    }
}

public class Person : IParsee
{
    public enum GenderType {
        Male,
        Female
    }

    public struct FullName {
        public string FirstName;
        public string LastName;
        public FullName(string firstName, string lastName) {
            FirstName = firstName;
            LastName = lastName;
        }
    }

    public FullName Name;
    public DateTime BirthDate;
    [XmlAttribute]
    public GenderType Gender;

    [XmlIgnore]
    public int Age => (DateTime.Now - BirthDate).Days / 365;

    public Person() {}

    public Person(FullName name, DateTime birthDate, GenderType gender)
    {
        Name = name;
        BirthDate = birthDate;
        Gender = gender;
    }

    public override string ToString() {
        return $"{Name.FirstName} {Name.LastName} ({Gender}, Age: {Age})";
    }

    public override int GetHashCode() {
        return Tuple.Create(Name, Gender, Age).GetHashCode();
    }

    public bool Equals(IParsee other) {
        var p = other as Person;
        if (p == null)
            return false;
        var thisTuple = Tuple.Create(Name, BirthDate, Gender);
        var thatTuple = Tuple.Create(p.Name, p.BirthDate, p.Gender);
        return thisTuple.Equals(thatTuple);
    }

    private static readonly string _regex =
        @"(\w+)\s(\w+)\s\((\d{4}/\d\d/\d\d),\s(male|female)\)";

    public static List<IParsee> Parse(string text)
    {
        var results = new List<IParsee>();
        foreach (Match match in Regex.Matches(text, _regex)) {
            try {
                var name = new FullName
                    (match.Groups[1].Value, match.Groups[2].Value);
                var birthday = DateTime.Parse
                    (match.Groups[3].Value);
                var gender = (GenderType) Enum.Parse
                    (typeof(GenderType), match.Groups[4].Value, true);
                results.Add(new Person(name, birthday, gender));
            } catch (FormatException) {} // DateTime.Parse
        }
        return results;
    }
}

public class Company : IParsee
{
    public string Name;
    public string Field;
    public string Country;

    public Company() {}

    public Company(string name, string field, string country)
    {
        Name = name;
        Field = field;
        Country = country;
    }

    public override string ToString() {
        return $"{Name} ({Field}, {Country})";
    }

    public override int GetHashCode() {
        return Tuple.Create(Name, Field, Country).GetHashCode();
    }

    public bool Equals(IParsee other) {
        var p = other as Company;
        if (p == null)
            return false;
        var thisTuple = Tuple.Create(Name, Field, Country);
        var thatTuple = Tuple.Create(p.Name, p.Field, p.Country);
        return thisTuple.Equals(thatTuple);
    }

    private static readonly string _regex =
        @"([A-Z][\w-]*(\s+[A-Z][\w-]*)*)\s\(([\w\s]+),\s([A-Z]{2})\)";

    public static List<IParsee> Parse(string text)
    {
        var results = new List<IParsee>();
        foreach (Match match in Regex.Matches(text, _regex)) {
            try {
                results.Add(new Company(
                    match.Groups[1].Value,
                    match.Groups[3].Value,
                    new RegionInfo(match.Groups[4].Value).EnglishName
                ));
            } catch (ArgumentException) {} // RegionInfo
        }
        return results;
    }
}

public class FootballClub : IParsee
{
    public string Name;
    public string City;
    public string Country;
    public int EstYear;

    public FootballClub() {}

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

    public override int GetHashCode() {
        return Tuple.Create(Name, City, Country, EstYear).GetHashCode();
    }

    public bool Equals(IParsee other) {
        var p = other as FootballClub;
        if (p == null)
            return false;
        var thisTuple = Tuple.Create(Name, City, Country, EstYear);
        var thatTuple = Tuple.Create(p.Name, p.City, p.Country, p.EstYear);
        return thisTuple.Equals(thatTuple);
    }

    private static readonly string _regex =
        @"(FC\s[\w]+)\s\(([\w\s]+),\s([\w\s]+),\ses?t\.\s([\d]{4})\)";

    public static List<IParsee> Parse(string text)
    {
        var results = new List<IParsee>();
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
        Auchan (Retail, FR)";

        var results = Parser.Parse(text,
            new Parser.Subparser(Person.Parse),
            new Parser.Subparser(Company.Parse),
            new Parser.Subparser(FootballClub.Parse)
        ).RemoveDuplicates();

        Parser.WriteXmlDir("xml", results);
        var storedResults = Parser.ReadXmlDir("xml").RemoveDuplicates();

        Console.Write(Parser.ToString(storedResults));
    }
}
