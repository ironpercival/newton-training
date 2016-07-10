<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Xml.Schema</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>

// Anton: awesome!

namespace ConsoleApplication
{
    using CustomExtensions;
    public class Program
    {
        public static void Main(string[] args)
        {
            string text = "Last year winner FC Dynamo (Kyiv, Ukraine, est. 1927) lost the game with 0:2 to FC " +
				"Kryvbas (Kryvyi Rih, Ukraine, est. 1959). Share price of Microsoft (Information Technology, " +
				"US) Microsoft (Information Technology, US) dropped to FC Dynamo (Kyiv, Ukraine, est. 1927) Miss " +
				"Lana Webb was born on 1979/07/13 $100 Mr. John Smith was born on 1983/11/05, male)  Miss Lana " +
				"Webb was born on 1979/07/13 has been sentenced to a 3 year imprisonment for corruption " +
				"Microsoft (Information Technology, Miss Jessica White was born on 1999/12/31 US)\n\n// Mr. " +
				"John Smith was born on 1983/11/05 Example: MakeUp (Cosmetics, USA)";
		    
            Console.WriteLine("Parsing objects from text and serializing them.");
            CompanyParser companyParser = new CompanyParser();
            FootballClubParser fcParser = new FootballClubParser();
            PersonParser pParser = new PersonParser();
            Func<string, IEnumerable<IParsable>> parsers = companyParser.Parse;
            parsers += fcParser.Parse;
            parsers += pParser.Parse;

            Utils.SerializeToFiles(Utils.Parse(text,parsers),"file.xml");

            Action<string,string> parseAndSerializeAll = Utils.ParseAndSaveToXmlFileAll<Company,CompanyParser>;
            parseAndSerializeAll += Utils.ParseAndSaveToXmlFileAll<FootballClub,FootballClubParser>;
            parseAndSerializeAll += Utils.ParseAndSaveToXmlFileAll<Person,PersonParser>;
            parseAndSerializeAll(text,"all.xml");

            Action<string,string> parseAndSerializeDistinct = Utils.ParseAndSaveToXmlFileDistinct<Company,CompanyParser>;
            parseAndSerializeDistinct += Utils.ParseAndSaveToXmlFileDistinct<FootballClub,FootballClubParser>;
            parseAndSerializeDistinct += Utils.ParseAndSaveToXmlFileDistinct<Person,PersonParser>;
            parseAndSerializeDistinct(text,"distinct.xml");

            Console.WriteLine("Deserializing the serialized objects.");

            var companyDeserializer = new Serializer(typeof(List<Company>));
            List<Company> allCompanies = (List<Company>)companyDeserializer.Deserialize("Company/all.xml");
            List<Company> distinctCompanies = (List<Company>)companyDeserializer.Deserialize("Company/distinct.xml");

            var fcDeserializer = new Serializer(typeof(List<FootballClub>));
            List<FootballClub> allFc = (List<FootballClub>)fcDeserializer.Deserialize("FootballClub/all.xml");
            List<FootballClub> distinctFc = (List<FootballClub>)fcDeserializer.Deserialize("FootballClub/distinct.xml");

			var pDeserializer = new Serializer(typeof(List<Person>));
			List<Person> allPersons = (List<Person>)pDeserializer.Deserialize("Person/all.xml");
			List<Person> distinctPersons = (List<Person>)pDeserializer.Deserialize("Person/distinct.xml");

            Console.WriteLine("Serialization And Deserialization work properly: ");

            List<Company> allCompanies2 = companyParser.Parse(text).ToList();
            List<Company> distinctCompanies2 = allCompanies2.RemoveDuplicates<Company>();
            List<FootballClub> allFc2 = fcParser.Parse(text).ToList();
            List<FootballClub> distinctFc2 = allFc2.RemoveDuplicates<FootballClub>(); 
			List<Person> allPersons2 = pParser.Parse(text).ToList();
			List<Person> distinctPersons2 = allPersons2.RemoveDuplicates();

            Console.WriteLine(Enumerable.SequenceEqual(allCompanies.OrderBy(t => t), allCompanies2.OrderBy(t => t)));
            Console.WriteLine(Enumerable.SequenceEqual(distinctCompanies.OrderBy(t => t), distinctCompanies2.OrderBy(t => t)));
            Console.WriteLine(Enumerable.SequenceEqual(allFc.OrderBy(t => t), allFc2.OrderBy(t => t)));
            Console.WriteLine(Enumerable.SequenceEqual(distinctFc.OrderBy(t => t), distinctFc2.OrderBy(t => t)));
			Console.WriteLine(Enumerable.SequenceEqual(allPersons.OrderBy(t => t), allPersons2.OrderBy(t => t)));
            Console.WriteLine(Enumerable.SequenceEqual(distinctPersons.OrderBy(t => t), distinctPersons2.OrderBy(t => t)));
        }
    }

