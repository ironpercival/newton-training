<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>


// Anton: good you've tries to use reflection here, but it was not a proper tool for the task. See some comments below. And check my versions of how it could have be done.

namespace Person
{
    using Collections;


    class Person
    {
        private string firstName, lastName;
        private char gender;
        private DateTime date;

        public Person(string firstName, string lastName, DateTime date, bool isMale)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.date = date;
            this.gender = isMale ? 'm' : 'f';
        }

        public int GetAge()
        {
            return DateTime.Now.Year - date.Year;
        }


        public override string ToString()
        {
            return $"{lastName}, {firstName} ({gender}, {this.GetAge()})";
        }

        public void ShowPerson()
        {
            Console.Write(this.ToString());
        }


        //gender first (female, male), then alphabetically by name, then by date of birth
		
		// Anton: think about how to write the same logic with a single condition ;)
        public static bool operator <(Person p1, Person p2)
        {
            if (p1.gender < p2.gender) return true;
            if (p1.gender > p2.gender) return false;

            if (StringComparator.FirstLessThanSecond(p1.firstName, p2.firstName)) return true;
            if (!StringComparator.FirstLessThanSecond(p1.firstName, p2.firstName)) return false;

            if (StringComparator.FirstLessThanSecond(p1.lastName, p2.lastName)) return true;
            if (!StringComparator.FirstLessThanSecond(p1.lastName, p2.lastName)) return false;

            if (p1.date < p2.date) return true;
            if (p1.date > p2.date) return false;

            return false;
        }

        public static bool operator >(Person p1, Person p2)
        {
            return !(p1 < p2);
        }

        public bool Equals(Person person)
        {
            if (this.gender == person.gender
                && this.date == person.date
                && this.firstName.Equals(person.firstName)
                && this.lastName.Equals(person.firstName))
                return true;

            return false;
        }

        public static Person[] GetPersonsFromText(String fileName)
        {
            var file = new StreamReader(fileName);
            string line;
            var list = new Collections.ArrayList();
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split();
                for (int i = 0; i < words.Length; i++)
                {
                    //Mrs.Jessica Brown was born on 1999 / 12 / 31
                    if (words[i].Equals("Mrs") || words[i].Equals("Mr"))
                    {
                        bool isMale = (!words[i].Equals("Mrs"));
                        string firstName = words[i + 1];
                        string lastName = words[i + 2];
                        int year = Int32.Parse(words[i + 6]);
                        int month = Int32.Parse(words[i + 7]);
                        int day = Int32.Parse(words[i + 8]);

                        list.Add(new Person(firstName, lastName, new DateTime(year, month, day), isMale));
                    }
                }
            }
            return list.ToArray();
        }


        //implement quick sort algorithm
        public static void Sort(Person[] a, int l = -1, int r = -1)
        {
            if (l == -1 || r == -1)
            {
                l = 0; r = a.Length - 1;
            }

            Person temp;
            Person x = a[l + (r - l) / 2];

            int i = l;
            int j = r;


            while (i <= j)
            {
                while (a[i] < x) i++;
                while (a[j] > x) j--;
                if (i <= j)
                {
                    temp = a[i];
                    a[i] = a[j];
                    a[j] = temp;
                    i++;
                    j--;
                }
            }
            if (i < r)
                Sort(a, i, r);

            if (l < j)
                Sort(a, l, j);

        }

        public static Person[] RemoveDublicates(Person[] person)
        {
            if (person == null || person.Length == 0) return null;

            Person.Sort(person);
            var list = new ArrayList();
            list.Add(person[0]);

            for (int i = 1; i < person.Length - 1; i++)
            {
                if (!list.GetElement(list.GetSize() - 1).Equals(person[i]))
                {
                    list.Add(person[i]);
                }
            }
            return list.ToArray();
        }

    }

    class UScitizen : Person
    {
        private string ssn;

        public UScitizen(string firstName, string lastName, DateTime date, bool isMale, string ssn)
        : base(firstName, lastName, date, isMale)
        {
            this.ssn = ssn;
        }

        public override string ToString()
        {
            return base.ToString() + $" /ssn={ssn}";
        }
    }
}


namespace Test
{
    class Test
    {
        static void Main(String[] args)
        {

            string text = "Share price of Microsoft (Information Technology, US) dropped to $100." +
                "IT giant Google (Information Technology, US) announced new Chromebook)." +
                "Share price of Microsoft (Information Technology, US) dropped to $100.";

            string regex = @"(\w+)\s+\((\w+\s\w+)\,\s(\w+)\)";

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(regex, "Company.Company");

            List<IComparable> list = Parser.Parser.Parse(text, dictionary);

            Console.WriteLine("Parsed objects:");
            ShowList(list);

            Parser.Parser.RemoveDublicates(list);
            Console.WriteLine("\n\nList after removing dublicates:");
            ShowList(list);

            Console.Read();
        }

