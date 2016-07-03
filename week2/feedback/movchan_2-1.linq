<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text</Namespace>
</Query>


// Anton: 
// First, don't invent a wheen with MyRegex. There are regexes already and they work well for such tasks. And they are a common tool so it's wise to use them.
// Second, this Entity, how should it work? The idea here was to use some interface or abstract class as the basis for parsed entities of different classes (Person, FootballClub etc). And potentially add more classes in the future, and do it easily. Please check the suggested solutions to see what were two possible ways to implement this task (and other ways are possible too). 

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            String mask = "The company * has * rating.*";
            String str = "The company Sony has increasing rating ";
            String[] res = MyRegex.splitContent(str, mask);

            //String textToParse = "";
            //MyRegex[] regexes = new MyRegex[]{new MyRegex("The company * has * rating.", new Dictionary<int,string>{
            //    {1,""},
            //    {2, ""}
            //}, "Company")};
            //Program p = new Program();
            // Dictionary<String, Dictionary<String, Entity>> list = p.parse(textToParse, regexes);
            Console.ReadKey();
        }
        Dictionary<String, Dictionary<String, Entity>> parse(String text, MyRegex[] regexes)
        {
            Dictionary<String, Dictionary<String, Entity>> temp = new Dictionary<String, Dictionary<String, Entity>>();
            String[] array = text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (MyRegex r in regexes)
            {
                foreach (String str in array)
                {
                    Entity e = r.parse(str);
                    if (e != null)
                    {
                        if (temp.ContainsKey(e.nameOfEntity))
                        {
                            if (temp[e.nameOfEntity].ContainsKey(e.nameOfObject))
                            {
                                temp[e.nameOfEntity][e.nameOfObject].merge(e);
                            }
                        }
                        else
                        {
                            temp.Add(e.nameOfEntity, new Dictionary<String, Entity>());
                        }
                        temp[e.nameOfEntity].Add(e.nameOfObject, e);
                    }
                }
            }
            return temp;
        }
        public class IncompatibleEntitiesException : Exception
        { }
        public class IncompatibleEntitiesInfoException : Exception
        { }

        public class Entity
        {
            public String nameOfEntity { set; get; }
            public String nameOfObject { set; get; }
            public Dictionary<String, String> props { set; get; }
            public void merge(Entity e2)
            {
                if (!nameOfEntity.Equals(e2.nameOfEntity) || !nameOfObject.Equals(e2.nameOfObject))
                {
                    throw new IncompatibleEntitiesException();
                }
                foreach (String key in e2.props.Keys)
                {
                    if (e2.props.ContainsKey(key) && !props[key].Equals(e2.props[key]))
                        throw new IncompatibleEntitiesInfoException();
                    props.Add(key, e2.props[key]);
                }
            }
        }
        public class MyRegex
        {
            public String mask { get; set; }
            public Dictionary<Int32, String> namesDict { get; set; }
            public String type { get; set; }
            public MyRegex(String mask, Dictionary<Int32, String> names, String entityType)
            {
                this.mask = mask;
                namesDict = names;
                type = entityType;
            }
            public Entity parse(String s)
            {
                try
                {
                    String[] fragments = splitContent(s, mask);
                    Entity e = new Entity();
                    e.nameOfEntity = type;
                    for (int i = 0; i < fragments.Length; i++)
                    {
                        if (namesDict.ContainsKey(i))
                        {
                            String value = namesDict[i];
                            if (value == null)
                            {
                                e.nameOfObject = fragments[i];
                            }
                            else
                            {
                                e.props[value] = fragments[i];
                            }
                        }
                    }
                    return e;
                }
                catch (Exception e1)
                {
                    return null;
                }
            }

            public static String[] splitContent(String toSplit, String mask)
            {

                String[] maskArr = mask.Split('*');
                return MyRegex.recursiveSplit(toSplit, maskArr);
            }
            public static String[] recursiveSplit(String toSplit, String[] splitters)
            {
                try
                {
                    return recursiveSplitTool(toSplit, splitters, new String[splitters.Length - 1], 0);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            private static String[] recursiveSplitTool(String toSplit, String[] splitters, String[] splitted, int index)
            {

                if (index < splitters.Length)
                {
                    String[] newSplitting = toSplit.Split(new String[] { splitters[index] }, 2, StringSplitOptions.None);
                    if (index != 0)
                        splitted[index - 1] = newSplitting.Length == 2 ? newSplitting[0] : "";
                    return recursiveSplitTool(newSplitting[newSplitting.Length - 1], splitters, splitted, index + 1);
                }
                else
                {
                    return splitted;
                }
            }

        }
    }
}
