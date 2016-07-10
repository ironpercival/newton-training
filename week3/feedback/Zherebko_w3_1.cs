<Query Kind="Expression">
  <Namespace>System</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

// Anton: good, but where are the classes from our Parser program? The task was to implement serialization of parsed entities :)

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {



            Serializer<int> sr = new Serializer<int>();
            System.Console.WriteLine("serialazing two numbers");
            sr.Serialize("./TestDir", "kke.xml", 0);
            sr.Serialize("./TestDir", "2.xml", 234);
            Serializer<string> s = new Serializer<string>();
            System.Console.WriteLine("serialazing string");

            s.Serialize("./TestDir", "2.xml", "TestDir");
            Serializer<Test> l = new Serializer<Test>();
            var testobj = new Test() { Finish = "kke", Start = 123, Date = new DateTime(1996, 1, 1) };
            System.Console.WriteLine("serialazing testobj" + Environment.NewLine + testobj.ToString());
            l.Serialize("./TestDir", "lol.xml", testobj);

            System.Console.WriteLine("Deserialize Test obj");
            var er = l.Deserialize("./TestDir", "lol.xml");
            System.Console.WriteLine(er);


            System.Console.WriteLine("Serialization with dynamic exeption expected");
            Serializer<dynamic> dynamic_serializer = new Serializer<dynamic>();
            string name = "test";
            int id = 1;
            dynamic_serializer.Serialize("./TestDir", "test.xml", new { name, id });


        }
    }

    class InvalidTypeExeption : Exception
    {
        public InvalidTypeExeption()
        {

        }

        public InvalidTypeExeption(string message) : base(message)
        {

        }
    }

    public class Test
    {
        public int Start { get; set; }
        public string Finish { get; set; }
        public DateTime Date { get; set; }
        public override string ToString()
        {
            return $"{Start} {Finish} {Date}";
        }

    }

    internal class Serializer<T>
    {
        private readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));
     


        public T Deserialize(string workingDirectory, string fileName)
        {
            var filePath = prepareDir(workingDirectory, fileName, typeof(T).ToString());
            using (XmlReader reader = XmlReader.Create(File.OpenRead(filePath)))
            {

                if (!_serializer.CanDeserialize(reader))
                {
                    throw new InvalidTypeExeption("you cant deserialize this data");
                }
                return (T)_serializer.Deserialize(reader);

            }
        }
        //create directory for file and return full filePath
        private static string prepareDir(string workingDirectory, string fileName, string typeName)
        {

            var separ = Path.DirectorySeparatorChar.ToString();

            string workingPath;
            if (typeName.StartsWith("<>"))
            {
                throw new InvalidTypeExeption("you cant serialize dynamics types ; your type should open contain parametrless constructor");

            }

            workingPath = workingDirectory + separ + typeName;
            var filePath = workingPath + separ + fileName;
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
            return filePath;
        }

        public void Serialize(string workingDirectory, string fileName, T data)
        {
            var filePath = prepareDir(workingDirectory, fileName, data.GetType().ToString());
            using (Stream writer = File.OpenWrite(filePath))
            {
                _serializer.Serialize(writer, data);
            }
        }
    }
}