using Microsoft.VisualBasic.FileIO;
using Projektseminar.Instance;

namespace Projektseminar
{
    internal class Importer
    {

        //Eigenschaften


        //Variablen
        Dictionary<int, List<Tuple<int, int>>> jobs = new Dictionary<int, List<Tuple<int, int>>>(); //Job wird identifiziert durch eine ID; Job enthält Tasks.
        Dictionary<Tuple<int, int>, Tuple<int, int>> tasks = new Dictionary<Tuple<int, int>, Tuple<int, int>>(); //Task wird identifiziert durch eine ID und seinen Job; Task enthält Laufzeit und Maschine 
        Dictionary<Tuple<int, int>, int> setups = new Dictionary<Tuple<int, int>, int>(); //Setup wird identifiziert durch vorheriger und nachfolgender Job; Setup enthält Laufzeit
        List<int> machines = new List<int>();//Maschine ist definiert durch Id

        //Methoden

        //Import von Instanzdaten ohne Setup in das Dictionary
        public void ImportInstanceFromFile(string filepath)
        {
            using (TextFieldParser csvParser = new TextFieldParser(filepath))
            {
                Dictionary<int, int[]> allLines = new Dictionary<int, int[]>();

                csvParser.CommentTokens = ["#"]; //Zeilen mit Raute werden ausgefiltert.
                csvParser.SetDelimiters([","]); //Delimiter ist ","
                csvParser.HasFieldsEnclosedInQuotes = true;

                //Lade alle Zeilen in das Dictionary, dass aus Zeilenident und Integer-Array besteht.
                for (int lineId = 0; !csvParser.EndOfData; lineId++)
                {
                    int[] line = csvParser.ReadFields().Select(int.Parse).ToArray();
                    allLines.Add(lineId, line);
                }

                //Input ist z.B. 5,5. Zweite Wert bestimmt Anzahl der Maschinen.
                for (int machineCounter = 0; machineCounter < allLines[0][1]; machineCounter++)
                {
                    machines.Add(machineCounter);
                }

                //Iteriere durch die Anzahl an Jobs. Erster Wert, erste Zeile.
                for (int jobCounter = 0; jobCounter < allLines[0][0]; jobCounter++)
                {
                    jobs[jobCounter] = new List<Tuple<int, int>>(); //Erstelle neue Liste im Job-Array

                    //Für jeden Job importierte zugehörige Tasks. Laufvariable bis zur ersten Spalte von jeder Zeile 
                    //Spalten pro Zeile ist Anzahl der Tasks * 2 (plus erste Spalte) 
                    for (int taskCounter = 1; taskCounter < allLines[jobCounter + 1][0] * 2; taskCounter = taskCounter + 2)
                    {

                        //Immer zwei Spalten bilden ein Task-Tupel
                        tasks[Tuple.Create(jobs[jobCounter].Count, jobCounter)] = Tuple.Create(allLines[jobCounter + 1][taskCounter + 1], (allLines[jobCounter + 1][taskCounter] - 1));
                        jobs[jobCounter].Add(Tuple.Create(jobs[jobCounter].Count, jobCounter));
                    }

                    //Import der Setups. Es gibt so viele Zeilen und Spalten in den Setups wie Jobs.
                    for (int setupCounter = 0; setupCounter < allLines[0][0]; setupCounter++)
                    {
                        setups[Tuple.Create(jobCounter, setupCounter)] = allLines[jobCounter + 1 + allLines[0][0]][setupCounter];
                    }
                }
            }
        }

        //Generiere eine zufällige Instanz basierend auf den Übergabeparametern
        public void ImportRandomInstance(int jobAmount, int machineAmount, int minTaskPerJob, int minTaskTime, int maxTaskTime)
        {
            Random rand = new Random(); //Initieren neuen Random mit SeedValue

            //Iteriere durch Menge an zu erstellenden Jobs.
            for (int jobCounter = 0; jobCounter < jobAmount; jobCounter++)
            {
                jobs[jobCounter] = new List<Tuple<int, int>>();

                int taskCountPerJob = rand.Next(minTaskPerJob, machineAmount + 1); //Neuer Zufallsinteger zwischen minimaler Anzahl an Jobs und Menge an Maschinen (+ 1 weil Grenze exklusiv)
                int taskMachine;
                List<int> invalidMachine = new List<int>(); //Liste an bereits vergegebenen Maschinen, weil pro Job jede Maschine nur einmal

                //Erstelle Anzahl an Tasks basierend auf oberen Grenzen
                for (int taskCounter = 0; taskCounter < taskCountPerJob; taskCounter++)
                {
                    do
                    {
                        taskMachine = rand.Next(machineAmount); //Wähle zufällige Maschine zwischen 0 und Anzahl an Maschine (Exklusiv)
                    }
                    while (invalidMachine.Contains(taskMachine)); //Wähle neuen Wert wenn bereits vergeben.

                    invalidMachine.Add(taskMachine);

                    //Erstelle Task in Dictionary und mit Dauer zwischen minimal und maximal (Exklusiv)
                    tasks[Tuple.Create(jobs[jobCounter].Count, jobCounter)] = Tuple.Create(rand.Next(minTaskTime, maxTaskTime + 1), taskMachine);
                    jobs[jobCounter].Add(Tuple.Create(jobs[jobCounter].Count, jobCounter));
                }

                //Erstelle Setup mit Anzahl an Jobs x Jobs
                for (int setupCounter = 0; setupCounter < jobAmount; setupCounter++)
                {
                    setups.Add(Tuple.Create(jobCounter, setupCounter), rand.Next(minTaskTime, maxTaskTime + 1));
                }

            }

            //Erstelle Maschinen basierend auf Menge der Maschinen
            for (int machineCounter = 0; machineCounter < machineAmount; machineCounter++)
            {
                machines.Add(machineCounter);
            }
        }

        //Generiere ein Problem-Object aus dem Dictionary
        public Problem GenerateProblem()
        {
            Problem newInstance = new Problem();

            //Für jeden Eintrag im Dictionary ein Job-Objekt erstellen
            foreach (int id in jobs.Keys)
            {
                Job newJob = new Job(id);
                newInstance.Jobs.Add(newJob);
            }

            //Für jeden Eintrag im Dictionary ein Maschinen-Objekt erstellen
            foreach (int id in machines)
            {
                Instance.Machine newMachine = new Instance.Machine(id);
                newInstance.Machines.Add(newMachine);
            }

            //Für jeden Eintrag im Dictionary ein Task-Objekt erstellen
            foreach (Tuple<int, int> idTuple in tasks.Keys)
            {
                Instance.Task newTask = new Instance.Task(newInstance.Machines[tasks[idTuple].Item2], newInstance.Jobs[idTuple.Item2], tasks[idTuple].Item1, idTuple.Item1);
                newInstance.Jobs[idTuple.Item2].Tasks.Add(newTask);

                newInstance.Jobs[idTuple.Item2].TotalDuration = newInstance.Jobs[idTuple.Item2].TotalDuration + tasks[idTuple].Item1;

                newInstance.Horizon = newInstance.Horizon + tasks[idTuple].Item1;
            }

            newInstance.Setups = setups;

            //Addiere alle Setups für einen großen Horizon
            foreach (var setup in setups)
            {
                newInstance.Horizon = newInstance.Horizon + setup.Value;
            }

            return newInstance;
        }

    }
}