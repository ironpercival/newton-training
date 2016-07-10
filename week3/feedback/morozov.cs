<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Xml</Namespace>
</Query>

// Anton: pretty good!

namespace Parsers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<IXMLConvertible> list = Parser.ParseXML("file.xml", 
                new MyXMLParser[] { Person.ReadXML, FootballClub.ReadXML });
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
                item.SaveToXML();
            }
        }

        interface IXMLConvertible
        {
            void SaveToXML();
        }

        delegate List<IXMLConvertible> MyXMLParser(string filename);

        class Person : IXMLConvertible
        {
            private string FirstName { get; set; }
            private string LastName { get; set; }

            private static readonly string XmlFolder = "people\\";

            public string FullName { get; private set; }
            public DateTime BirthDate { get; private set; }
            public int Age { get; private set; }
            public char Gender { get; private set; }

            public Person(string firstName, string lastName, string gender, DateTime birthDate)
            {
                FirstName = firstName;
                LastName = lastName;

                Gender = (gender == "male" ? 'm' : 'f');
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

            public static List<IXMLConvertible> ReadXML(string filename)
            {
                List<IXMLConvertible> people = new List<IXMLConvertible>();

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(filename, settings))
                {
                    reader.ReadToFollowing("person");

                    do
                    {
                        try
                        {
                            string gender = reader["gender"];

                            reader.ReadStartElement("person");

                            reader.ReadStartElement("name");
                            string firstName = reader.ReadElementContentAsString("firstname", "");
                            string lastName = reader.ReadElementContentAsString("lastname", "");
                            reader.ReadEndElement();

                            DateTime birthDate = DateTime.Parse(reader.ReadElementContentAsString("birthdate", ""));

                            people.Add(new Person(firstName, lastName, gender, birthDate));
                        }
                        catch (XmlException)
                        {
                            Console.WriteLine("Wrong format");
                        }

                    } while (reader.ReadToNextSibling("person"));
                }
                return people;
            }

            public void SaveToXML()
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                DirectoryInfo di = Directory.CreateDirectory(XmlFolder);
                string path = XmlFolder + FullName + ".xml";

                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("person");
                    writer.WriteAttributeString("gender", Gender == 'm' ? "male" : "female");

                    writer.WriteStartElement("name");
                    writer.WriteElementString("firstname", FirstName);
                    writer.WriteElementString("lastname", LastName);
                    writer.WriteEndElement();

                    writer.WriteElementString("birthdate", BirthDate.ToString("yyyy/mm/dd"));

                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
        }

        class FootballClub : IXMLConvertible
        {
            private static readonly string XmlFolder = "clubs\\";

            public string Country { get; private set; }
            public string Name { get; private set; }
            public string OriginCity { get; private set; }
            public uint FoundationYear { get; private set; }

            public FootballClub(string name, string originCity, string country, uint foundationYear)
            {
                Name = name;
                OriginCity = originCity;
                FoundationYear = foundationYear;
                Country = country;
            }

            public override string ToString()
            {
                return $"{Name}, {OriginCity}, est. {FoundationYear}";
            }

            public static List<IXMLConvertible> ReadXML(string filename)
            {
                List<IXMLConvertible> clubs = new List<IXMLConvertible>();

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(filename, settings))
                {
                    reader.ReadToFollowing("club");

                    do
                    {
                        try
                        {

                            reader.ReadStartElement("club");

                            string name = reader.ReadElementContentAsString("name", "");

                            reader.ReadStartElement("origin");
                            string city = reader.ReadElementContentAsString("city", "");
                            string country = reader.ReadElementContentAsString("country", "");
                            reader.ReadEndElement();

                            int yearFoundation = reader.ReadElementContentAsInt("foundation", "");

                            clubs.Add(new FootballClub(name, city, country, (uint)yearFoundation));
                        }
                        catch (XmlException)
                        {
                            Console.WriteLine("Wrong format");
                        }

                    } while (reader.ReadToNextSibling("person"));
                }
                return clubs;
            }

            public void SaveToXML()
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                DirectoryInfo di = Directory.CreateDirectory(XmlFolder);
                string path = XmlFolder + Name + ".xml";

                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    writer.WriteStartDocument();

                    writer.WriteStartElement("club");

                    writer.WriteElementString("name", Name);

                    writer.WriteStartElement("origin");
                    writer.WriteElementString("city", OriginCity);
                    writer.WriteElementString("country", Country);
                    writer.WriteEndElement();

                    writer.WriteElementString("foundation", FoundationYear.ToString());

                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
        }

        class Parser
        {
            public static List<IXMLConvertible> ParseXML(string filename, MyXMLParser[] parsers)
            {
                List<IXMLConvertible> result = new List<IXMLConvertible>();
                foreach (var parser in parsers)
                {
                    List<IXMLConvertible> instances = parser(filename);
                    foreach (var instance in instances)
                    {
                        result.Add(instance);
                    }
                }
                return result;
            }
        }
    }
}
