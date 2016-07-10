<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.IO</Namespace>
</Query>


// Anton: ok, good that you've learned how to use XmlSerializer. But in this case we have several entities - Person, Company etc. How would you handle the serialization of a list of all these entities? Your solution is technically correct, but hardly usable in real scenario. Imagine you've parsed a list of 5 types of entities. Now you need to serialize it to XML. What will you do?

namespace Anokhin
{
    class Program
    {
        public static void Main(String[] args)
        {
           
            Person p1 = new Person("Vika", "Dochkina", false, new DateTime(1997, 12, 1));
           
            Name n = new Name(){Firstname ="Oleksandr", Lastname="Anokhin"};
            Person p2 = new Person(){Male=true, Birthdate=new DateTime(1997, 4, 1), Name=n};
            
            XML<Person>.Serialize(p1, "1");
            XML<Person>.Serialize(p2, "2");

            FileInfo fi2 = new FileInfo("xml/Anokhin.Person/2.xml");
            System.Console.WriteLine(XML<Person>.Deserialize(fi2));
            
            PersonList pl = new PersonList();
            pl.Add(p1);
            pl.Add(p2);
            XML<PersonList>.Serialize(pl, "3");
            

            FileInfo fi3 = new FileInfo("xml/Anokhin.PersonList/3.xml"); 
            PersonList res = XML<PersonList>.Deserialize(fi3);
            System.Console.WriteLine("\n\n"+res.ToString());
        }
    }
    [XmlRoot("Name")]
    public struct Name : IComparable<Name>
    {
        [XmlElement("Firstname")]
        public string Firstname {get;set;}
        [XmlElement("Lastname")]        
        public string Lastname {get;set;}
        public override bool Equals(object o)
        {
            if (!(o is Name)) return false;
            Name nn = (Name)o;
            return Firstname.Equals(nn.Firstname) && Lastname.Equals(nn.Lastname);
        }
        public int CompareTo(Name n)
        {
            int a = Lastname.CompareTo(n.Lastname);
            if (a == 0) return Firstname.CompareTo(n.Firstname);
            return a;
        }
    }
    [XmlRoot("Person")]
    public class Person : IComparable
    {
        [XmlAttribute("Male")] 
        public bool Male  {get;set;}
        Name name;
        [XmlElement("Name")]
        public Name Name {get{return name;}set{name=value;}}
        [XmlElement("Birthdate")]
        public DateTime Birthdate  {get;set;}
        [XmlIgnore]
        int age = -1;
        public Person(){}
        public Person(String firstname, String lastname, bool male, DateTime birthdate)
        {
            name.Firstname = firstname;
            name.Lastname = lastname;
            this.Male = male;
            this.Birthdate = birthdate;
        }
        public Person(String firstname, String lastname, bool male, int age)
        {
            name.Firstname = firstname;
            name.Lastname = lastname;
            this.Male = male;
            this.age = age;
        }

