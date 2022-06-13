using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace labaFive {
    [Serializable]
    public class Corrector {
        private Dictionary<string, List<string>> Dictionary { get; set; } = new Dictionary<string, List<string>>();

        private List<string> AddMisspelled(List<string> Misspelled) {
            string Word;
            Console.WriteLine("Add variations. Enter 0 to stop.");

            while (true) {
                Word = Console.ReadLine();

                if (Word == "0")
                {
                    break;
                }

                Misspelled.Add(Word);
            }
            return Misspelled;
        }

        private void AddWord() {
            Console.WriteLine("Enter a word: ");
            var NewWord = Console.ReadLine();

            if (Dictionary.ContainsKey(NewWord)) {
                throw new Exception("Word already added."); //словарик
            }

            var Misspelled = new List<string>();
            Dictionary.Add(NewWord, Misspelled);
            Dictionary[NewWord] = AddMisspelled(Dictionary[NewWord]);

            Console.WriteLine("New word added.");
        }

        private string Correction(string CurrentWord) { //перебор словаря
            foreach (var Word in Dictionary) {
                foreach (var version in Word.Value) {

                    if (CurrentWord == version) {
                        CurrentWord = Word.Key;
                        return CurrentWord;
                    }
                }
            }
            return CurrentWord;
        }

        private string EditWordsLine(string Line) {
            var Words = Line.Split();
            var EditedLine = "";
            foreach (var Word in Words) {
                EditedLine += $"{Correction(Word)} "; //исправление орфографических ошибок
            }
            EditedLine = $"{EditedLine}\n";
            return EditedLine;
        }

        private string EditNumbersLine(string Line) {
            string Number;
            var RegexFind = new Regex(@"([(]\d{3}[)]\s{1}\d{3})-(\d{2})-(\d{2})");
            var ReplaceDash = new Regex(@"-");
            var ReplaceBracket1 = new Regex(@"[(]");
            var ReplaceBracket2 = new Regex(@"[)]");

            var Matches = RegexFind.Matches(Line);
            if (Matches.Count > 0) {
                foreach (Match Match in Matches) {
                    Number = Match.Value;
                    Number = ReplaceDash.Replace(Number, " ");
                    Number = ReplaceBracket1.Replace(Number, "");
                    Number = ReplaceBracket2.Replace(Number, "");
                    Number = $"+380 {Number.Substring(1)}";

                    Line = Line.Replace(Match.Value, Number);
                }
            }
            var EditedLine = $"{Line}\n";
            return EditedLine;
        }

        private void EditWords(string FileName) {
            var EditedText = "";
            using (var TextFile = new StreamReader(FileName)) { //запись в словарик
                string OriginalText = "";
                while (!TextFile.EndOfStream) {
                    OriginalText = TextFile.ReadLine();
                    EditedText += EditWordsLine(OriginalText);
                }
            }
            using (var TextFile = new StreamWriter(FileName)) {
                TextFile.WriteLine(EditedText);
                TextFile.Flush();
            }
            Console.WriteLine("Editing finished.");
            Console.ReadKey();
        }

        private void EditNumbers(string FileName) {
            var EditedText = "";
            using (var TextFile = new StreamReader(FileName)) {
                string OriginalText = "";
                while (!TextFile.EndOfStream) {
                    OriginalText = TextFile.ReadLine();
                    EditedText += EditNumbersLine(OriginalText);
                }
            }
            using (var TextFile = new StreamWriter(FileName)) {
                TextFile.WriteLine(EditedText);
                TextFile.Flush();
            }
            Console.WriteLine("Editing finished.");
            Console.ReadKey();
        }

        private void Serialize(FileStream File) {
            var formatter = new BinaryFormatter();
            formatter.Serialize(File, this);
            File.Close();
        }

        private void Deserialize(FileStream File) {
            var formatter = new BinaryFormatter();
            var deserialized = (Corrector)formatter.Deserialize(File);
            Dictionary = deserialized.Dictionary;
            File.Close(); //процесс сохранения обьектов в бинарной форме
        }

        public void Program() {
            string Directory, Name;
            while (true) {
                Console.WriteLine("Enter directory:");
                Directory = Console.ReadLine();
                Console.WriteLine("Enter file name:");
                Name = Console.ReadLine();
                Name = Directory + "/" + Name;

                if (File.Exists(Name)) {
                    break;
                }
                else {
                    Console.WriteLine("\nFile not found. Try again.\n");
                }
            }

            var FileS = new FileStream($"{Directory}/dictionary.bin", FileMode.OpenOrCreate, FileAccess.Read);
            if (FileS.Length != 0) {
                Deserialize(FileS);
            }
            FileS.Close();

            while (true) {
                Console.Clear();
                Console.WriteLine("Add new word to dictionary     0");
                Console.WriteLine("Edit text                      1");
                Console.WriteLine("Edit number                    2");
                Console.WriteLine("Exit                           3");

                switch (Console.ReadLine()) {
                    case "0":
                        Console.Clear();
                        try {
                            AddWord();
                        }
                        catch (Exception Exception) {
                            Console.WriteLine(Exception.Message);
                            Console.ReadKey();
                            break;
                        }
                        FileS = new FileStream($"{Directory}/dictionary.bin", FileMode.Open, FileAccess.Write);
                        Serialize(FileS);
                        FileS.Close();
                        Console.ReadKey();
                        break;
                    case "1":
                        Console.Clear();
                        EditWords(Name);
                        break;
                    case "2":
                        Console.Clear();
                        EditNumbers(Name);
                        break;
                    case "3":
                        return;
                    default:
                        break;
                }
            }

        }
    }
}