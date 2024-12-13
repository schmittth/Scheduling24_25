using Microsoft.VisualBasic.FileIO;
using Projektseminar.Instance;
using System.Data.Common;

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

                csvParser.CommentTokens = ["#"];
                csvParser.SetDelimiters([","]);
                csvParser.HasFieldsEnclosedInQuotes = true;

                for (int id = 0; !csvParser.EndOfData; id++)
                {
                    int[] line = csvParser.ReadFields().Select(int.Parse).ToArray();
                    allLines.Add(id, line);
                }

                for (int m = 0; m < allLines[0][1]; m++)
                {
                    machines.Add(m);
                }

                for (int i = 0; i < allLines[0][0]; i++)
                {
                    jobs[i] = new List<Tuple<int, int>>();

                    for (int j = 1; j < allLines[i + 1][0] * 2; j = j + 2)
                    {
                        tasks[Tuple.Create(jobs[i].Count, i)] = Tuple.Create(allLines[i + 1][j + 1], (allLines[i + 1][j] - 1));
                        jobs[i].Add(Tuple.Create(jobs[i].Count, i));
                    }

                    for (int k = 0; k < allLines[0][0]; k++)
                    {
                        setups[Tuple.Create(i, k)] = allLines[i + 1 + allLines[0][0]][k];
                    }
                }
            }
        }

        //Generiere eine zufällige Instanz der größe 5,3 
        public void ImportRandomInstance(int jobCount, int machineCount, int seedValue, int minTaskPerJob, int minTaskTime, int maxTaskTime)
        {
            Random rand = new Random(seedValue);

            for (int i = 0; i < jobCount; i++)
            {
                jobs[i] = new List<Tuple<int, int>>();

                int taskCount = rand.Next(minTaskPerJob, machineCount);
                int taskMachine;
                List<int> invalidMachine = new List<int>();

                for (int j = 0; j <= taskCount; j++)
                {
                    do
                    {
                        taskMachine = rand.Next(machineCount);
                    } 
                    while (invalidMachine.Contains(taskMachine));

                    invalidMachine.Add(taskMachine);

                    tasks[Tuple.Create(jobs[i].Count, i)] = Tuple.Create(rand.Next(minTaskTime, maxTaskTime), taskMachine);
                    jobs[i].Add(Tuple.Create(jobs[i].Count, i));
                }
            }

            for (int m = 0; m < machineCount; m++)
            {
                machines.Add(m);
            }

            for (int l = 0; l < jobCount; l++)
            {
                for (int k = 0; k < jobCount; k++)
                {
                    setups.Add(Tuple.Create(l, k), rand.Next(minTaskTime, maxTaskTime));
                }
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