        public static class Utils
	{   
		public static List<IParsable> Parse(string input, Func<string, IEnumerable<IParsable>> parsers)
		{
			var res = new List<IParsable>();
			foreach (Func<string,  IEnumerable<IParsable>> p in parsers.GetInvocationList())
			{
				res.AddRange(p(input));
			}
			return res;
		}
		public static void SerializeToFiles(List<IParsable> list, string fileNames)
		{
		    for(int i = 0; i < list.Count; ++i)
			{
				Serializer s = new Serializer(list[i].GetType());
				if(!Directory.Exists(list[i].GetType().Name)){
					DirectoryInfo di = Directory.CreateDirectory(list[i].GetType().Name);
				}
				s.Serialize(list[i],list[i].GetType().Name+"/"+i+fileNames); 
			}
		}

		public static void ParseAndSaveToXmlFileAll<T,TParser>(string input, string file)
            where T : IParsable
            where TParser: IParser<T>, new()
		{
            TParser parser = new TParser(); 
			IEnumerable<T> insatnces = parser.Parse(input);
            Serializer serializer = new Serializer(typeof(List<T>));
			if(!Directory.Exists(typeof(T).Name)){
				DirectoryInfo di = Directory.CreateDirectory(typeof(T).Name);
			}
            serializer.Serialize(insatnces.ToList(),typeof(T).Name+"/"+file); 
		}
        
        public static void ParseAndSaveToXmlFileDistinct<T,TParser>(string input, string file)
            where T : IParsable
            where TParser: IParser<T>, new()
		{
            TParser parser = new TParser(); 
			IEnumerable<T> insatnces = parser.Parse(input);
            Serializer serializer = new Serializer(typeof(List<T>));
            List<T> distinct = insatnces.ToList().RemoveDuplicates<T>();
			if(!Directory.Exists(typeof(T).Name)){
				DirectoryInfo di = Directory.CreateDirectory(typeof(T).Name);
			}
            serializer.Serialize(distinct,typeof(T).Name+"/"+file); 
		}
	}

    public class Serializer
    {
        XmlSerializer serializer;
        public Serializer(Type t){
            serializer = new XmlSerializer(t);
        }

        public void Serialize(object obj, string toFile){
            using(StreamWriter writer = new StreamWriter(new FileStream (toFile, FileMode.Create)))
            {
                serializer.Serialize(writer,obj);
            }
        }  

        public object Deserialize(string fromFile) {
            object result;
            using(StreamReader reader = new StreamReader(new FileStream (fromFile, FileMode.Open)))
            {
                result = serializer.Deserialize(reader);
            }
            return result;
        }  
    }

    public interface IParsable {}
	public interface IParser<out T> where T: IParsable 
    {
		IEnumerable<T> Parse(string text);
    }
    
	public sealed class CompanyParser: IParser<Company>
	{
		private readonly string pattern = "(\\w+)\\s\\(([\\w][\\w ]+),\\s(\\w+)\\)";	

		public IEnumerable<Company> Parse(string input)
		{
            var companyMatches = Regex.Matches(input, pattern);
            var result = new Company[companyMatches.Count];
            int i = 0;
			foreach (Match match in Regex.Matches(input,pattern)) {
				try{
                    result[i] = new Company(match.Groups[1].Value,match.Groups[2].Value,match.Groups[3].Value);
				} catch(Exception e){
					Console.WriteLine (e);
				}
                ++i;
			}
            return result;
		}
	}

	public sealed class FootballClubParser : IParser<FootballClub>
	{
		private readonly string pattern ="FC\\s(\\w+)\\s\\(([\\w][\\w ]+),\\s([\\w][\\w ]+),\\sest\\.\\s(\\d{4})\\)";

		public IEnumerable<FootballClub> Parse(string input)
		{
            var fcMatches = Regex.Matches(input, pattern);
            var result = new FootballClub[fcMatches.Count];
            int i = 0;
			foreach (Match match in Regex.Matches(input,pattern)) {
				try{
                    result[i] = new FootballClub(match.Groups[1].Value,match.Groups[2].Value,
					match.Groups[3].Value,Int32.Parse(match.Groups[4].Value));
				} catch(Exception e){
					Console.WriteLine (e);
				}
                ++i;
			}
            return result;
		}
	}

