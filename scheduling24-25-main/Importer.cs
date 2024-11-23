using JobShopSchedulingProblemCP.Instance;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP
{
    internal class Importer
    {

        //Eigenschaften


        //Variablen

        //Job wird identifiziert durch eine ID; Job enthält Tasks.
        Dictionary<int, List<Tuple<int, int>>> jobs = new Dictionary<int, List<Tuple<int, int>>>();

        //Task wird identifiziert durch eine ID und seinen Job; Task enthält Laufzeit und Maschine 
        Dictionary<Tuple<int, int>, Tuple< int, int>> tasks = new Dictionary<Tuple<int, int>, Tuple<int, int>>();

        //Setup wird identifiziert durch vorheriger und nachfolgender Job; Setup enthält Laufzeit
        Dictionary<Tuple<int, int>, int> setups = new Dictionary<Tuple<int, int>, int>();

        //Maschine ist definiert durch Id
        List<int> machines = new List<int>();

        //Methoden

        //Import von Instanzdaten ohne Setup in das Dictionary
        public void ImportInstance(string filepath)
        {
            using (TextFieldParser csvParser = new TextFieldParser(filepath))
            {
                csvParser.CommentTokens = ["#"];
                csvParser.SetDelimiters([","]);
                csvParser.HasFieldsEnclosedInQuotes = true;

                //Kopfzeile überspringen
                csvParser.ReadLine();

                for (int id = 0; !csvParser.EndOfData; id++)
                {
                    int[] line = csvParser.ReadFields().Select(int.Parse).ToArray();

                    //Für jeden Job einen neuen Key erstellen
                    if (!jobs.ContainsKey(line[0]))
                    {
                        jobs[line[0]] = new List<Tuple<int, int>>();
                    }

                    //Für jeden Task einen Eintrag im Dictionary erstellen
                    tasks[Tuple.Create(jobs[line[0]].Count, line[0])] = Tuple.Create(line[2], line[1]);

                    //Task dem Job hinzufügen
                    jobs[line[0]].Add(Tuple.Create(line[2], line[1]));

                    //Für jede Maschine einen Eintrag im Dictionary erstellen
                    if (!machines.Contains(line[1]))
                    {
                        machines.Add(line[1]);
                    }
                }

                jobs.OrderBy(x => x.Key);
                tasks.OrderBy(x => x.Key.Item2).OrderBy(x => x.Key.Item1);
                machines.Sort();

            }
        }

        //Import von allen Setups in das Dictionary
        public void ImportSetup(string filepath)
        {
            using (TextFieldParser csvParser = new TextFieldParser(filepath))
            {
                csvParser.CommentTokens = ["#"];
                csvParser.SetDelimiters([","]);
                csvParser.HasFieldsEnclosedInQuotes = true;

                for (int j = 0; csvParser.LineNumber != -1; j++)
                {
                    int[] line = csvParser.ReadFields().Select(int.Parse).ToArray();

                    //Für jede Zeile ein Setup im Dictionary erstellen
                    for (int column = 0; column < line.Length; column++)
                    {
                        if (!setups.ContainsKey(Tuple.Create(j, column)))
                        {
                            setups[Tuple.Create(j, column)] = line[column];
                        }
                    }
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

            newInstance.Setups = this.setups;

            //Addiere alle Setups für einen großen Horizon
            foreach (var setup in setups)
            {
                newInstance.Horizon = newInstance.Horizon + setup.Value;
            }

            return newInstance;
        }
    }
}
