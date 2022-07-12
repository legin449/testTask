using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace Program
{
    class Program
    {
        //dict for triples
        public static ConcurrentDictionary<string, int> triples = new ConcurrentDictionary<string, int>();
        static List<Thread> threads = new List<Thread>();

        static void Main()
        {
            Console.WriteLine("Введите путь до txt файла");
            string pathText = Console.ReadLine();
            if (!File.Exists(pathText))
            {
                throw (new Exception("Проверьте правильно ли указан путь до файла"));
            }
            //Init timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //read text from txt
            string TextTest = File.ReadAllText(pathText).ToLower();

            //replace all non-word chars to space
            TextTest = Regex.Replace(TextTest, "\\W{1,}", " ");

            //splitting text for multithreading
            var spaces = Regex.Matches(TextTest, " ");
            int spacesCount = spaces.Count();

            //spiltBy could be changed for performance
            int splitBy = 1;

            //getting number of splits
            int numOfProcesses = (spacesCount+splitBy) / splitBy;
            int begin;
            int end;

            //getting text boundaries and starting proccess each part
            for (int i = 0; i < spacesCount; i = i + numOfProcesses)
            {
                if(i + numOfProcesses < spacesCount)
                {
                    if(i==0)
                    {
                        begin = i;
                        end = spaces[i + numOfProcesses].Index;
                    }
                    else
                    {
                        begin = spaces[i].Index;
                        end = spaces[i + numOfProcesses].Index;
                    }
                }
                else
                {
                    if(i==0)
                    {
                        begin = 0;
                    }
                    else
                    {
                        begin = spaces[i].Index;
                    }
                    end = spaces[spacesCount - 1].Index;
                }
                if (!(begin == end))
                {
                    Thread thread = new Thread(() => FindTriplets(begin, end, TextTest));
                    thread.Start();
                    threads.Add(thread);
                }
            }
            Console.WriteLine($"threads count: {threads.Count}");

            //wait for all threads are closed
            while (threads.Count(x => x.IsAlive) > 0)
                Thread.Sleep(5);
            //sorting dictionary
            var triplesSorted = triples.OrderByDescending(key => key.Value).Take(10);
            //displaying first 10 triples
            foreach (var triple in triplesSorted)
            {
                Console.WriteLine($"{triple.Key} - found {triple.Value} times,");
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("\nRunTime " + elapsedTime + " ms");
        }

        static void FindTriplets(int begining, int end, string Text)
        {
            for (int i = begining; i < end; i++)
            {
                string triple;
                //if text is not ended
                if (i + 2 <= end)
                {
                    //getting triple
                    triple = Text[i].ToString() + Text[i + 1].ToString() + Text[i + 2].ToString();
                    //if triple already in dict - skipping it
                    if (triples.ContainsKey(triple))
                    {
                        continue;
                    }
                }
                else
                {
                    break;
                }
                //if space was faced in triple - skipping
                if (triple.Contains(" "))
                {
                    continue;
                }
                //getting all matches for triple in text
                var matches = Regex.Matches(Text, triple);
                //adding triple to dict
                if (matches.Count > 0)
                {
                    if (triples.ContainsKey(triple))
                    {
                        continue;
                    }
                    else
                    {
                        triples[triple] = matches.Count();
                    }
                }
            }
        }
    }
}