	public sealed class PersonParser : IParser<Person>
	{
		private readonly string pattern = "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )(\\w+)\\s(\\w+)\\swas\\sborn\\son\\s(\\d{4})/(\\d{2})/(\\d{2})";

		public IEnumerable<Person> Parse(string input)
		{
			var pMatches = Regex.Matches(input, pattern);
            var result = new Person[pMatches.Count];
            int i = 0;
			foreach (Match match in Regex.Matches(input,pattern)) {
				try{
					Gender g = match.Groups[1].Value == "Mr. " ? Gender.m : Gender.f;
					DateTime date = new DateTime (Int32.Parse(match.Groups[4].Value),Int32.Parse(match.Groups[5].Value),Int32.Parse(match.Groups[6].Value));
					result[i] = new Person(match.Groups[2].Value, match.Groups[3].Value,date,g);
				} catch(Exception e){
					Console.WriteLine (e);
				}
                ++i;
			}
			return result;
		}
	}

    public class Company : IComparable<Company>, IParsable, IXmlSerializable
	{
		public string Name { get; set; }
		public string Country { get; set; }
		public string FieldOfActivity { get; set; }

        public Company(){}
		public Company (string name, string field, string country)
		{
			Name = name;
			Country = country;
			FieldOfActivity = field;
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			var other = obj as Company;
			return Name.Equals(other.Name) && Country.Equals(other.Country) 
				&& FieldOfActivity.Equals(other.FieldOfActivity);
		}

		public override int GetHashCode(){
			int hash = 17;
			hash = hash * 31 + Name.GetHashCode();
			hash = hash * 31 + Country.GetHashCode();
			hash = hash * 31 + FieldOfActivity.GetHashCode();
			return hash;
		}

		public int CompareTo(Company other)
		{
			if (!Name.Equals (other.Name))
				return Name.CompareTo (other.Name);
			if (!Country.Equals (other.Country))
				return Country.CompareTo (other.Country);
			return FieldOfActivity.CompareTo (other.FieldOfActivity);
		}

		public override string ToString()
        {
			return string.Format ("{0} ({1}, {2})", Name, FieldOfActivity, Country);
		}

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
			reader.MoveToContent();
			reader.ReadStartElement();
			Name = reader.ReadElementContentAsString();
			FieldOfActivity = reader.ReadElementContentAsString();
			Country = reader.ReadElementContentAsString();
			reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            	writer.WriteElementString("Name", Name);
		        writer.WriteElementString("FiledOfActivity", FieldOfActivity);
                writer.WriteElementString("Country",Country);
        }