        private static void ShowList(List<IComparable> list)
        {
            foreach (IComparable item in list)
            {
                Console.WriteLine(item);
            }
        }
    }
}



namespace Company
{

// Anton: Generic IComparable<T> is much better
    public class Company : IComparable
    {

        public string Name { get; private set; }
        public string Area { get; private set; }
        public string Country { get; private set; }


        public Company(string name, string area, string country)
        {
            this.Name = name;
            this.Area = area;
            this.Country = country;
        }

        public override bool Equals(object obj)
        {
            return this.CompareTo(obj) == 0;
        }

        public int CompareTo(object obj)
        {
            var comp = obj as Company;
			// Anton: if comp == null, there is nothing to do here

            if (String.Compare(this.Name, comp?.Name) == -1) return -1; 
            if (String.Compare(this.Name, comp?.Name) == 1) return 1;
			
			// Anton: try this, it's twice less code and 			
			// if (Name!=comp.Name) return String.Compare(this.Name, comp.Name);

            if (String.Compare(this.Area, comp?.Area) == -1) return -1;
            if (String.Compare(this.Area, comp?.Area) == 1) return 1;

            if (String.Compare(this.Country, comp?.Country) == -1) return -1;
            if (String.Compare(this.Country, comp?.Country) == 1) return 1;

            return 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override string ToString()
        {
            return string.Format("{0} ({1}, {2})", Name, Area, Country);
        }
    }

}


namespace Parser
{

    public static class Parser
    {

        public static void RemoveDublicates(List<IComparable> list)
        {
            list.Sort();
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i - 1].Equals(list[i]))
                {
                    list.RemoveRange(i, 1);
                }

            }
        }


        //Dictionary<regex, class name>
		
		// Anton: no, this is a bad solution. You have many other much better ways to do this. Why not use Func<> or lambdas? 
        public static List<IComparable> Parse(string text, Dictionary<string, string> dictionary)
        {
            var result = new List<IComparable>();

            foreach (KeyValuePair<string, string> kv in dictionary)
            {
                try
                {
                    var objectsFromText = Regex.Matches(text, kv.Key);
                    //Console.WriteLine(objectsFromText.Count);
                    if (objectsFromText.Count == 0) continue;

                    Type type = Type.GetType(kv.Value);
                    if (!HasInterfase(type, Type.GetType("System.IComparable")))
                        throw new Exception("Object should implement IComparable interface.");


                    for (int i = 0; i < objectsFromText.Count; ++i)
                    {
                        var match = objectsFromText[i];

                        List<object> argms = new List<object>();
                        for (int j = 0; j < match.Length; j++)
                        {
                            if (!match.Groups[j + 1].Value.Equals(""))
                            {
                                argms.Add(match.Groups[j + 1].Value);
                            }
                        }

                        result.Add((IComparable)Activator.CreateInstance(type, argms.ToArray()));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return result;
        }


        public static bool HasInterfase(Type type, Type requiredInterface)
        {
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].Equals(requiredInterface)) return true;
            }
            return false;
        }

    }

}




namespace Collections
{
    using Person;

// Anton: please check the theory. .NET has a way to compare string without inventing a wheel.
    class StringComparator
    {

        public static bool FirstLessThanSecond(string s1, string s2)
        {
            int length = (s1.Length < s2.Length) ? s1.Length : s2.Length;

            for (int i = 0; i < length; i++)
            {
                if (s1[i] < s2[i]) return true;
                else if (s1[i] > s2[i]) return false;
            }
            return (s1.Length < s2.Length) ? true : false;
        }

    }

    class ArrayList
    {
        private Person[] allocator;
        private int size;

        public ArrayList(int capacity = 2)
        {
            allocator = new Person[2]; size = 0;
        }

        public void Add(Person person)
        {
            if (size == allocator.Length)
                Resize();
            allocator[size++] = person;
        }


        public Person[] ToArray()
        {
            Person[] copy = new Person[allocator.Length];
            for (int i = 0; i < allocator.Length; i++)
            {
                copy[i] = allocator[i];
            }
            return copy;
        }

        public Person GetElement(int index)
        {
            return allocator[index];
        }


        public int GetSize()
        {
            return size;
        }


        private void Resize()
        {
            Person[] copy = new Person[allocator.Length * 2];
            for (int i = 0; i < allocator.Length; i++)
            {
                copy[i] = allocator[i];
            }
            allocator = copy;
        }

    }
}
