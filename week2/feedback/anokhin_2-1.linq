<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>


// Anton: very good, though IComparable should be changed to smth more specific to your task 

namespace Anokhin
{
    public struct Name : IComparable<Name>
    {
        public String firstname, lastname;
        public override bool Equals(object o)
        {
            if (!(o is Name)) return false;
            Name n = (Name)o;
            return this.firstname.Equals(n.firstname) && this.lastname.Equals(n.lastname);
        }
        public int CompareTo(Name n)
        {
            int a = lastname.CompareTo(n.lastname);
            if (a == 0) return firstname.CompareTo(n.firstname);
            return a;
        }
    }
    public class Person : IComparable
    {
        bool male;
        DateTime birthdate;
        Name name;
        int age = -1;
        public Person(String firstname, String lastname, bool male, DateTime birthdate)
        {
            name.firstname = firstname;
            name.lastname = lastname;
            this.male = male;
            this.birthdate = birthdate;
        }
        public Person(String firstname, String lastname, bool male, int age)
        {
            name.firstname = firstname;
            name.lastname = lastname;
            this.male = male;
            this.age = age;
        }

        public int CompareTo(object o)
        {
            if (!(o is Person)) return -1;
            Person p = o as Person;
            int a = this.male.CompareTo(p.male);
            if (a == 0)
            {
                int b = this.name.CompareTo(p.name);
                if (b == 0) return this.birthdate.CompareTo(p.birthdate);
                else return b;
            }
            return a;
        }
        public override bool Equals(object o)
        {
            if (!(o is Person)) return false;
            Person p = o as Person;
            return this.male == p.male &&
                this.birthdate.Equals(p.birthdate) &&
                this.name.Equals(p.name);
        }
        public override string ToString()
        {
            char male = this.male ? 'm' : 'f';
            int ageCounted;
            if (this.age == -1)
            {
                DateTime now = DateTime.Today;
                ageCounted = now.Year - birthdate.Year;
                if (birthdate > now.AddYears(-ageCounted)) ageCounted--;
            }
            else ageCounted = this.age;

            return $"{name.lastname}, {name.firstname} ({male}, {ageCounted})";
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
    public class PersonList
    {
        private Person[] ar;
        public Person[] List { get { return ar; } }
        private int size, last;
        public PersonList() : this(50)
        { }
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
    class Fourth
    {
        public static void Main(String[] args)
        {
            Person p = new Person("Sasha", "Anokhin", true, new DateTime(1997, 4, 1));
            Person p2 = new Person("Vika", "Dochkina", false, new DateTime(1997, 12, 1));
            Person p3 = new Person("Sasha", "Anokhin", true, new DateTime(1997, 4, 1));
            Person p4 = new Person("Sasha", "Anokhin", true, new DateTime(1997, 4, 1));
            Person p5 = new Person("Vika", "Dochkina", false, new DateTime(1997, 12, 1));
            
            String a = p +"One morning, when Gregor Samsa woke from troubled dreams, he found himself transformed in his bed into a horrible vermin. He lay on his armour-like back\n" 
            + p2 + "The bedding was hardly able to cover it and seemed ready to slide off any moment.\n" + p +"It wasn't a dream. His room, a proper human room although a little too small, lay peacefully between its four familiar walls.\n"+p4
            +"Far far away, behind the word mountains, far from the countries Vokalia and Consonantia, there live the blind texts.\n"+p3
            +"Separated they live in Bookmarksgrove right at the coast of the Semantics, a large language ocean\n"+p5;
            
            var l = Parser.Parse(a, Person.ParseText);
            System.Console.WriteLine("---------\nParsed string\n---------");            
            foreach (var el in l)
                System.Console.WriteLine(el.ToString());
            Parser.RemoveDublicates(l);
            System.Console.WriteLine("---------\nAfter removing dublicates\n---------");
            foreach (var el in l)
                System.Console.WriteLine(el.ToString());
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
}