		/*
			<Company>
				<Name>MakeUp</Name>
				<FiledOfActivity>Cosmetics</FiledOfActivity>
				<Country>USA</Country>
			</Company>
		*/
	}

    public class FootballClub : IComparable<FootballClub>, IParsable, IXmlSerializable
    {
        int yearOfFoundation;

        public string Name{ get; set;}
        public string City{ get; set;}
        public string Country{ get; set;}
        public int YearOfFoundation{ 
            get{ return yearOfFoundation;} 
            set{ 
                if (value < 0 || value > DateTime.Now.Year) {
                    throw new ArgumentOutOfRangeException ("year of foundation");
                } else {
                    yearOfFoundation = value;
                }
            }
        }

        
        public FootballClub(){}
        public FootballClub (string name,string city,string country,int yearOfFoundation)
        {
            Name = name;
            City = city;
            Country = country;
            YearOfFoundation = yearOfFoundation;
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            var other = obj as FootballClub;
            return Name.Equals(other.Name) && Country.Equals(other.Country) 
                && City.Equals(other.City) && YearOfFoundation.Equals(other.YearOfFoundation);
        }

        public override int GetHashCode(){
            int hash = 17;
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + Country.GetHashCode();
            hash = hash * 31 + City.GetHashCode();
            hash = hash * 31 + YearOfFoundation.GetHashCode();
            return hash;
        }

        public int CompareTo(FootballClub other)
        {
            if (!Name.Equals (other.Name))
                return Name.CompareTo (other.Name);
            if (!Country.Equals (other.Country))
                return Country.CompareTo (other.Country);
            if (!City.Equals (other.City))
                return City.CompareTo(other.City);
            return YearOfFoundation.CompareTo (other.YearOfFoundation);
        }

        public override string ToString ()
        {
            return string.Format ("FC {0} ({1}, {2}, est. {3})", Name, City, Country, YearOfFoundation);
        }

        public XmlSchema GetSchema() => null;

        /*
            <?xml version="1.0" encoding="utf-8"?>
            <FootballClub Name="Knattspyrnusamband Islands">
                <Country>Reykjavík</Country>
                <City>Iceland</City>
                <YearOfFoundation>1947</YearOfFoundation>
            </FootballClub>
		*/
        public void ReadXml(XmlReader reader)
        {
			Name = reader.GetAttribute("Name");
            reader.MoveToContent();
			reader.ReadStartElement();
            Country = reader.ReadElementContentAsString();
            City = reader.ReadElementContentAsString ();
            YearOfFoundation = reader.ReadElementContentAsInt();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
                writer.WriteAttributeString("Name",Name);
            	writer.WriteElementString("Country", Country);
		        writer.WriteElementString("City", City);
                writer.WriteElementString("YearOfFoundation",YearOfFoundation.ToString());
        }
    }

    
	public enum Gender {f, m};

	public class Person : IComparable <Person>, IParsable
	{
		string firstname, lastname;
		DateTime dateOfBirth;
		Gender gender;

		public string Firstname
		{
			get
			{ 
				return firstname;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException ("firstname");
				string trimmedValue = value.Trim ();
				if(trimmedValue.Length == 0) 
					throw new ArgumentOutOfRangeException ("firstname");
				firstname = trimmedValue;
			}
		}

		public string Lastname
		{
			get
			{ 
				return lastname;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException ("lastname");
				string trimmedValue = value.Trim ();
				if(trimmedValue.Length == 0) 
					throw new ArgumentOutOfRangeException ("lastname");
				lastname = trimmedValue;
			}
		}

		public DateTime DateOfBirth
		{
			get
			{ 
				return dateOfBirth;
			}
			set
			{
				if (value > DateTime.Now)
					throw new ArgumentOutOfRangeException ("dateOfBirth");
				dateOfBirth = value;
			}
		}

		public Gender Gender
		{
			get
			{
				return gender;
			}
			set
			{
				if ((int)value >= Enum.GetNames(typeof(Gender)).Length || (int)value < 0)
					throw new ArgumentOutOfRangeException ("gender");
				gender = value;
			}
		}

		public Person(){}
		public Person (string firstname, string lastname, DateTime dateOfBirth, Gender gender)
		{
			Firstname = firstname;
			Lastname = lastname;
			DateOfBirth = dateOfBirth;
			Gender = gender;
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;
			var other = obj as Person;
			return Gender.Equals(other.Gender) && Firstname.Equals(other.Firstname) 
				&& Lastname.Equals(other.Lastname) && DateOfBirth.Equals(other.DateOfBirth);
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + firstname.GetHashCode();
			hash = hash * 31 + lastname.GetHashCode();
			hash = hash * 31 + gender.GetHashCode();
			hash = hash * 31 + dateOfBirth.GetHashCode();
			return hash;
		}

		public int CompareTo(Person other) {
			if (!Gender.Equals (other.Gender))
				return Gender.CompareTo (other.Gender);
			if (!Lastname.Equals (other.Lastname))
				return Lastname.CompareTo (other.Lastname);
			if (!Firstname.Equals (other.Firstname))
				return Firstname.CompareTo (other.Firstname);
			return DateOfBirth.CompareTo (other.DateOfBirth);
		}

		public override string ToString ()
		{
			int age = DateTime.Now.Year - DateOfBirth.Year;
			return string.Format ("{0}, {1} ({2}, {3})", Lastname, Firstname, Gender, age);
		}
		/*
		<Person gender="m">
			<Name>
				<FirstName>Anton</FirstName>
				<LastName>Shabinskiy</LastName>
			</Name>
			<BirthDate>1987-05-02</BirthDate>
		</Person>
		*/

		public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
			Gender = reader.GetAttribute("gender") == "f" ? Gender.f : Gender.m;
			reader.MoveToContent();
			reader.ReadStartElement();
			reader.ReadStartElement();
			Firstname  = reader.ReadElementContentAsString();
			Lastname = reader.ReadElementContentAsString();
			reader.ReadEndElement();
			reader.ReadStartElement();
			DateOfBirth = Convert.ToDateTime(reader.ReadContentAsString());
			reader.ReadEndElement();
			reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            	writer.WriteAttributeString("gender", Gender.ToString());
				writer.WriteStartElement("Name");
				writer.WriteElementString("FirstName", Firstname);
				writer.WriteElementString("LastName", Lastname);
				writer.WriteEndElement();
				writer.WriteElementString("BirthDate",DateOfBirth.ToString());
        }
	}    
}

namespace CustomExtensions
{
	public static class ListExtension
	{
		public static List<T> RemoveDuplicates<T>(this List<T> list)
		{
			return list.Distinct().ToList();
		}
    }
}
