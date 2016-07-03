<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>DateTime</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Collections</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
</Query>

namespace NI_Week1
{
// Anton: check some comments below. Also, check my version.


// Anton: this class does not implement IComparable!
    public class FootballClub : IComparable
    {
        //paramethers
        private string name;
        private string city;
        private string country;
        private int est_year; // Anton: no underscores in names!

        //getters
		// Anton: please pay attention to naming - we do not use this Java-style names in C#!
		// Anton: C# has properties - there is no need to write explicit getters!
        public string getName() { return name; }
        public string getCity() { return city; }
        public string getCountry() { return country; }
        public int getEST() { return est_year; }

        public string printClub(FootballClub FC)
        {
		// Anton: use string.format or $"FC {FC.getName()} {FC.getCity()} ..."
            return "FC " + FC.getName() + " (" + FC.getCity() + ", " + FC.getCountry() + ", est. " + FC.getEST() + ")";
        }

        public FootballClub(string nname, string ccity, string ccountry, int eest_year)
        {
            this.name = nname;
            this.city = ccity;
            this.country = ccountry;
            this.est_year = eest_year;
            return;
        }

        public static bool operator <(FootballClub fc1, FootballClub fc2)
        {
            var thisName = fc1.getName().ToLower();
            var otherName = fc2.getName().ToLower();
            var thisCity = fc1.getCity().ToLower();
            var otherCity = fc2.getCity().ToLower();
            var thisCountry = fc1.getCountry().ToLower();
            var otherCountry = fc2.getCountry().ToLower();

// Anton: what's going on here? Why CompareTo + Equals?
            if (thisName.CompareTo(otherName).Equals(fc1.getName().ToLower())) return true;
            if (!thisName.CompareTo(otherName).Equals(fc2.getName().ToLower())) return false;

            if (thisCity.CompareTo(otherCity).Equals(c1.getCity().ToLower())) return true;
            if (!thisCity.CompareTo(otherCity).Equals(c2.getCity().ToLower())) return false;

            if (thisCountry.CompareTo(otherCountry).Equals(c1.getCountry().ToLower())) return true;
            if (!thisCountry.CompareTo(otherCountry).Equals(c2.getCountry().ToLower())) return false;


// Anton: 
// This can be done much simpler:
// return fc1.getEST() < fc2.getEST();
            if (fc1.getEST() < fc2.getEST()) return true;
			
			// Anton: this always returns false
            if (fc1.getEST() > fc2.getEST()) return false;
            else return false;
        }

        public static bool operator >(FootballClub fc1, FootballClub fc2)
        {
            return !(fc1 < fc2);
        }

        public bool Equals(FootballClub fc)
        {
		// Anton: this can be just one return statement.  
		// Actually you wrote: if(true) return true; else return false;
            if (this.getName().Equals(fc.getName()) && this.getCity().Equals(fc.getCity()) && this.getCountry().Equals(fc.getCountry()) && this.getEST() == fc.getEST())
                return true;
            else return false;
        }

    }

    public class Company : IComparable
    {
        //paramethers
        private string name;
        private string activity;
        private string country;

        //getters
        public string getName() { return name; }
        public string getActivity() { return activity; }
        public string getCountry() { return country; }

        public string printCompany(Company c)
        {
            return c.getName() + " (" + c.getActivity() + ", " + c.getCountry() + ")";
        }

        public Company(string nname, string ccountry, string aactivity)
        {
            this.name = nname;
            this.activity = aactivity;
            this.country = ccountry;
        }

        public static bool operator <(Company c1, Company c2)
        {
            var thisName = c1.getName().ToLower();
            var otherName = c2.getName().ToLower();
            var thisActivity = c1.getActivity().ToLower();
            var otherActivity = c2.getActivity().ToLower();
            var thisCountry = c1.getCountry().ToLower();
            var otherCountry = c2.getCountry().ToLower();

            if (thisName.CompareTo(otherName).Equals(c1.getName().ToLower())) return true;
            if (!thisName.CompareTo(otherName).Equals(c2.getName().ToLower())) return false;

            if (thisActivity.CompareTo(otherActivity).Equals(c1.getActivity().ToLower())) return true;
            if (!thisActivity.CompareTo(otherActivity).Equals(c2.getActivity().ToLower())) return false;

            if (thisCountry.CompareTo(otherCountry).Equals(c1.getCountry().ToLower())) return true;
            if (!thisCountry.CompareTo(otherCountry).Equals(c2.getCountry().ToLower())) return false;

            else return false;
        }

        public static bool operator >(Company c1, Company c2)
        {
            return !(c1 < c2);
        }

        public bool Equals(Company c)
        {
            if (this.getName().Equals(c.getName()) && this.getActivity().Equals(c.getActivity()) && this.getCountry().Equals(c.getCountry()))
                return true;
            else return false;
        }

    }

