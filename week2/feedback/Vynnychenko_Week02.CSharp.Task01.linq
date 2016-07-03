<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>


// Anton: well done!

namespace Week02.CSharp.Task01
{
    public enum PersonGender { Female, Male };

    public interface IParsable : IEquatable<IParsable> { }

    public delegate IParsable[] ParserDelegate(string text);

    public class Person : IParsable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public PersonGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age
        {
            get { return (DateTime.Today - BirthDate).Days / 365; }
        }

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
        public string Name { get; }

        public Company(string name) { Name = name; }

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

        public override string ToString() => Name;

        public bool Equals(IParsable other)
        {
            if (other is Company) { if (Name == ((Company)other).Name) { return true; } }
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

    public static class GlobalParser
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

            /* Input text is Task1.4 for C#. Only added "near company Cisco, based in San-Francisco" and "near company Texas Instruments, based in Texas",
             * just so program can parse Company type instances out of it. */
            string text = @"1. Implement a type Person. A Person has a gender, a name, and a date of birth. A name is a full name
                                with first name and last name.
                            2. Person can be printed to Console in the format: <lastname>, <firstname> (<m/f>, <age>). Example: Shabinskiy, Anton (m, 29)
                            3. Implement extracting person data from texts. Assume that person always appears
                                in text as Mr.John Smith was born on 2001/11/03 near company Cisco, based in San-Francisco or
                                Mrs.Jessica Brown was born on 1999/12/3 near company Texas Instruments, based in Texas etc. You need to extract such data
                                and get an array of Person objects.
                            4.Implement a special type of person - a US citizen. US citizens have a Social Security Number(SSN).
                            5.For an array of Person objects, implement a procedure to remove duplicates and a procedure to sort an array.Two persons
                                are equal if they have the same gender, full name, and date of birth. For sorting, persons are sorted by gender
                                first(female, male), then alphabetically by name, then by date of birth.
                                NOTE: do not use LINQ, collections, and any framework methods for sorting etc. Use only plain arrays and implement your own
                                logic.Also, use types wisely(classes, structs, enums etc) to make your implementation elegant and efficient.";

            ParserDelegate BuggedParser = (t) => { throw new ParserException(123); } ;

            Console.WriteLine("----Running normal parsers----");
            var results = GlobalParser.Parse(text, Person.Parse, Person.Parse, Company.Parse, (t) => new IParsable[] { new Person("Bill", "Gates", PersonGender.Male, DateTime.Now.AddYears(-60)) });
            foreach (var result in results) { Console.WriteLine(result.ToString()); }

            Console.WriteLine("\n----Running normal parsers, results excluding duplicates----");
            results = GlobalParser.Parse(text, Person.Parse, Person.Parse, Company.Parse, (t) => new IParsable[] { new Person("Bill", "Gates", PersonGender.Male, DateTime.Now.AddYears(-60)) });
            foreach (var result in results.GetCopyWithoutDuplicates()) { Console.WriteLine(result.ToString()); }

            Console.WriteLine("\n----Running same with bugged parsers----");
            results = GlobalParser.Parse(text, Person.Parse, BuggedParser, Person.Parse, Company.Parse, (t) => new IParsable[] { new Person("Bill", "Gates", PersonGender.Male, DateTime.Now.AddYears(-60)) });
            foreach (var result in results.GetCopyWithoutDuplicates()) { Console.WriteLine(result.ToString()); }

            Console.ReadKey();
        }
    }

}
