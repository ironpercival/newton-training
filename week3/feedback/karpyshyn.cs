<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.IO</Namespace>
</Query>


// Anton: your design decision is somewhat good, but implementation is bad. See comments below.

namespace NewtonTraining {


	class Program {
		static void Main(string[] args) {
			IXMLConverter<Person> conv = new PersonConverter();
			List<Person> persons = new List<Person>();
			persons.Add(new Person("John", "Smith", Convert.ToDateTime("01/08/2008"), GenderEnum.Male));
			conv.WriteToXml(persons, System.IO.Directory.GetCurrentDirectory());
			

		}
	}


	public enum GenderEnum {
		Male,
		Female
	}

	public struct Gender : IComparable<Gender>, IEquatable<Gender> {
		public GenderEnum Value { get; private set; }

		public int CompareTo(Gender other) {
			if (Value == other.Value)
				return 0;

			if (Value == GenderEnum.Female)
				return -1;

			return 1;
		}

		public Gender(GenderEnum gender) {
			Value = gender;
		}

		public override bool Equals(object obj) {
			if (obj == null || !(obj is Gender))
				return false;

			var other = (Gender)obj;

			return Equals(other);
		}

		public override string ToString() {
			return Value.ToString()[0].ToString(); // m/f
		}

		public bool Equals(Gender other) {
			return Value == other.Value;
		}

		public static bool operator ==(Gender left, Gender right) {
			return left.Equals(right);
		}

		public static bool operator !=(Gender left, Gender right) {
			return !(left.Equals(right));
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}
	}

	public struct FullName : IComparable<FullName>, IEquatable<FullName> {
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public int CompareTo(FullName other) {
			var thisName = $"{LastName}{FirstName}".ToLower(); // lowercase, to avoid case-sensitivity

			var otherName = $"{other.LastName}{other.FirstName}".ToLower();

			return thisName.CompareTo(otherName); // string by default provides alphabetic comparison
		}

		public bool Equals(FullName other) {
			return FirstName == other.FirstName && LastName == other.LastName;
		}

		public override bool Equals(object obj) {
			if (obj == null || !(obj is FullName))
				return false;

			var other = (FullName)obj;

			return Equals(other);
		}

		public override int GetHashCode() {
			return Tuple.Create(FirstName, LastName).GetHashCode();
		}

		public static bool operator ==(FullName left, FullName right) {
			return left.Equals(right);
		}

		public static bool operator !=(FullName left, FullName right) {
			return !(left == right);
		}
	}

	public class Person : IParsee, IXMLConvertable {

		public Person(string firstName, string lastName, DateTime birthDate, GenderEnum gender) {
			Name = new FullName() { FirstName = firstName, LastName = lastName };
			BirthDate = birthDate;
			Gender = new Gender(gender);
		}

		public FullName Name { get; private set; }

		public DateTime BirthDate { get; private set; }

		public int Age => (DateTime.Now - BirthDate).Days / 365; // C# 6 fancy getter syntax (don't bother with precision of age calculation, it's not important here)

		public Gender Gender { get; private set; }

		protected virtual bool CheckEquality(Person p) {
			return Name == p.Name
				&& BirthDate == p.BirthDate
				&& Gender == p.Gender;
		}

		public override bool Equals(object obj) {
			var other = obj as Person;

			return other != null && CheckEquality(other);
		}

		public override string ToString() {
			return $"{Name.LastName}, {Name.FirstName} ({Gender}, {Age})";
		}

		public override int GetHashCode() {
			return Tuple.Create(Name, BirthDate, Gender).GetHashCode();
		}

		public int CompareTo(IParsee other) {

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
	}

	public class UsCitizen : Person {
		public string SSN { get; private set; }

		public UsCitizen(string firstName, string lastName, DateTime birthDate, GenderEnum gender, string ssn) :
			base(firstName, lastName, birthDate, gender) {
				SSN = ssn;
			}

