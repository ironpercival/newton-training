using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NewtonTraining {


	class Program {
		static void Main(string[] args) {

			var text = @"1. Implement a type Person. A Person has a gender, a name, and a date of birth. A name is a full name with first name and last name. 2. Person can be printed to Console in the format: <lastname>, <firstname> (<m/f>, <age>). Example: Shabinskiy, Anton (m, 29) 3. Implement extracting person data from texts. Assume that person always appears in text as Mr. John Smith was born on 2001/11/03 or Mrs. Jessica Brown was born on 1999/12/31 etc. You need to extract such data and get an array of Person objects. 4. Implement a special type of person - a US citizen. US citizens have a Social Security Number (SSN). 5. For an array of Person objects, implement a procedure to remove duplicates and a procedure to sort an array. Two persons are equal if they have the same gender, full name, and date of birth. For sorting, persons are sorted by gender first (female, male), then alphabetically by name, then by date of birth. NOTE: do not use LINQ, collections, and any framework methods for sorting etc. Use only plain arrays and implement your own logic. Also, use types wisely (classes, structs, enums etc) to make your implementation elegant and efficient. And finally put some duplicates here  Mr. John Smith was born on 2001/11/03 or Mrs. Jessica Brown was born on 1999/12/31 to check how it works.";

			var parsers = new List<IParser<IParsee>>();
			parsers.Add(new RegexPersonParser());
			// parsers.Add(new AnyParser());
			// parsers.Add(new AnotherCrazyParser());
			// etc ...

			var parsedEntities = parsers.SelectMany(p => p.Parse(text)).RemoveDuplicates().ToList();

			Console.WriteLine(parsedEntities.Stringify());

			Console.ReadKey();
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

	public class Person : IComparable<Person>, IParsee {

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

		public int CompareTo(Person other) {
			if (!Gender.Equals(other.Gender))
				return Gender.CompareTo(other.Gender);

			if (!Name.Equals(other.Name))
				return Name.CompareTo(other.Name);

			if (BirthDate != other.BirthDate)
				return BirthDate.CompareTo(other.BirthDate);

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

	public interface IParsee { }

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
}
