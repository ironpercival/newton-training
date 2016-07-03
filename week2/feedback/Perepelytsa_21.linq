<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>

namespace PersonImpl
{

// Anton: that was a bad idea to work with IComparable everywhere. You should have used your own interface for that purpose. You should force implementers to implement your Extract method - this is normally done with an interface like IExtractable or smth like that. 

    static class MyParser
    {
        //c# supports covariance of generic interfaces and generic delegates, not classes(List<> is not supported)
        public delegate IComparable[] Extract(String input); // Anton: IComparable is too general for this!!! You should return a more concrete interface. 
        //examples:
        // Mr. John Smith, born on 2001/06/15 (class Person)
        // Company Microsoft (IT, USA)          (class Company)
        // FC Dynamo (Kyiv, Ukraine, est. 1927)   (class FootballClub)
        //
        // to add other class you need to implement IComparable and to write an Extract method which parses input string into array of objects of that class
        // you can use  List<String> GetEntries(String input, String regexStr) to apply regex on input string and then parse each entry into object

        // main() here just to locate it easier
        public static void Main()
        {
            String s = " 123 abc Mr. John Smith, born on 2001/06/15 abc123 Company Microsoft (IT, USA) gdfgdf Mrs. Jane Smith, born on 1999/11/03 32 asd" +
                "abc 123 FC Dynamo (Kyiv, Ukraine, est. 1927) asdad FC Dynamo (Kyiv, Ukraine, est. 1927)";
            Extract[] ex = { Person.Extract, FootballClub.Extract, Company.Extract};         
            List<IComparable> res = MyParser.ParseAll(s,ex);
            Console.WriteLine("input string:");
            Console.WriteLine(s);
            Console.WriteLine();
            
            foreach (IComparable t in res)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();
            //removing duplicates is implemented by sorting collection by type, splitting it, removing duplicates from each type, merging back
            List<IComparable> newres = RemoveDuplicates(res);
            Console.WriteLine("removed duplicates:");
            foreach (IComparable t in newres)
            {
                Console.WriteLine(t.ToString());
            }

        }


        public static List<IComparable> RemoveDuplicates(this List<IComparable> list)
        {
            IComparable[] objects = list.ToArray();
            QuicksortByType(objects, 0, objects.Length - 1);
            IComparable[][] objectsByType = SplitByType(objects);
            List<IComparable> flattened = new List<IComparable>();
            for (int i = 0; i < objectsByType.Length; i++)
            {
                objectsByType[i] = RemoveDuplicatesOneType(objectsByType[i]);
                flattened.AddRange(objectsByType[i]);
            }
            return flattened;
        }

