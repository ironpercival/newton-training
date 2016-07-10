<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Xml.Linq</Namespace>
</Query>

// Anton: good overall, you seem to understand some basics about XmlWriter, but implementation is a bit awkward. You explicitly implement logic for every entity type. Check the sample implementation to get more ideas about how to implement it.

namespace CreateEntities
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, world!");

            
            const string clubReg = @"(FC\.|AS\.)\s(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\d{4})";
            const string personReg = @"(Mr\.|Mrs\.|Ms\.)\s(\w+)\s(\w+)\swas\sborn\son\s(\d{4}/\d{2}/\d{2})";
            const string companyReg = @"(\w+\s+)*?(\w+)\s+\(([^,]*)[^)]([^)]*)\)(\w+\s+)*?";


            var objectsParser = new Parser();

            Func<Match, Person> personParser = Person.CreatePerson;
            Func<Match, FootballClub> clubsParser = FootballClub.CreateClub;
            Func<Match, Company> companyParser = Company.CreateCompany;
            Dictionary<string, Func<Match,MyEntity>> delDictionary = new Dictionary<string, Func<Match, MyEntity>>
            {
                [personReg] = personParser,
                [clubReg] = clubsParser,
                [companyReg] = companyParser
            };

            string text = "Mr. John Smith was born on 2001/11/03. Mrs. Jessica Brown was born on 1999/12/31. Mr. John Smith was born on 2001/11/03. Last year winner FC. Dynamo from Kyiv Ukraine 1927 won against AS. Milan from Milan Italy 1899. Company Microsoft (Information Technology, US) was closed. Share price of Microsoft (Information Technology, US) dropped to $100. Company Google (IT, US) bought a new product."
            ;

            var objects = objectsParser.Parse(text, delDictionary);

            Console.WriteLine("Before:");
            objectsParser.ShowArr(objects);

            objects = objectsParser.RemoveD(objects);
            Console.WriteLine("\nAfter:");
            objectsParser.ShowArr(objects);

            //Закоментував метод, після того як записав, щоб продемонструвати, що зчитування також виконується
            //коли відкоментовані і запис і зчитування, видає помилку, що Рідер доступається до файлу, який зайнятий іншим (Врайтером)
            //objectsParser.SaveToXML(objects);

            MyEntity me = new MyEntity();
            
            Console.WriteLine("\n\nPersons from XML:");
            var personsList=new List<MyEntity>();
            personsList = Person.ReadPersonXml();

            me.Print(personsList);

            Console.WriteLine("\n\nClubs from XML:");
            var clubsList = new List<MyEntity>();
            clubsList = FootballClub.ReadClubXml();
            me.Print(clubsList);

            Console.WriteLine("\n\nCompanies from XML:");
            var companiesList = new List<MyEntity>();
            companiesList = Company.ReadCompanyXml();
            me.Print(companiesList);
            
            Console.ReadLine();
        }
    }

    class MyEntity 
    {
        public MyEntity() { }
        public void Print(List<MyEntity> list)
        {
            foreach (MyEntity instance in list)
            {
                Console.WriteLine("{0}", instance);
            }
        }   
        public int CompareTo(MyEntity other)
        {
            return this.CompareTo(other);             
        }
    }
    class Parser
    {
        public Parser() { }
        public List<MyEntity> Parse(string text, Dictionary<string, Func<Match, MyEntity>> delDictionary)
        {
            var objectsList = new List<MyEntity>();
            foreach (KeyValuePair<string, Func<Match, MyEntity>> entry in delDictionary)
            {
                var objectsMatches = Regex.Matches(text, entry.Key);
                for (int i = 0; i < objectsMatches.Count; i++)
                {
                    Match match = objectsMatches[i];
                    //виконую дві лишніх дії, але, видає помилку, що неможливо перетворити object в MyEntity
                    //коли пишу намагаюсь додати до ліста одразу delDictionary[entry.Key].DynamicInvoke(match)
                    var obj = delDictionary[entry.Key].DynamicInvoke(match);
                    var other = obj as MyEntity;
                    objectsList.Add(other);

                }
            }
            return objectsList;
        }
        public List<MyEntity> RemoveD(List<MyEntity> list)
        {
            var objectsList = new List<MyEntity>();
            objectsList.Add(list.First());
            foreach (MyEntity obj in list)
            {
                if (!IsIn(objectsList, obj))
                    objectsList.Add(obj);
            }
            return objectsList;
        }
        public bool IsIn(List<MyEntity> list, MyEntity obj)
        {
            foreach (MyEntity item in list)
            {
                if (obj.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        public void ShowArr(List<MyEntity> arr)
        {
            foreach (object i in arr)
            {
                Console.WriteLine("{0}", i);
            }
        }
        public void SaveToXML(List<MyEntity> list)
        {
            Stream personsStrm = new FileStream("Persons\\Persons.xml", FileMode.Create);
            Stream clubsStrm = new FileStream("Clubs\\Clubs.xml", FileMode.Create);
            Stream companiesStrm = new FileStream("Companies\\Companies.xml", FileMode.Create);

            XmlWriterSettings writeSettings = new XmlWriterSettings();
            writeSettings.Indent = true;
            writeSettings.OmitXmlDeclaration = true;

            XmlWriter personsWriter = XmlWriter.Create(personsStrm, writeSettings);
            XmlWriter clubsWriter = XmlWriter.Create(clubsStrm, writeSettings);
            XmlWriter companiesWriter = XmlWriter.Create(companiesStrm, writeSettings);

            WritePersonXml(personsWriter, list);
            WriteClubsXml(clubsWriter,list);
            WriteCompaniesXml(companiesWriter,list);

        }
        //в кожному методі ппроходжу весь ліст. Хотілось зробити проходження літса лише один раз, і відправляти 
        //в відповідну функцію один екземпляр сутності так мабуть було б краще. Але, постійно виникали помилки
        //оскільки потрібно вже не створити файл і записати, а дописувати до вже існуюючого. 
        //Пробував за допомогою перевірки чи існує файл,але не допомогло, помилка виникає, коли записую кореневий елемент
        //наприклад для Person  структура <Persons><Person>....</Person></Persons>, от коли дивлюсь, що файл вже існує
        //не можу оминути повторний запис <Persons></Persons> оскільки без цього, взагалі не записує
        //Можливо тут порібно виконувати РідСтартЕлемент замість врайт, щоб прочитати кореневий елемент,
        //а потім я вже зможу додавати нових  персонів
        //в середину <Persons></Persons> ?
		
		// Anton: потрібно просто мати інтерфейс і імплементувати його у всіх типах сутностей. Потім пройти один раз по колекції сутностей і кожну сутність серіалізувати у XML.
		
		// Anton: you pass the writer from outside and dispose it inside the method!
        public void WritePersonXml(XmlWriter w, List<MyEntity> list)
        {
            using (w)
            {
                w.WriteStartElement("Persons");
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Person)
                    {
                        var other = list[i] as Person;

                        XElement e = new XElement("Person",
                        new XAttribute("gender", other.Gender),
                        new XElement("FirstName", other.firstName),
                        new XElement("LastName", other.lastName),
                        new XElement("BirthDate", other.Date));
                        e.WriteTo(w);
                    }
                }
                w.WriteEndElement();
            }
        }
        public void WriteClubsXml(XmlWriter w, List<MyEntity> list)
        {
            using (w)
            {
                w.WriteStartElement("Clubs");
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is FootballClub)
                    {
                        var other = list[i] as FootballClub;
                        XElement e = new XElement("Club",
                            new XAttribute("sc",other.ShortCode),
                            new XElement("FCName", other.FCName),
                            new XElement("Country", other.Country),
                            new XElement("City", other.City),
                            new XElement("Year", other.Year));
                        e.WriteTo(w);
                    }
                }
                w.WriteEndElement();
            }
        }
        public void WriteCompaniesXml(XmlWriter w, List<MyEntity> list)
        {
            using (w)
            {
                w.WriteStartElement("Companies");
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Company)
                    {
                        var other = list[i] as Company;
                        XElement e = new XElement("Company",
                            new XAttribute("code", other.companyCode),
                            new XElement("Name", other.companyName),
                            new XElement("Industry", other.industryName)
                            );
                        e.WriteTo(w);
                    }
                }
                w.WriteEndElement();
            }
        }
    }


    

    class Name : IComparable<Name>
    {
        private string firstName;
        private string lastName;
        public Name(string fN, string lN)
        {
            firstName = fN;
            lastName = lN;
        }
        public Name()
        {
            firstName = "";
            lastName = "";
        }
        public string fName
        {
            get { return firstName; }
            set { firstName = value; }
        }
        public string lName
        {
            get { return lastName; }
            set { lastName = value; }
        }
        public int CompareTo(Name other)
        {
            var thisName = $"{lName}{fName}".ToLower();
            var otherName = $"{other.lName}{other.fName}".ToLower();
            return thisName.CompareTo(otherName);
        }
        public bool Equals(Name other)
        {
            return fName == other.fName && lName == other.lName;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Name))
                return false;
            var other = (Name)obj;
            return Equals(other);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(fName, lName).GetHashCode();
        }


    }
    class Person : MyEntity,IComparable<Person>
    {
        public Name fullName;
        private char myGender = 'm';
        private DateTime birthDate;
        private int myAge = 0;


        public Person(string firstName, string lastName, char gender, DateTime data)
        {
            fullName = new Name(firstName, lastName);
            birthDate = new DateTime(data.Year, data.Month, data.Day);
            myGender = gender;
            myAge = getAge(data);
        }

        public int getAge(DateTime data)
        {
            DateTime today = DateTime.Today;
            if (today.Month >= data.Month && today.Day >= data.Day)
                return today.Year - data.Year;
            return today.Year - data.Year - 1;
        }
        public char Gender
        {
            get { return myGender; }
            set { myGender = value; }
        }
        public string firstName
        {
            get { return fullName.fName; }
            set { fullName.fName = value; }
        }
        public string lastName
        {
            get { return fullName.lName; }
            set { fullName.lName = value; }
        }
        public DateTime Date
        {
            get { return birthDate; }
            set { birthDate = value; }
        }
        public int Age
        {
            get { return myAge; }
            set { myAge = value; }
        }
        protected virtual bool EqualPersons(Person i)
        {
            return firstName == i.firstName && lastName == i.lastName && Gender == i.Gender && Date == i.Date;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Person;

            return other != null && EqualPersons(other);
        }
        public int CompareTo(Person other)
        {
            if (!Gender.Equals(other.Gender))
                return Gender.CompareTo(other.Gender);

            if (!fullName.Equals(other.fullName))
                return fullName.CompareTo(other.fullName);

            if (Date != other.Date)
                return Date.CompareTo(other.Date);

            return 0;
        }
        public override int GetHashCode()
        {
            return Tuple.Create(fullName, Date, Gender).GetHashCode();
        }
        static public Person CreatePerson(Match match)
        {
            var gender = (match.Groups[1].Value == "Mr.") ? 'm' : 'f';
            var firstName = match.Groups[2].Value;
            var lastName = match.Groups[3].Value;
            var birthDate = DateTime.Parse(match.Groups[4].Value);

            return new Person(firstName, lastName, gender, birthDate);
        }
        static public List<MyEntity> ReadPersonXml()
        {
            List<MyEntity> tmp=new List<MyEntity>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader r = XmlReader.Create("Persons\\Persons.xml", settings))
            {
                r.ReadStartElement("Persons");
                while (r.Name == "Person")
                {
                    XElement x = (XElement)XNode.ReadFrom(r);
                    string gender = (string)x.Attribute("gender");
                    string FirstName = (string)x.Element("FirstName");
                    string LastName = (string)x.Element("LastName");
                    DateTime date = (DateTime)x.Element("BirthDate");

                    tmp.Add(new Person(FirstName, LastName, gender[0],date));
                }
                r.ReadEndElement();
            }
            return tmp;
        }
        
        public override string ToString()
        {
            return fullName.lName + ", " + fullName.fName + " ( " + Gender + ", " + Age + " ) ";
        }
    }

    class FootballClub : MyEntity, IComparable<FootballClub>
    {
        private string shortCode;
        private string name;
        private string country;
        private string city;
        private int year;

        public FootballClub(string shortCode, string name, string country, string city, int year)
        {
            ShortCode = shortCode;
            FCName = name;
            Country = country;
            City = city;
            Year = year;
        }
		
		// Anton: no need to write such properties; auto-properties do the same!
        public string ShortCode
        {
            get { return this.shortCode; }
            private set { this.shortCode = value; }
        }
        public string FCName
        {
            get { return this.name; }
            private set { this.name = value; }
        }
        public string Country
        {
            get { return this.country; }
            private set { this.country = value; }
        }
        public string City
        {
            get { return this.city; }
            private set { this.city = value; }
        }
        public int Year
        {
            get { return this.year; }
            private set { this.year = value; }
        }
        protected virtual bool EqualClubs(FootballClub i)
        {
            return FCName == i.FCName && City == i.City && Country == i.Country && Year == i.Year;
        }
        public override bool Equals(object obj)
        {
            var other = obj as FootballClub;

            return other != null && EqualClubs(other);
        }
        public int CompareTo(FootballClub other)
        {
            if (!Year.Equals(other.Year))
                return Year.CompareTo(other.Year);

            if (!FCName.Equals(other.FCName))
                return FCName.CompareTo(other.FCName);

            if (Country != other.Country)
                return Country.CompareTo(other.Country);
            if (City != other.City)
                return City.CompareTo(other.City);
            return 0;
        }
        public override int GetHashCode()
        {
            return Tuple.Create(FCName, Country, City, Year).GetHashCode();
        }
        static public FootballClub CreateClub(Match match)
        {
            var sc = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var country = match.Groups[5].Value;
            var city = match.Groups[4].Value;
            var year = Int32.Parse(match.Groups[6].Value);

            return new FootballClub(sc, name, country, city, year);
        }
        static public List<MyEntity> ReadClubXml()
        {
            List<MyEntity> tmp = new List<MyEntity>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader r = XmlReader.Create("Clubs\\Clubs.xml", settings))
            {
                r.ReadStartElement("Clubs");
                while (r.Name == "Club")
                {
                    XElement x = (XElement)XNode.ReadFrom(r);
                    string sc = (string)x.Attribute("sc");
                    string fcName = (string)x.Element("FCName");
                    string country = (string)x.Element("Country");
                    string city = (string)x.Element("City");
                    int year = (int)x.Element("Year");
                    
                    tmp.Add(new FootballClub(sc,fcName,country,city,year));
                }
                r.ReadEndElement();
            }
            return tmp;
        }
        public override string ToString()
        {
            return $"{this.ShortCode} {this.FCName} ( {this.City} , {this.Country}, {this.Year} )";
        }

    }


    class Company : MyEntity, IComparable<Company>
    {
        private string name;
        private string industry;
        private string code;

        public Company(string name, string industry, string code)
        {
            companyName = name;
            industryName = industry;
            companyCode = code;
        }
        public string companyName
        {
            get { return this.name; }
            private set { this.name = value; }
        }
        public string industryName
        {
            get { return this.industry; }
            private set { this.industry = value; }
        }
        public string companyCode
        {
            get { return this.code; }
            private set { this.code = value; }
        }

        protected virtual bool EqualCompanies(Company i)
        {
            return companyName == i.companyName && industryName == i.industryName && companyCode == i.companyCode;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Company;

            return other != null && EqualCompanies(other);
        }

        public int CompareTo(Company other)
        {
            if (!companyName.Equals(other.companyName))
                return companyName.CompareTo(other.companyName);
            return 0;
        }
        public override int GetHashCode()
        {
            return Tuple.Create(companyName, industryName, companyCode).GetHashCode();
        }
        static public Company CreateCompany(Match match)
        {
            var name = match.Groups[2].Value;
            var ind = match.Groups[3].Value;
            var code = match.Groups[4].Value;

            return new Company(name, ind, code);
        }
        static public List<MyEntity> ReadCompanyXml()
        {
            List<MyEntity> tmp = new List<MyEntity>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader r = XmlReader.Create("Companies\\Companies.xml", settings))
            {
                r.ReadStartElement("Companies");
                while (r.Name == "Company")
                {
                    XElement x = (XElement)XNode.ReadFrom(r);
                    string countryCode = (string)x.Attribute("code");
                    string comName = (string)x.Element("Name");
                    string ind = (string)x.Element("Industry");
                    
                    tmp.Add(new Company(comName, ind,countryCode));
                }
                r.ReadEndElement();
            }
            return tmp;
        }
        public override string ToString()
        {
            return $"{this.companyName} ( {this.industryName}, {this.companyCode} )";
        }

    }

}
