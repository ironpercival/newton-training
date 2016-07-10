<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Schema</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.IO</Namespace>
</Query>

//Anton: pretty good!
namespace ConsoleApp1
{

    public class Person : IXMLCompatible
    {

        public FullName Name { get; private set; }
        public DateTime BirthDate { get; private set; }
        public Gender Gender { get; private set; }

        public int Age => (DateTime.Now - BirthDate).Days / 365;


        public string XName { get; }

        public Person() { }

        public Person (XmlReader r)
        {
            ReadXml(r);
            XName = typeof(Person).ToString();
        }

        public Person(string firstName, string lastName, DateTime birthDate, GenderEnum gender, string xName = null)
        {
            this.Name = new FullName(firstName, lastName);

            this.BirthDate = birthDate;

            this.Gender = new Gender(gender);

            XName = (xName != null) ? xName : this.GetType().Name;
        }


        public override string ToString()
        {
            return $"{Name.LastName}, {Name.FirstName} ({Gender}, {Age})";
        }

        public int CompareTo(IXMLCompatible other)
        {

            var person = other as Person;

            if (person == null)
                throw new Exception("Trying to compare with an unsupported object!");

            if (!Gender.Equals(person.Gender))
                return Gender.CompareTo(person.Gender);

            if (!Name.Equals(person.Name))
                return Name.CompareTo(person.Name);

            if (BirthDate != person.BirthDate)
                return BirthDate.CompareTo(person.BirthDate);

            return 0;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement("Name");
            if (!isEmptyElement)
            {
                this.Name = new FullName(reader.ReadElementContentAsString("FirstName", ""),
                    reader.ReadElementContentAsString("LastName", ""));
            }
            reader.ReadEndElement();
            this.BirthDate = DateTime.Parse(reader.ReadElementContentAsString("BirthDate",""));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("gender", Gender.ToString());
            writer.WriteStartElement("Name");
            writer.WriteElementString("FirstName", Name.FirstName);
            writer.WriteElementString("LastName", Name.LastName);
            writer.WriteEndElement();
            writer.WriteElementString("BirthDate", BirthDate.ToString());
        }
    }


    public enum GenderEnum
    {
        Male, Female
    }

    public struct Gender : IComparable<Gender>
    {
        public GenderEnum Value { get; private set; }

        public Gender (GenderEnum value)
        {
            Value = value;
        }

        public int CompareTo (Gender gender)
        {
            if (this.Value == gender.Value)
            {
                return 0;
            }

            if (this.Value > gender.Value)
            {
                return 1; 
            }

            return -1;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }


    public struct FullName : IComparable<FullName>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public FullName(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public int CompareTo (FullName name)
        {
            var thisName = $"{this.FirstName}{this.LastName}".ToLower();

            var anotherName = $"{name.FirstName}{name.LastName}".ToLower();

            return thisName.CompareTo(anotherName);
        }
    }

    public interface IXMLCompatible : IComparable<IXMLCompatible>, IXmlSerializable
    {
        string XName { get; }
    }


    public static class XMLAccesories
    {
        public static void Serialize (IXMLCompatible obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (FileStream fs = File.Create(obj.XName + ".xml"))
            {
                serializer.Serialize(fs, obj);
            }
        }

        public static IXMLCompatible DeSerialize (string file, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);


            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                return (IXMLCompatible)serializer.Deserialize(fs);
            }

            
        }
    }

    public class PKuzvi3
    {
        public static void Main(string[] args)
        {
            Person fred = new Person("H", "H", DateTime.Now, GenderEnum.Female);


            XMLAccesories.Serialize(fred);

            XMLAccesories.DeSerialize("Person.xml", typeof(Person));

            Console.WriteLine("hello world");
        }
    }
}