        public int CompareTo(object o)
        {
            if (!(o is Person)) return -1;
            Person p = o as Person;
            int a = this.Male.CompareTo(p.Male);
            if (a == 0)
            {
                int b = this.name.CompareTo(p.name);
                if (b == 0) return this.Birthdate.CompareTo(p.Birthdate);
                else return b;
            }
            return a;
        }
        public override bool Equals(object o)
        {
            if (!(o is Person)) return false;
            Person p = o as Person;
            return this.Male == p.Male &&
                this.Birthdate.Equals(p.Birthdate) &&
                this.name.Equals(p.name);
        }
        public override string ToString()
        {
            char male = this.Male ? 'm' : 'f';
            int ageCounted;
            if (this.age == -1)
            {
                DateTime now = DateTime.Today;
                ageCounted = now.Year - Birthdate.Year;
                if (Birthdate > now.AddYears(-ageCounted)) ageCounted--;
            }
            else ageCounted = this.age;

            return $"{name.Lastname}, {name.Firstname} ({male}, {ageCounted})";
        }
        public static Person ExtractPerson(String a)
        {
            String firstname, lastname;
            bool male;
            DateTime d;
            try
            {
                String[] ar = a.Split(' ');
                male = ar[0].Equals("Mr.");
                firstname = ar[1];
                lastname = ar[2];
                d = DateTime.ParseExact(Regex.Match(a, @"\b\d{4}\/\d{2}\/\d{2}\b").Value, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong string given " + a);
            }
            return new Person(firstname, lastname, male, d);
        }
        public static Person ExtractPersonExactly(String a)
        {
            String res = Regex.Match(a, @"\w+,\s\w+\s\(\w,\s\d{1,3}\)").Value;
            if (res.Equals("")) throw new ArgumentException("Wrong string " + a);
            res = Regex.Replace(res, "[(),]", "");
            String[] elems = res.Split(' ');
            bool male = elems[2] == "m" ? true : false;
            return new Person(elems[1], elems[0], male, Int32.Parse(elems[3]));
        }
        // in the example every new element was in new line, so I thought I've got to split the text by lines. 
        public static List<IComparable> ParseText(String a)
        {
            String[] b = a.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            List<IComparable> results = new List<IComparable>();
            foreach (String c in b)
            {
                try
                {
                    results.Add(ExtractPersonExactly(c));
                }
                catch (ArgumentException e)
                {
                }
            }
            return results;
        }
    }
    public class USCitizen : Person
    {
        int SSN { get; set; }
        public USCitizen(String firstname, String lastname, bool male, DateTime birthdate, int ssn) : base(firstname, lastname, male, birthdate)
        {
            this.SSN = ssn;
        }
        public override bool Equals(object o)
        {
            USCitizen u;
            if ((u = o as USCitizen) !=null) return base.Equals(o) && this.SSN == u.SSN;
            return false;
        }
        public override String ToString()
        {
            return base.ToString()+ " SSN: "+SSN;
        }
    }
    [XmlRoot("PersonList")]
    public class PersonList
    {
        private Person[] ar;
        [XmlArray(IsNullable = true)]
        [XmlArrayItem(typeof(Person), IsNullable = false)]
        public Person[] List { get { return ar; } set{ar = value;}}
        private int size;
        [XmlAttribute("total from 0")]
        public int last;
        public PersonList() : this(50) { }
        public PersonList(int size)
        {
            if (size <= 0) size = 1;
            this.size = size;
            ar = new Person[size];
            last = -1;
        }
        private void Extend()
        {
            size = size * 2;
            Array.Resize(ref ar, size);
        }
        public void Add(Person p)
        {
            if (last == size - 1) Extend();
            ar[++last] = p;
        }
        public bool Contains(Person p)
        {
            for (int i = 0; i <= last; i++)
                if (ar[i].Equals(p)) return true;
            return false;
        }
        public static bool Contains(Person[] array, Person p)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] != null && array[i].Equals(p)) return true;
            return false;
        }
        public void Sort()
        {
            SortingUtilities.Quicksort<Person>(ar, 0, last, (a, b) => a.CompareTo(b));
        }
        public void Remove(Person p)
        {
            Person[] ar2 = new Person[size];
            int last2 = last;
            for (int i = 0, k = 0; i <= last; i++)
            {
                if (!ar[i].Equals(p))
                {
                    ar2[k] = ar[i];
                    k++;
                }
                else last2--;
            }
            ar = ar2;
            last = last2;
        }

        public void RemoveDublicates()
        {
            Person[] unique = new Person[last + 1];
            int k = 0;
            for (int i = 0; i <= last; i++)
            {
                if (!Contains(unique, ar[i]))
                {
                    unique[k] = ar[i];
                    k++;
                }
            }
            last = k - 1;
            Array.Resize(ref unique, size);
            ar = unique;
        }
        public override String ToString()
        {
            String a = "";
            for (int i = 0; i <= last; i++)
                a += $"{ar[i]}\n";
            return a;
        }
    }
    public class SortingUtilities
    {
        public static void Quicksort<T>(T[] elements, int left, int right, Func<T, T, int> compare)
        {
            int i = left, j = right;
            T pivot = elements[(left + right) / 2];
            while (i <= j)
            {
                while (compare(elements[i], pivot) < 0)
                    i++;
                while (compare(elements[j], pivot) > 0)
                    j--;
                if (i <= j)
                {
                    T tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;
                    i++;
                    j--;
                }
            }
            if (left < j)
                Quicksort<T>(elements, left, j, compare);
            if (i < right)
                Quicksort<T>(elements, i, right, compare);
        }
    }
    public class Parser
    {
        public static List<IComparable> Parse(String text, params Func<String, List<IComparable>>[] meth)
        {
            List<IComparable> l = new List<IComparable>();
            List<IComparable> res;
            for (int i = 0; i < meth.Length; i++)    
                if ((res = meth[i](text)) != null)
                    l.AddRange(res);
            return l;
        }
        public static void RemoveDublicates(List<IComparable> l)
        {
            for(int i = 0; i < l.Count; i++)
            {
                var el = l[i];
                for (int k = i+1; k < l.Count; k++)
                {
                    if (el.Equals(l[k]) )
                        l.RemoveAt(k--);
                }
            }
        }
    }
        public class XML<T>
    {
        public static void Serialize(T obj)
        {
            Serialize(obj, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff"));
        }
        public static void Serialize(T obj, string file)
        {
            file = getDir(typeof(T)) + file;

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            using (FileStream fs = File.Create(file+".xml"))
            {
                    serializer.Serialize(fs, obj, ns);
            }
        }
        public static T Deserialize(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xml))
            {
                try
                {
                    return (T) serializer.Deserialize(reader);
                }
                catch(InvalidOperationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Wrong xml for this data type: " +typeof(T)+"\n"+ex.Message);
                    Console.ResetColor();
                }
                return default(T);
            }
        }
        public static T Deserialize(FileInfo f)
        {
            string text = File.ReadAllText(f.FullName);
            return Deserialize(text);
        }
        private static string getDir(System.Type type)
        {
            char separator = Path.DirectorySeparatorChar;
            string directory = Directory.GetCurrentDirectory() + separator +"xml"+separator+type+separator;
            Directory.CreateDirectory(directory);
            return directory;            
        }
    }
    
}