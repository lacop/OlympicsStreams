using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace OlympicStreams
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 5*12+5;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("http://www.eurovisionsports.tv/london2012/epg/epg.csv");
            request.Proxy = null;
            string contents = new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd();
            const int offset = 2; // offset from GMT time

            List<Event> events = new List<Event>();
            foreach (var line in contents.Split(new[]{'\n'}).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(new[] {','}, 7);
                int m = Int32.Parse(parts[1].Substring(4, 2));
                int d = Int32.Parse(parts[1].Substring(6, 2));
                int hs = Int32.Parse(parts[4].Substring(0, 2));
                int ms = Int32.Parse(parts[4].Substring(3, 2));
                int he = Int32.Parse(parts[5].Substring(0, 2));
                int me = Int32.Parse(parts[5].Substring(3, 2));

                events.Add(new Event(new DateTime(2012, m, d, hs, ms, 0).AddHours(offset), new DateTime(2012, m, d, he, me, 0).AddHours(offset), parts[2], parts[3], parts[6]));
            }

            foreach (var channel in events.GroupBy(x => x.Channel).OrderBy(x => Int32.Parse(x.Key.Substring(5))))
            {
                Console.WriteLine(channel.Key);
                var sorted = channel.OrderBy(x => x.Start).ToArray();
                int i;
                for (i = 0; i < sorted.Length; i++)
                {
                    if (sorted[i].End >= DateTime.Now)
                    {
                        break;
                    }
                }
                
                if (i > 0)
                {
                    Console.WriteLine("\tprev\t{0}", sorted[i-1]);
                }
                else
                {
                    Console.WriteLine("\t----");
                }

                if (sorted[i].Start <= DateTime.Now)
                {
                    var orig = Console.BackgroundColor;
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("{0:00}:{1:00}\tNOW \t{2}", (sorted[i].End - DateTime.Now).Hours, (sorted[i].End - DateTime.Now).Minutes, sorted[i]);
                    Console.BackgroundColor = orig;
                }
                else
                {
                    Console.WriteLine("{0:00}:{1:00}\tNOW \t{2}", (sorted[i].Start - DateTime.Now).Hours, (sorted[i].End - DateTime.Now).Minutes, sorted[i]);
                }

                if (i < sorted.Length-1)
                {
                    Console.WriteLine("{0:00}:{1:00}\tNOW \t{2}", (sorted[i + 1].Start - DateTime.Now).Hours, (sorted[i + 1].End - DateTime.Now).Minutes, sorted[i + 1]);
                }

                Console.WriteLine();
            }

            Console.ReadLine();
        }

        private struct Event
        {
            public DateTime Start;
            public DateTime End;
            public string Channel;
            public string Discipline;
            public string Description;

            public Event(DateTime start, DateTime end, string channel, string discipline, string description)
            {
                Start = start;
                End = end;
                Channel = channel;
                Discipline = discipline;
                Description = description;
            }

            public override string ToString()
            {
                return string.Format("{0} - {1}: {2} - {3}", shortdate(Start), shortdate(End), Discipline.PadRight(25, ' '), Description);
            }

            private string shortdate (DateTime dt)
            {
                if (dt.Date > DateTime.Now.Date)
                {
                    return string.Format("T{0:00}:{1:00}", dt.Hour, dt.Minute);
                }
                if (dt.Date < DateTime.Now.Date)
                {
                    return string.Format("Y{0:00}:{1:00}", dt.Hour, dt.Minute);
                }
                return string.Format(" {0:00}:{1:00}", dt.Hour, dt.Minute);
            }
        }
    }
}
