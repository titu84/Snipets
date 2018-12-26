 class Program
    {
        static void Main(string[] args)
        {
            var l = new List<string>()
            {
                "r", "a"
            };
            string result = l.Get(a => a == "r");
            Console.WriteLine(result);
            Console.Read();
        }
    }
    public static class Extensions
    {
        public static string Get(this IEnumerable<string> list, Func<string, bool> func)
        {
            return list.Where(a => func(a)).First();
        }
    }
