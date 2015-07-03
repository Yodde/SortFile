using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SortFile {
    class Program {
        static readonly object LOCK = new object();
        const string file = "file.bin";
        const string sortedFile = "sortedFile.bin";
        const int max = 100;
        static int index;
        static Random rand;
        static List<int> listOfNumbers;
        static List<int> generateInt;
        static void Main(string[] args) {
            rand = new Random();
            index = 0;
            generateInt = new List<int>();
            listOfNumbers = new List<int>();
            for(int i = 0; i < max; i++) {
                generateInt.Add(rand.Next(max * max));
            }
            WriteFile(file,generateInt); //generujemy plik z liczbami
            ReadFile_Task(file);

            listOfNumbers.Sort();

            WriteFile_Task(sortedFile, listOfNumbers);
            ReadFileAndWriteConsole_Task(sortedFile);
            Console.ReadKey();
        }
        public static void WriteFile_Task(string file, List<int> list) {
            index = 0;
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < list.Count(); ++i) {
                var temp = i;
                tasks.Add(Task.Run(() => {
                    while(true) {
                        if(temp == index) {
                            lock(LOCK) {
                                writeElemToFile(file, list[temp]);
                                ++index;
                            }
                            break;
                        }
                        else
                            Thread.Sleep(10);
                    }
                }));
            }  
            Task.WaitAll(tasks.ToArray());
        }
        public static void ReadFile_Task(string file) {
            List<Task> tasks = new List<Task>();
            if(!File.Exists(file))
                Console.WriteLine("Nie ma takiego pliku!");
            else {
                using(BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open))) {
                    for(int i = 0; i < max; ++i) {
                        tasks.Add(Task.Run(() => {
                            lock(LOCK)
                                listOfNumbers.Add(reader.ReadInt32());
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
            }
        }
        public static void ReadFileAndWriteConsole_Task(string file) {
            List<Task> tasks = new List<Task>();
            using(BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open))) {
                for(int i = 0; i < max; ++i) {
                    tasks.Add(Task.Run(() => {
                        lock(LOCK)
                            Console.WriteLine(reader.ReadInt32());
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            }
        }
        public static void ReadFileAndWriteConsole(string file) {
            using(BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open))) {
                for(int i = 0; i < max; ++i) {
                    Console.WriteLine(reader.ReadInt32());
                }
            }
        }
        public static void WriteFile(string file, List<int> list) {
            if(File.Exists(file)) {
                File.Delete(file);
            }
            foreach(var temp in list) {
                writeElemToFile(file, temp);

            }
        }
        public static void writeElemToFile(string file, int elem) {
            using(BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Append))) {
                writer.Write(elem);
                writer.Close();
            }
        }
    }
}

