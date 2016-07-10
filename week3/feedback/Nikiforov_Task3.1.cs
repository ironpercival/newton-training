<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.IO</Namespace>
</Query>

// Anton: good!
public class Ser
{
	public static void Main()
    {
		List<Person> p = new List<Person>();
		p.Add(new Person("Kolya", "Lolik", "male", 23));
		p.Add(new Person("Nadya", "Jolik", "female", 13));
		Serialize<Person> s = new Serialize<Person>();
		s.ParseToXML(p);
    }
}


class Serialize<T>
{
	XmlSerializer serializer = new XmlSerializer(typeof(T));
	public List<T> ParseFromXML(List<string> fileRoots)
	{
		List<T> result = new List<T>();
		foreach(string fileRoot in fileRoots)
		{
			TextReader reader = new StreamReader(File.OpenRead(fileRoot));
            {
                try
                {
                    result.Add((T)serializer.Deserialize(reader));

                }
                catch
                {
                    Console.WriteLine("Wrong filepath");
                }
            }
		}   
		return result;
	}
	
	public  void ParseToXML(List<T> items)
	{
		
		for(int i = 0; i < items.Count; i++)
		{
			if (!Directory.Exists(items[i].GetType().ToString()))
				Directory.CreateDirectory(items[i].GetType().ToString());
			TextWriter writer = new StreamWriter(String.Format("{0}\\{1}.xml", items[i].GetType(), i));
			serializer.Serialize(writer, items[i]);
			writer.Close();
		}
	}
		
}

[XmlRoot( 
     ElementName = "Person", 
     DataType = "string", 
     IsNullable=true)]
public class Person
{
	public string firstName;
	public string secondName;
	[XmlAttribute("gender")]
	public string gender;
	public int age;
	public Person()
	{
	}
	public Person(string fn, string sn, string g, int a)
	{
		firstName = fn;
		secondName = sn;
		gender = g;
		age =a;
	}
	
}