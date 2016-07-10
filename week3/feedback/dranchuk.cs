<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Xml</Namespace>
</Query>

// Anton: Good! It would be nice if you provided some usage example though.

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {

//Anton: no usage example :(
        }
    }


    class FootballTeam : IComparable<FootballTeam>, IParsable
    {
        public string Name { get; private set; }
        public DateTime LastGame { get; private set; }
        public string Rival;
		
        //Anton: you should be careful with static fields! Static fields are shared accross all instances of a class, incl. inhereted classes!
		public static string regex = @"(w+)\shas\swon\sa\smatch\son\s(\d{4}/\d\d/\d\d)\swith(w+)";
        
		public FootballTeam(string name, DateTime game, string rival)
        {
            Name = name;
            LastGame = game;
            Rival = rival;
        }

        //get obj from xml
        public FootballTeam GetFromXML(string filename)
        {
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader r = XmlReader.Create(filename, settings))
            {
                r.MoveToContent(); 
                r.ReadStartElement("team");
                string TeamName = r.ReadElementContentAsString("team-name", "");
                DateTime LastG = DateTime.Parse(r.ReadElementContentAsString("last-game", ""));
                string Rival = r.ReadElementContentAsString("rival", "");
                r.MoveToContent(); 
                r.ReadEndElement();
                return new FootballTeam(TeamName, LastG, Rival);
            }

            
        }
        public string GetRegex()
        {
            return regex;
        }
        public FootballTeam(string s)
        {
            var match = Regex.Matches(s, regex);
            Name = (string)match[1].Value;
            LastGame = DateTime.Parse(match[2].Value);

        }

        public int CompareTo(FootballTeam ft)
        {
            return (this.Name).CompareTo(ft.Name);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Name, LastGame).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Name}, last game: {LastGame}";
        }

        public List<IParsable> Parse(string text)
        {

            var matches = Regex.Matches(text, regex);

            List<IParsable> res = new List<IParsable>();
            for (int i = 0; i < matches.Count; i++)
            {
                res.Add(new FootballTeam(matches[i].Value));

            }

            return res;

        }

        public List<FootballTeam> RemoveDuplicates(List<FootballTeam> l)
        {
            List<FootballTeam> res = l.Distinct(new FTComparer()).ToList();
            return res;
        }

        //add obj info to xml file
        public void SaveAsXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create("D:\FootballTeams\ft.xml", settings))
            {
                writer.WriteStartElement("team");
                writer.WriteStartElement("team-name", Name);

                writer.WriteElementString("last-game", LastGame.ToString());
                writer.WriteElementString("rival", Rival);
                writer.WriteEndElement();
            }
        }

    }
    
    //needed to remove duplicates
    class FTComparer : IEqualityComparer<FootballTeam>
    {

        public bool Equals(FootballTeam x, FootballTeam y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(FootballTeam obj)
        {
            return obj.GetHashCode();
        }

    }
	
	public interface IParsable
    {
        List<IParsable> Parse(string text);
        void SaveAsXML();
    }

    class Parser
    {
        public List<IParsable> Parse(IParsable[] delegates, string text)
        {
            List<IParsable> res = new List<IParsable>();
            foreach (IParsable p in delegates)
            {
                List<IParsable> temp = p.Parse(text);
                res.Concat(temp);

            }

            return res;


        }

    }
}
