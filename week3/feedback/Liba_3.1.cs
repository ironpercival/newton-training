<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Collections</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>

// Anton: good overall. Though your approach is a bit limited to what and how you serialize. At least you should use attributes in order to specify what fields to serialize. Also, it would be nice to have a virtual method for serialization in order to control how the object is serialized. Finally, check my comment on Dispose().

namespace Task3
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Renewed Task 2.1, but without removing duplicates
            const String text = "Last year winner FC Dynamo (Kyiv, Ukraine, et. 1927) lost the game with 0:2 to FC Kryvbas (Kryvyi Rih, Ukraine, est. 1959)." +
            "Share price of Comp. Microsoft (Information Technology, US) dropped to $100" +
            "John Smith (1983/11/05, male) has been sentenced to a 3 year imprisonment for corruption.";

            var parsers = new List<Func<string, IEnumerable<IParsee>>>();
            parsers.Add(Person.ParserFunc);
            parsers.Add(FootballClub.ParserFunc);

            var parsedEntities = parsers.SelectMany(p => p(text));

            Console.WriteLine("All objects:");
            Console.WriteLine(parsedEntities.Stringify());

            // And Task 3.1
            // Create XML-files
            List<IParsee> all = new List<IParsee>(parsedEntities.ToArray());

            String path = Path.Combine(Directory.GetCurrentDirectory(), @"Data\"); 
            XML.encode(all, path);

            // Create objects from XML-files
            path = $"{path}\\FootballClub\\Dynamo1927Kyiv.xml";
            var newClub = XML.decode<FootballClub>(path);
            Console.WriteLine(newClub.ToString());
            Console.ReadKey();
        }
    }

    class XML
    {
        public XML() { }

        public static void encode(List<IParsee> list, string path)
        {
            if (list.Count == 0)
                throw new System.ArgumentException("The array is empty!");

            foreach (var item in list)
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(item.GetType());

                String folder = item.GetType().Name;
                String fileName = createFileName(item);

                String fullpath = Path.Combine(path, folder, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

                System.IO.FileStream file = System.IO.File.Create(fullpath);

                writer.Serialize(file, item);
				
				// Anton: that is what using{} block is for, FileStream implements IDisposable. Check it in IO theory of week 3 and disposal theory of week 4.
                file.Dispose(); // I cant use Close()
            }

            Console.WriteLine($"XML files were created. Their location: {path}");
        }

        public static T decode<T>(string path)
        {
            if (!File.Exists(path))
                throw new System.ArgumentException($"The path ({path}) is incorrect! There is no such file!");
            
            XmlSerializer serializer = new
            XmlSerializer(typeof(T));
            
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);
            
            T temp = (T)serializer.Deserialize(reader);
            fs.Dispose();

            return temp;
        }

        public static string createFileName(Object item)
        {
            string filename = "";
            foreach (var prop in item.GetType().GetProperties())
            {
                filename += prop.GetValue(item, null);
            }

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            filename = rgx.Replace(filename, "");

            return $"{filename}.xml";
        }
    }

    public interface IParsee { }

    public class Person : IParsee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        [XmlAttribute("gender")]
        public string Gender { get; set; }

        public Person() { }
        public Person(string firstName, string lastName, DateTime birthDate, string gender)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Gender = gender;
        }

        public int Age => (DateTime.Now - BirthDate).Days / 365;

        public static IEnumerable<Person> ParserFunc(string text)
        {
            string regex = @"(\p{Lu}\w+)?\s(\p{Lu}\w+)?\s\((\d{4}/\d{2}/\d{2}),\s(\w+)?\)";

            var Matches = Regex.Matches(text, regex);
            var persons = new Person[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];

                var firstName = match.Groups[1].Value;
                var lastName = match.Groups[2].Value;
                var birthDate = DateTime.Parse(match.Groups[3].Value);
                var gender = match.Groups[4].Value;

                persons[i] = new Person(firstName, lastName, birthDate, gender);
            }

            return persons;
        }

        public override string ToString()
        {
            return $"{FirstName}, {LastName} ({Gender}, {Age})";
        }
    }


    public class FootballClub : IParsee
    {
        public string Name { get; set; }
        public int Creation { get; set; }
        public string Adress { get; set; }

        public FootballClub() { }
        public FootballClub(string name, string adress, int year)
        {
            Name = name;
            Adress = adress;
            Creation = year;
        }

        public static IEnumerable<FootballClub> ParserFunc(string text)
        {
            string regex = @"FC\s(\p{Lu}\w+)?\s\((\p{Lu}.+?),\s(\p{Lu}\w+).+?(\d{4})";

            var Matches = Regex.Matches(text, regex);
            var clubs = new FootballClub[Matches.Count];

            for (int i = 0; i < Matches.Count; i++)
            {
                var match = Matches[i];
                int year = Int32.Parse(match.Groups[4].Value);

                clubs[i] = new FootballClub(match.Groups[1].Value, match.Groups[2].Value, year);
            }

            return clubs;
        }

        public override string ToString()
        {
            return $"FC {Name}, ({Adress}, founded in {Creation})";
        }
    }

    public static class ParseeExtensions
    {
        public static string Stringify(this IEnumerable<IParsee> persons)
        {
            var strBuilder = new StringBuilder();
            foreach (var p in persons)
                strBuilder.AppendLine(p.ToString());

            return strBuilder.ToString();
        }
    }
}
