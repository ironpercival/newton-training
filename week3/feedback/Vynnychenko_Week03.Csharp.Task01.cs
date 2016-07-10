<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

// Anton: ok, pretty good!

namespace Week03.Csharp.Task01
{
    public enum PersonGender
    {
        [XmlEnum(Name = "f")]
        Female,
        [XmlEnum(Name = "m")]
        Male
    };

    public interface IParsable : IEquatable<IParsable> { }

    public delegate IParsable[] ParserDelegate(string text);

    public class Person : IParsable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [XmlAttribute(AttributeName = "gender")]
        public PersonGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        [XmlIgnore]
        public int Age
        {
            get { return (DateTime.Today - BirthDate).Days / 365; }
        }

        public Person() { }

        public Person(string firstName, string lastName, PersonGender gender, DateTime birthDate)
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            BirthDate = birthDate;
        }

        public override string ToString()
        {
            var genderString = Gender == PersonGender.Male ? "m" : "f";
            return $"{LastName}, {FirstName} ({genderString}, {Age})";
        }

        public void ToConsole() => Console.Write(this.ToString());

        public static Person[] Parse(string text)
        {
            var matches = Regex.Matches(text, @"(?<gender>(Mr|Ms|Mrs))\.\s*(?<firstName>\w+)\s+(?<lastName>\w+)\s+was\s+born\s+on\s+(?<year>\d{4})/(?<month>\d{1,2})/(?<day>\d{1,2})");
            var persons = new Person[matches.Count];
            int i = 0;
            foreach (Match match in matches)
            {
                PersonGender gender;
                switch (match.Groups["gender"].Value)
                {
                    case "Mr": gender = PersonGender.Male; break;
                    default: gender = PersonGender.Female; break;
                }
                var birthDate = new DateTime(int.Parse(match.Groups["year"].Value), int.Parse(match.Groups["month"].Value), int.Parse(match.Groups["day"].Value));
                persons[i++] = new Person(match.Groups["firstName"].Value, match.Groups["lastName"].Value, gender, birthDate);
            }
            return persons;
        }

        public bool Equals(IParsable other)
        {
            if (other is Person)
            {
                var person = (Person)other;
                return ((FirstName.ToLower() == person.FirstName.ToLower()) && (LastName.ToLower() == person.LastName.ToLower()) &&
                    (Gender == person.Gender) && (BirthDate == person.BirthDate));
            }
            return false;
        }
    }

    public class Company : IParsable
    {
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "ownership-form")]
        public string OwnershipForm { get; set; }
        public int NumberOfEmployees { get; set; }

        public Company() { }

        public Company(string name, string ownershipForm = "", int numberOfEmployees = 0)
        {
            Name = name;
            OwnershipForm = ownershipForm;
            NumberOfEmployees = numberOfEmployees;
        }

        public static Company[] Parse(string text)
        {
            var matches = Regex.Matches(text, @"company\s+(?<name>[\w\s]+)\s*,\s*based", RegexOptions.IgnoreCase);
            var companies = new Company[matches.Count];
            int i = 0;
            foreach (Match match in matches)
            {
                companies[i++] = new Company(match.Groups["name"].Value);
            }
            return companies;
        }

        public override string ToString() => $"{Name} {OwnershipForm}: {NumberOfEmployees} employees";

        public bool Equals(IParsable other)
        {
		    // Anton: Just a hint: I would write it this way. It's easier to comprehend and even requires less operations :)
			// var company = other as Company;
			// return company!=null && Name == company.Name && OwnershipForm == company.OwnershipForm && NumberOfEmployees == company.NumberOfEmployees;
			
		
            if (other is Company)
            {			
                if (Name == ((Company)other).Name && OwnershipForm == ((Company)other).OwnershipForm && NumberOfEmployees == ((Company)other).NumberOfEmployees) { return true; } }
            return false;
        }
    }

    public static class Extensions
    {
        public static IParsable[] GetCopyWithoutDuplicates(this IParsable[] parsables)
        {
            var distinctParcables = new IParsable[parsables.Length];
            int distinctParcablesCounter = 0;
            bool isItemDistict;
            for (int i = 0; i < parsables.Length; i++)
            {
                isItemDistict = true;
                int k = i + 1;
                while (k < parsables.Length)
                {
                    if (parsables[i].Equals(parsables[k++]))
                    {
                        isItemDistict = false;
                        break;
                    }
                }
                if (isItemDistict) { distinctParcables[distinctParcablesCounter++] = parsables[i]; }
            }
            Array.Resize(ref distinctParcables, distinctParcablesCounter);
            return distinctParcables;
        }
    }

    public static class NiUtils
    {
        public static IParsable[] Parse(string text, params ParserDelegate[] parsers)
        {
            var parsables = new List<IParsable>();
            foreach (var parser in parsers)
            {
                try
                {
                    parsables.AddRange(parser(text));
                }
                catch (ParserException e)
                {
                    Console.WriteLine("ERROR: Parse error at " + e.PositionInText + " character\n");
                    continue;
                }
            }
            return parsables.ToArray();
        }

        public static string SerializeToString(IParsable iparsable)
        {
            var xmlSerializer = new XmlSerializer(iparsable.GetType());
            string xmlString;
            using(var sw = new StringWriter())
            using(var writer = XmlWriter.Create(sw))
            {
                xmlSerializer.Serialize(writer, iparsable);
                xmlString = sw.ToString();
            }
            return xmlString;
        }

        public static IParsable DeserializeFromString(string xmlString, Type type)
        {
            var xmlSerializer = new XmlSerializer(type);
             IParsable parsable;
            using (var sr = new StringReader(xmlString))
            {
                parsable = (IParsable)xmlSerializer.Deserialize(sr);
            }
            return parsable;
        }

        public static void SerializeToXmlFiles(IEnumerable<IParsable> parsables, string path)
        {
            if ( !String.IsNullOrEmpty(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }
            foreach (var parsable in parsables)
            {
                string xmlString = SerializeToString(parsable);
                Directory.CreateDirectory(path + parsable.GetType().FullName);
                File.WriteAllText(path + parsable.GetType().FullName + Path.DirectorySeparatorChar + Guid.NewGuid().ToString() + ".xml", xmlString);
            }
        }

        public static IEnumerable<IParsable> DeserializeFromXmlFiles(string path)
        {
            var directories = Directory.EnumerateDirectories(path);
            var result = new List<IParsable>();
            foreach (var directory in directories)
            {
                var type = Type.GetType(Path.GetFileName(directory));
                var files = Directory.EnumerateFiles(directory);
                foreach (var file in files) { result.Add(DeserializeFromString(File.ReadAllText(file), type)); }
            }
            return result;
        }

        public static IEnumerable<IParsable> SilentDeserializeFromXmlFiles(string path, out int deserializeFailCount)
        {
            int failCount = 0;
            var directories = Directory.EnumerateDirectories(path);
            var result = new List<IParsable>();
            foreach (var directory in directories)
            {
                var type = Type.GetType(Path.GetFileName(directory));
                var files = Directory.EnumerateFiles(directory);
                foreach (var file in files)
                {
                    try
                    {
                        result.Add(DeserializeFromString(File.ReadAllText(file), type));
                    }
                    catch (InvalidOperationException) { failCount++; }
                }
            }
            deserializeFailCount = failCount;
            return result;
        }
    }

    public class ParserException : Exception
    {
        public int PositionInText { get; }
        public ParserException() { }
        public ParserException(int position) { PositionInText = position; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IParsable[] parsables = {
                new Person("Oleksii", "Vynnychenko", PersonGender.Male, new DateTime(1983, 11, 21)),
                new Company("NewtonIdeas", "Ltd", 20),
                new Person("Bill", "Gates", PersonGender.Male, new DateTime(1955, 10, 28)),
                new Person("Melinda", "Gates", PersonGender.Female, new DateTime(1964, 8, 15)),
                new Company("Microsoft", "Public JSC", 118584) };
            NiUtils.SerializeToXmlFiles(parsables, "IParsables");

            /* SilentDeserializeFromXmlFiles() method, unlike DeserializeFromXmlFiles() will count and ignore any deserialization
             * errors (InvalidOperationException) that may occur. On repository in <IParsable> directory there will be corrupted XML-file
             * fault_person.xml in Person's subdirectory. */
            int failedXmlFiles;
            var deserializedParsables = NiUtils.SilentDeserializeFromXmlFiles("IParsables", out failedXmlFiles);
            Console.WriteLine("---- Deserializing XML files ----");
            foreach (var parsable in deserializedParsables) { Console.WriteLine(parsable.ToString()); }
            Console.WriteLine($"\nNumber of corrupted XML files: {failedXmlFiles}");
            Console.ReadKey();
        }
    }
}
