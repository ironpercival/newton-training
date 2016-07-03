<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Text.RegularExpressions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//hi :)
//I have got some problems with completing the full programm in time, but I hope I will fix it before you start checking our homeworks
//If you see this message - the programm isnt full, but you can see implementations of few ideas

// Anton: hi! I see you have a proper mindset and your ideas are good, but you lack a full understanding of interfaces and a proper implementation :) Check my version and see how it could be done. It's quite similar to the approach you've chosen! Check some my comments below.

	class Program {
		static void Main(string[] args) {
			List<Parsable> test = new List<Parsable>();
		}
	}

//Anton: IComparable - good!
//example
	class FootballTeam: IComparable<FootballTeam>, Parsable{
		public string Name {get; private set; }
		public DateTime LastGame{get; private set; }
		public static string regex = @"(w+)\shas\swon\sa\smatch\son\s(\d{4}/\d\d/\d\d)";
		public FootballTeam(string name, DateTime game){
			Name = name;
			LastGame = game;
		}
		
		public string GetRegex(){
			return regex;
		}
		public FootballTeam(string s){
			var match = Regex.Matches(s, regex);
			Name = (string)match[1].Value;
			LastGame = DateTime.Parse(match[2].Value);
			
		}
		
		public int CompareTo(FootballTeam ft){
			return (this.Name).CompareTo(ft.Name);
		}
		
		public override int GetHashCode() {
			return Tuple.Create(Name, LastGame).GetHashCode();
		}
		
		public override string ToString() {
			return $"{Name}, last game: {LastGame}";
		}
		
		public List<Parsable> Parse(string text){
			
			var matches = Regex.Matches(text, regex);
			
			List<Parsable> res = new List<Parsable>();
			for(int i = 0; i < matches.Count; i++){
				res.Add(new FootballTeam(matches[i].Value));
				
			}
			
		return res;
			
		}
		
		public List<FootballTeam> RemoveDuplicates(List<FootballTeam> l){
			List<FootballTeam> res = l.Distinct(new FTComparer()).ToList();
			return res;
		}
			
	}
	
	// Anton: good!
	//needed to remove duplicates
	class FTComparer : IEqualityComparer<FootballTeam> {

    public bool Equals(FootballTeam x, FootballTeam y) {
        return x.Name.Equals(y.Name);
    }

    public int GetHashCode(FootballTeam obj) {
        return obj.GetHashCode();
    }
	
}
//Anton: interface is used - good
//Anton: interfaces should be named with I letter first like IParsable
public interface Parsable{
	string GetRegex(); // Anton: I think you don't need this since you have a general method Parse(). Let the implementer decide how to do it, either with Regex or not etc. No need to force them to provide a regex.
	List<Parsable> Parse(string text);
}

	class Parser{
		public List<Parsable> Parse(Parsable[] delegates, string text){
			List<Parsable> res = new List<Parsable>();
			foreach(Parsable p in delegates){
				List<Parsable> temp = p.Parse(text);
				//res.Add(temp);
				
			}
			
			
		}
	
	}