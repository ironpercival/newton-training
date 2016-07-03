<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

// Anton: sorry, I don't get your point at all. I see some proper ideas, but at the same time some decisions look strange to me. See some comments below. And check my version if you feel interested.

public class Reg
{
	public static void Main()
    {
        string pattern = @"([A-Z][a-z]+\W[A-Z][a-z]+)\W\W(\d+/\d+/\d+), ([male|female]+)";
        Regex regex = new Regex(pattern);
		Person pers = new Person();
		pers.Reg = regex;
		pers.NumbOfFildes =  3;
		List<Item> search = new List<Item>();
		search.Add(pers);
		List<Item> result = Parser.Pars("John Smith (1983/11/05, male) has been sentenced Kolya Savchenko (1933/12/01, female) to a 3 year imprisonment for John Smith (1983/11/05, male) corruption", search);
		foreach(Item i in result)
			Console.WriteLine(i);
		Console.WriteLine("_________________________________________");
		result = Parser.delDublicate(result);
		foreach(Item i in result)
			Console.WriteLine(i);
    }
}

class Parser
{
	
	public static List<Item> delDublicate(List<Item> items)
	{
		List<Item> result = new List<Item>();
		foreach(Item item in items)
		{
			bool flag = true;
			if(result.Count == 0)
				result.Add(item);
			foreach(Item r in result)
			{
				if(r.Equals(item))
					flag = false;
			}
			
			if(flag)
				result.Add(item);
			
		}
		return result;
	}
	
	
	public static List<Item> Pars(String text, List<Item> patterns){
		List<Item> result = new List<Item>();
		foreach (Item pattern in patterns)
		{
			if(!pattern.haveRegex())
				continue;
			
			foreach (Match match in pattern.Reg.Matches(text))
			{
			
				if(pattern.NumbOfFildes != match.Groups.Count-1)
					continue;
				
				String[] parameters = new String[pattern.NumbOfFildes];
				
				for(int i = 1; i < match.Groups.Count; i++)
				{
					parameters[i-1] = match.Groups[i].Value;
				}
				
				Item dublicate = pattern.GetNew().SetProporites(parameters);
				result.Add(dublicate);
			}
				
		}
		
		return result;
	}
	
	
}


class Person : Item
{
	
	private String name;
	private String dateOfBirth { get; set; }
	private String gender;
	
	// Anton: why do you think you will get properties in the right order? How do you know you get it right?
	public override Item SetProporites(String[] proporites)
	{
		name = proporites[0];
		dateOfBirth = proporites[1];
		gender = proporites[2];
		
		return this; // Anton: what?! Why return this?!
	}
	
	public override Item GetNew() // Anton: what is this?! You ask an instance to return a new instance of same class? Why?
	{
		return new Person();	
	}
	
	
	public override bool Equals(Object obj)
	{
		if(obj.GetType() != this.GetType())
			return false;
			
		Person p = (Person)obj;
		
		return (this.name.Equals(p.name)) && (this.dateOfBirth.Equals(p.dateOfBirth)) && (this.gender.Equals(p.gender));
		
	}
	
	public override String ToString()
	{
		return String.Format("{0} ({1}, {2})", name, dateOfBirth, gender);	
	}
}

abstract class Item
{
// Anton: this is bad. Static field is shared across ALL instances of a class. That means ALL instances of all derived classes will always use one and the same regex. Is this what you want?
	//regex to find new items
	protected static Regex reg;
	//number of text fildes in object
	
	// Anton: same problem as above - be careful with static fields!
	protected static int numbOfFildes = 0;
	
	//a method that will set all proporites for object
	public abstract Item SetProporites(String[] proporites);
	
	
	public Regex Reg
    {
        get
        {
            return reg;
        }
 
        set
        {
            reg = value;
        }
    }
	
	public int NumbOfFildes
    {
        get
        {
            return numbOfFildes;
        }
 
        set
        {
            numbOfFildes = value;
        }
    }
	
	public bool haveRegex()
	{
		return reg != null;	
	}
	
	//a method that must to return a objact of new type converted to Item
	public abstract Item GetNew();
	public override abstract String ToString();
	public override abstract bool Equals(Object obj);
	
}