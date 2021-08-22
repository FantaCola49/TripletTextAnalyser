using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TripletTextAnalyser
{
    class Program
    {
        private static readonly string FilePath = @$"{Environment.CurrentDirectory}\\ExampleTextRus.txt"; // !!! ЗДЕСЬ ИЗМЕНИТЬ ССЫЛКУ НА НУЖНЫЙ ФАЙЛ
        static IEnumerable<KeyValuePair<string, int>> MostFreqTrips;
        private static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MostFreqTrips = TripletParser(FilePath, 10);

            string DisplayStr = null;
            foreach (KeyValuePair<string, int> item in MostFreqTrips)
            {
                DisplayStr = $"{DisplayStr}{item.Key}, "; //итеративное дополнение строки вывода новыми ключами
            }
            Console.WriteLine($"Самые часто встречающиеся триплеты в тексте: \n{DisplayStr.Substring(0, DisplayStr.Length - 2)}");
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format($"{ts.Hours:00} часов, {ts.Minutes:00} минут, {ts.Seconds:00} секунд, { ts.Milliseconds:0000} миллисекунд");
            Console.WriteLine($"Время выполнения программы: {elapsedTime}");
        }

        static IEnumerable<KeyValuePair<string, int>> TripletParser(string FilePath, int TripletsNum)
        {
            ConcurrentDictionary<string, int> TripletsDict = new ConcurrentDictionary<string, int>();
            char[] SplitChars = new char[] { ' ' };

            try
            {
                Parallel.ForEach(File.ReadLines(FilePath), delegate (string TextLine, ParallelLoopState state, long lineNumber)
                {
                    TextLine = TextLine.ToLower().Replace(" ", "");

                    string[] Words = TextLine.Split(SplitChars);
                    foreach (string word in Words)
                    {
                        int TripletCounter;
                        bool _AreAllLetters = false; //отмечает, что все три члена последовательности являются буквами. Тогда последовательности типа 111 и ### не будут добавляться.
                        for (int i = 0; i < word.Length - 3; i++)
                        {
                            string TripletSubString = "";
                            if (char.IsLetter(word[i]) & char.IsLetter(word[i + 1]) & char.IsLetter(word[i + 2]))
                            {
                                _AreAllLetters = true;
                                if (word[i] == word[i + 1] & word[i] == word[i + 2])
                                {
                                    if (_AreAllLetters)
                                    {
                                        TripletSubString = $"{word[i]}{word[i + 1]}{word[i + 2]}";
                                        _AreAllLetters = false;
                                        TripletsDict.TryGetValue(TripletSubString, out TripletCounter);
                                        TripletCounter++;
                                        TripletsDict.AddOrUpdate(TripletSubString, TripletCounter, (KeyString, KeyValue) => TripletCounter); // по выданному ключу string обновляет значение словаря на TripletCounter
                                    }
                                }
                            }
                        }
                    }
                });

                return TripletsDict.ToList().OrderBy(x => -x.Value).Take(TripletsNum); // инверсировал значение отбора, чтобы триплеты шли от самого часто используемого до наименее частого (слева направо)
            }
            catch (OperationCanceledException)
            {
                return TripletsDict.ToList().OrderBy(x => -x.Value).Take(TripletsNum);
            }
        }
    }
}