		public override string ToString() {
			return $"{Name.LastName}, {Name.FirstName} ({Gender}, {Age}; SSN: {SSN})";
		}

		public override bool Equals(object obj) {
			var other = obj as UsCitizen;

			return other != null && SSN == other.SSN && CheckEquality(other);
		}

		public override int GetHashCode() {
			return base.GetHashCode() + 37 * SSN.GetHashCode();
		}
	}

	public interface IParsee: IComparable<IParsee> { }

	public interface IParser<out T> where T: IParsee {
		IEnumerable<T> Parse(string text);
	}

	public class RegexPersonParser: IParser<Person> {

		private readonly Dictionary<string, GenderEnum> salutGenderMap = new Dictionary<string, GenderEnum> {
			["Mr."] = GenderEnum.Male,
				["Mrs."] = GenderEnum.Female,
				["Ms."] = GenderEnum.Female
		};

		private readonly string regex = @"(Mr\.|Mrs\.|Ms\.)\s(\w+)\s(\w+)\swas\sborn\son\s(\d{4}/\d\d/\d\d)";

		public IEnumerable<Person> Parse(string text) {
			var personsMatches = Regex.Matches(text, regex);

			var persons = new Person[personsMatches.Count];

			for (int i = 0; i < personsMatches.Count; i++) {

				var match = personsMatches[i];

				try {
					var salut = match.Groups[1].Value;

					var gender = salutGenderMap[salut];

					var firstName = match.Groups[2].Value;
					var lastName = match.Groups[3].Value;

					var birthDate = DateTime.Parse(match.Groups[4].Value);

					persons[i] = new Person(firstName, lastName, birthDate, gender);
				}
				catch {
					// do some logging here etc
				}
			}

			return persons;
		}
	}

	public static class ParseeExtensions {

		public static IEnumerable<IParsee> RemoveDuplicates(this IEnumerable<IParsee> persons) {
			return new HashSet<IParsee>(persons);			
		}			

		public static string Stringify(this IEnumerable<IParsee> persons) {
			var strBuilder = new StringBuilder();
			foreach (var p in persons)
				strBuilder.AppendLine(p.ToString());

			return strBuilder.ToString();
		}
	}

// Anton: you don't need this!
	public interface IXMLConvertable {}

// Anton: no need to constrain converter on some specific type! It's enough to make it generic.
	public interface IXMLConverter<T> where T : IXMLConvertable
	{
		string ConvertToXml(T convertable);
		bool WriteToXml(List<T> objects, string path);
	}

	public class PersonConverter : IXMLConverter<Person>
	{
			//return $"<Person><Gender value={Gender}/><Name><FirstName>Name.FirstName</FirstName><LastName>Name.LastName</LastName><BirthDate>birthDate</BirthDate><Person>";
		public string ConvertToXml(Person person)
		{
		// Anton: oh no! Never do this! Please read the theory on how to manipulate XML from .Net
		// And when you need to build a string, use string.format of string interpolation ($ strings)!!!
			return "<Person>\n\t<Gender value=" + '"' + person.Gender + '"' + "/>\n\t<Name>\n\t\t<FirstName>"+ person.Name.FirstName + "</FirstName>\n\t\t<LastName>" + person.Name.LastName + "</LastName>\n\t</Name>\n\t<BirthDate value=" + '"' + person.BirthDate + '"' + "</BirthDate>\n</Person>";
		}

		public bool WriteToXml(List<Person> objects, string path)
		{
			Directory.CreateDirectory(path + "/Person");
			int i = 0;
			foreach (Person person in objects)
			{
				System.IO.File.WriteAllText(path + "/Person" + "/Person" + i + ".txt", ConvertToXml(person));
				++i;
			}
			return false;


		}
	}


	/*public class ObjectsToXMLConverter
	{
		private List<IXMLConverter> ParsableObjects;

		public ObjectsToXMLConverter(List<IXMLParseble> objects)
		{
			this.ParsableObjects = objects;
		}

		public void WriteToXml()
		{
		}
	}*/
		
}