    public class Person : IComparable
    {
        //paramethers
        private int ageP;
        private string nameP;
        private char genderP;
        private string getGenderP(string g)
        {
            if (g == "Mr")
            {
                genderP = 'm';
                return "male";
            }
            if (g == "Ms" || g == "Miss" || g == "Mrs")
            {
                genderP = 'f';
                return "female";
            }
            else { genderP = 'u'; return "unknown"; }
        }

        public char getGender() { return genderP; }

        private DateTime dateofb;

        // age of person
        public static int getAge(Person p)
        {
            DateTime now = DateTime.Today;
            DateTime birthday = Convert.ToDateTime(p.dateofb);
            int age = now.Year - birthday.Year;
            if (now < birthday.AddYears(age)) age--;
            return age;
        }

        class FullName
        {
            string FirstName;
            string LastName;
            public string getFullName()
            {
                return LastName + ", " + FirstName;
            }
            public FullName(string firstName, string secondName)
            {
                this.FirstName = firstName;
                this.LastName = secondName;
            }
        }

        public Person(FullName fn, DateTime dt, string g)
        {
            this.dateofb = dt;
            this.ageP = getAge(this);
            this.getGenderP(g);
            this.nameP = fn.getFullName();
        }

        public Person(string firstName, string secondName, DateTime dt, string gender)
        {
            FullName fn = new FullName(firstName, secondName);
            this.dateofb = dt;
            this.ageP = getAge(this);
            this.getGenderP(gender);
            this.nameP = fn.getFullName();
        }

        public string printPerson(Person P)
        {
            return P.nameP + "(" + P.genderP + "," + P.ageP + ")";
        }

        public Person(Person old)
        {
            this.ageP = old.ageP;
            this.genderP = old.genderP;
            this.nameP = old.nameP;
        }


        //comparing part
        //Person comparing. 1-gender, 2-full name, 3-age
        public static bool operator <(Person p1, Person p2)
        {
            if (p1.getGender() < p2.getGender()) return true;
            if (p1.getGender() > p2.getGender()) return false;

            var thisName = p1.nameP.ToLower();
            var otherName = p2.nameP.ToLower();

            if (thisName.CompareTo(otherName).Equals(p1.nameP.ToLower())) return true;
            if (!thisName.CompareTo(otherName).Equals(p1.nameP.ToLower())) return false;

            if (getAge(p1) < getAge(p2)) return true;
            if (getAge(p1) > getAge(p2)) return false;

            else return false;
        }

        public static bool operator >(Person p1, Person p2)
        {
            return !(p1 < p2);
        }
        public bool Equals(Person person)
        {
            if (this.getGender() == person.getGender() && getAge(this) == getAge(person) && this.nameP.Equals(person.nameP))
                return true;
            else return false;
        }


        public class Citizen : Person
        {
            protected int SSN;
            public Citizen(Person person, int ssn)
                : base(person)
            {
                this.SSN = ssn;
            }
        }
    }

    public class Parsers
    {
        //It can read messages like:
        //Mr. John Smith was born on 1983 / 11 / 05
        public static Queue<Person> PersonsParser(String fileN)
        {
            var q = new Queue<Person>();
            var file = new StreamReader(fileN);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split();
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Equals("Mrs") || words[i].Equals("Mr"))
                    {
                        string gender = words[i];
                        bool isMale = (!words[i].Equals("Mrs"));
                        string firstName = words[i + 1];
                        string lastName = words[i + 2];
                        int year = Int32.Parse(words[i + 6]);
                        int month = Int32.Parse(words[i + 7]);
                        int day = Int32.Parse(words[i + 8]);
                        q.Enqueue(new Person(firstName, lastName, new DateTime(year, month, day), gender));
                    }
                }
            }
            return q;
        }

        //It can read messages like:
        //Microsoft ( Information Technology, US )
        public static Queue<Company> CompanyParser(String fileN)
        {
            var q = new Queue<Company>();
            var file = new StreamReader(fileN);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split();
                string name = words[0];
                string activity = "";
                for (int i = 1; i < words.Length - 2; i++)
                {
                    activity += words[i] + " ";
                }
                string year = words[words.Length - 1];
                q.Enqueue(new Company(name, year, activity));
            }
            return q;
        }

        //It can read messages like:
        //FC Dynamo ( Kyiv, Ukraine, est. 1927 )
        public static Queue<FootballClub> FCParser(String fileN)
        {
            var q = new Queue<FootballClub>();
            var file = new StreamReader(fileN);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split();
                string name = words[1];
                string city = words[2];
                string country = words[3];
                int est = Int32.Parse(words[5]);
                q.Enqueue(new FootballClub(name, city, country, est));
            }
            return q;
        }

    }



    public class Program
    {
        static void Main(string[] args)
        {
            List<object> list = new List<object>();

        }
    }

}