        // only for arrays of single type
		// Anton: no need to do this if you would use more common methods to compare object - overriding Equals() for example
        private static IComparable[] RemoveDuplicatesOneType(IComparable[] source)
        {

            IComparable[] newsource = new IComparable[source.Length];
            Quicksort(source, 0, source.Length - 1);
            int i = 0, j = 0, count = 0;
            while (i < source.Length - 1)
            {
                if (source[i].CompareTo(source[i + 1]) == 0)
                {
                    newsource[i - count] = source[i];
                    j = i;
                    i++;
                    while (i < source.Length && source[j].CompareTo(source[i]) == 0)
                    {
                        i++;
                        count++;
                    }
                }
                else
                {
                    newsource[i - count] = source[i];
                    i++;
                }
            }
            if (i == source.Length - 1)
            {
                newsource[i - count] = source[i];
            }
            IComparable[] resArray = new IComparable[source.Length - count];
            Array.Copy(newsource, 0, resArray, 0, source.Length - count);
            return resArray;
        }
        //only for sorted arrays (by type)
        private static IComparable[][] SplitByType(IComparable[] source)
        {
            int count = 0;
            Type t = null;
            foreach (IComparable obj in source)
            {
                if (obj.GetType() != t)
                {
                    count++;
                    t = obj.GetType();
                }
            }

            IComparable[][] res = new IComparable[count][];
            int currTypeStart = 0, currTypeNumb = 0;
            t = source[0].GetType();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].GetType() != t)
                {
                    res[currTypeNumb] = new IComparable[i - currTypeStart];
                    Array.Copy(source, currTypeStart, res[currTypeNumb], 0, i - currTypeStart);
                    currTypeStart = i;
                    currTypeNumb++;
                    t = source[i].GetType();
                }
            }
            res[currTypeNumb] = new IComparable[source.Length - currTypeStart];
            Array.Copy(source, currTypeStart, res[currTypeNumb], 0, source.Length - currTypeStart);
            return res;
        }
        //sort array by type
        private static void QuicksortByType(Object[] source, int left, int right)
        {
            int i = left, j = right;
            Object pivot = source[(left + right) / 2];
            while (i <= j)
            {

                while (source[i].GetType().FullName.CompareTo(pivot.GetType().FullName) < 0)
                {
                    i++;
                }
                while (source[j].GetType().FullName.CompareTo(pivot.GetType().FullName) > 0)
                {
                    j--;
                }
                if (i <= j)
                {
                    Object tmp = source[i];
                    source[i] = source[j];
                    source[j] = tmp;

                    i++;
                    j--;
                }
            }

            if (left < j)
            {
                QuicksortByType(source, left, j);
            }
            if (i < right)
            {
                QuicksortByType(source, i, right);
            }
        }


        private static void Quicksort(IComparable[] source, int left, int right)
        {
            int i = left, j = right;
            IComparable pivot = source[(left + right) / 2];

            while (i <= j)
            {
                while (source[i].CompareTo(pivot) < 0)
                {

                    i++;
                }
                while (source[j].CompareTo(pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    IComparable tmp = source[i];
                    source[i] = source[j];
                    source[j] = tmp;
                    i++;
                    j--;
                }
            }

            if (left < j)
            {
                Quicksort(source, left, j);
            }
            if (i < right)
            {
                Quicksort(source, i, right);
            }
        }

        public static List<IComparable> ParseAll(String input,Extract[] parsers)
        {
            int count = 0;
            IComparable[][] alloc = new IComparable[parsers.Length][];
            for (int i = 0; i < parsers.Length; i++)
            {
                if (parsers[i](input) != null)
                {
                    alloc[i] = parsers[i](input);
                    count += alloc[i].Length;
                }
            }
            
            List<IComparable> res = new List<IComparable>();
            for (int i = 0; i < parsers.Length; i++)
            {
                if (alloc[i] != null)
                {
                    res.AddRange(alloc[i]);
                    count -= alloc[i].Length;
                }
            }
            return res;
        }

        
        public static List<String> GetEntries(String input, String regexStr)
        {
            var regex = new Regex(regexStr);
            MatchCollection matches = regex.Matches(input);
            List<String> result = new List<String>();
            foreach (Match mat in matches)
            {
                result.Add(mat.Value);
            }
            return result;
        }
    }

    class FootballClub : IComparable
    {
        public String Name { get;  }
        public String Country { get; }
        public String City { get;  }
        public DateTime Established { get; }
        public FootballClub(String name, String country, String city, DateTime established)
        {
            this.Name = name;
            this.Country = country;
            this.City = city;
            this.Established = established;
        }
        override public String ToString()
        {
            return Name + " (" + Country + ", " + City + ", est." + Established.Year + ")";
        }

        public static FootballClub[] Extract(String input)
        {
            List<String> clubs=MyParser.GetEntries(input, @"FC\D+\((\D+\d{4})\)");          
            FootballClub[] result = new FootballClub[clubs.Count];
            int i = 0;

                foreach (String s in clubs)
                {
                String name;
                String country;
                String city;
                DateTime established;
                String[] parts = s.Split(' ');

                name = parts[1].Substring(0, parts[1].Length);
                city = parts[2].Substring(1, parts[2].Length - 2);
                country = parts[3].Substring(0, parts[3].Length - 1);

                established = DateTime.ParseExact(parts[5].Substring(0, 4), "yyyy", System.Globalization.CultureInfo.InvariantCulture);
                FootballClub res = new FootballClub(name, country, city, established);
                result[i++] = res;
            }
            return result;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            FootballClub other = obj as FootballClub;

            if (this.Country.CompareTo(other.Country) > 0)
                return 1;
            else if (this.Country.CompareTo(other.Country) < 0)
                return -1;
            else
            {
                if (this.City.CompareTo(other.City) > 0)
                {
                    return 1;

                }
                else if (this.City.CompareTo(other.City) < 0)
                {
                    return -1;
                }
                else
                {
                    if (this.Name.CompareTo(other.Name) == 0)
                    {
                        return DateTime.Compare(this.Established, other.Established);
                    }
                    else return this.Name.CompareTo(other.Name);
                }
            }
        }
    }
    enum Gender { m, f };
    class Person : IComparable
    {
        public Gender Gender { get; }
        public String FirstName { get; }
        public String LastName { get; }
        public DateTime DateOfBirth { get; }

        public Person(Gender gender, String firstName, String lastName, DateTime birth)
        {
            this.Gender = gender;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.DateOfBirth = birth;
        }
        override public String ToString()
        {
            return LastName + ", " + FirstName + " (" + Gender + ", " + GetAge(DateOfBirth) + ")";
        }

        public int GetAge(DateTime DateOfBirth)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (DateOfBirth.Year * 100 + DateOfBirth.Month) * 100 + DateOfBirth.Day;

            return (a - b) / 10000;
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Person other = obj as Person;

            if (this.Gender > other.Gender)
                return 1;
            else if (this.Gender < other.Gender)
                return -1;
            else
            {
                if (this.LastName.CompareTo(other.LastName) > 0)
                {
                    return 1;

                }
                else if (this.LastName.CompareTo(other.LastName) < 0)
                {
                    return -1;
                }
                else
                {
                    if (this.FirstName.CompareTo(other.FirstName) == 0)
                    {
                        return DateTime.Compare(this.DateOfBirth, other.DateOfBirth);
                    }
                    else return this.FirstName.CompareTo(other.FirstName);
                }
            }
        }
  
        public static Person[] Extract(String input)
        {
            //Mr. John Smith, born on 2001/06/15
            List<String> people = MyParser.GetEntries(input, @"(Mr|Mrs)\D+, born on \d{4}/((0\d)|(1[012]))/(([012]\d)|3[01])");
            Person[] result = new Person[people.Count];
            int i = 0;
            foreach (String s in people)
            {
                Gender Gender = Gender.m;
                String FirstName;
                String LastName;
                DateTime DateOfBirth;
                String[] parts = s.Split(' ');
                if (parts[0].Equals("Mrs.")) Gender = Gender.f;
                FirstName = parts[1]; 
                LastName = parts[2].Substring(0, parts[2].Length - 1);
                DateOfBirth = DateTime.ParseExact(parts[5], "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                Person res = new Person(Gender, FirstName, LastName, DateOfBirth);
                result[i++] = res;
            }
            return result;
        }
    }

    class Company : IComparable
    {

        public String Name { get; }
        public String Segment { get; }
        public String Location { get; }

        public Company(String name, String segment, String location)
        {
            this.Name = name;
            this.Segment = segment;
            this.Location = location;
        }
        override public String ToString()
        {
            return Name + " (" + Segment + ", " + Location + ")";
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Company other = obj as Company;

            if (this.Segment.CompareTo(other.Segment) > 0)
            {
                return 1;

            }
            else if (this.Segment.CompareTo(other.Segment) < 0)
            {
                return -1;
            }
            else
            {
                if (this.Location.CompareTo(other.Location) > 0)
                {
                    return 1;

                }
                else if (this.Location.CompareTo(other.Location) < 0)
                {
                    return -1;
                }
                else
                {
                    return this.Name.CompareTo(other.Name);
                }
            }
        }

        public static Company[] Extract(String input)
        {
            //Company Microsoft (IT, USA)
            List<String> companies = MyParser.GetEntries(input, @"Company \D+ \((\D+,\D+)\)");
            Company[] result = new Company[companies.Count];
            int i = 0;
           foreach (String s in companies)
            {
                String Name;
                String Segment;
                String Location;
                String[] parts = s.Split(' ');
                Name = parts[1];
                Segment = parts[2].Substring(1, parts[2].Length - 2);
                Location = parts[3].Substring(0, parts[3].Length - 1);
                Company res = new Company(Name, Segment, Location);
                result[i++] = res;
            }
            return result;
        }
    }

}