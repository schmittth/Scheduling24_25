using System.Reflection.PortableExecutable;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Projektseminar.Instance
{
    internal class Problem
    {
        // Eigenschaften
        public int Horizon { get => horizon; set => horizon = value; } //Horizon: Eine ausreichend hohe Zahl
        public int Makespan { get => makespan; set => makespan = value; } //Makespan: Die gesammte Laufzeit dieses Problems

        public List<Machine> Machines
        {
            get => machines;
        }

        public List<Job> Jobs
        {
            get => jobs;
        }
        public Dictionary<Tuple<int, int>, int> Setups
        {
            get => setups;
            set => setups = value;
        }

        //Variablen

        private List<Job> jobs;
        private List<Machine> machines;
        private Dictionary<Tuple<int, int>, int> setups;

        private int horizon;
        private int makespan;

        //Konstruktoren

        //Parameterloser Konstruktor
        public Problem()
        {
            jobs = new List<Job>();
            machines = new List<Machine>();
            setups = new Dictionary<Tuple<int, int>, int>();
        }

        //Klon-Konstruktor der zu kopierendes Problem enthält. Kopiertes Projekt ist this.
        public Problem(Problem existingProblem, int jobsLength = 0, int machinesLength = 0)
        {
            //Initialisiere diese Listen mit definierter Länge um Rechenleistung zu sparek
            jobs = new List<Job>(existingProblem.Jobs.Capacity);
            machines = new List<Machine>(existingProblem.Machines.Capacity);

            horizon = existingProblem.Horizon; //Kopiere den Horizon in ein neues Element
            makespan = existingProblem.Makespan; //Kopiere den Makespan in ein neues Problem

            //Kopiere alle Maschinen in Liste in das neue Projekt
            foreach (Machine machine in existingProblem.Machines)
            {
                Machine cloneMachine = new Machine(machine.Id, machine.Schedule.Capacity); //Erstelle Maschine mit vordefinierter Länge für Schedule
                cloneMachine.Load = machine.Load;

                machines.Add(cloneMachine);
            }

            //Kopiere alle Jobs in Listen im neuen Projekt
            foreach (Job job in existingProblem.Jobs)
            {
                Job cloneJob = new Job(job.Id,job.Tasks.Capacity); //Erstelle Job mit vordefinierter Länge für Tasks
                jobs.Add(cloneJob);

                //Kopiere alle Tasks in Liste in neuerstellten Job
                foreach (Task task in job.Tasks)
                {
                    Task cloneTask = new Task(machines[task.Machine.Id], cloneJob, task.Duration, task.Id);
                    cloneJob.Tasks.Add(cloneTask);

                    cloneTask.Start = task.Start;
                    cloneTask.End = task.End;

                    cloneTask.Setup = task.Setup;
                    cloneTask.Tail = task.Tail;

                    cloneTask.Position = task.Position;
                }
            }

            //Kopiere die Schedule des alten Problems ins neue Problem
            foreach (Machine machine in existingProblem.Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    machines[machine.Id].Schedule.Add(Jobs[task.Job.Id].Tasks[task.Id]);
                }
            }

            Setups = existingProblem.Setups; //Kopiere die Setups

            SetRelatedTasks(); //Führe einmal Methode aus um alle Objektreferenzen wieder herzustellen.
        }

        //Methoden

        //Problem als Diagramm in den angegebenen Pfad schreiben
        public void ProblemAsDiagramm(string filepath, bool openOnWrite, int seedValue, TimeSpan watchTime)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath)); //Erstelle den Ordner wenn noch nicht vorhanden.

            File.Copy(@"..\..\..\Diagramms\template.html", filepath, true); //Kopiere das Template-File in neues Diagramm

            //Füge dem Diagramm neue Zeilen hinzu
            using (StreamWriter sw = File.AppendText(filepath))
            {
                Dictionary<int, string> jobColors = new Dictionary<int, string>(); //Initialsiere ein Dictionary um Jobs zu Farben zu mappen
                var random = new Random();
                string coloration = "";

                int setupCount = 0; //Instanziiere Integer um Setups zu zählen

                //Erstelle für jeden eingeplanten Task eine neue Zeile
                foreach (Machine machine in Machines)
                {
                    foreach (Task task in machine.Schedule)
                    {
                        //Wenn Job des aktuellen Tasks noch keine Farbe hat, erstelle neue.
                        if (!jobColors.ContainsKey(task.Job.Id))
                        {
                            jobColors.Add(task.Job.Id, String.Format("#{0:X6}", random.Next(0x1000000)));
                        }

                        //Wenn Task ein Setup hat füge Zeile für Setup hinzu.
                        if (task.Setup != 0)
                        {
                            //Schreibe Zeile für Setup. Setze Farbe auf Schwarz
                            sw.WriteLine($"[ 'Machine {task.Machine.Id}' , 'Setup{setupCount}',  'Duration: {task.Setup} Start: {task.Start - task.Setup} End: {task.Start}' , new Date(0, 0, 0, 0, 0, {task.Start - task.Setup}) , new Date(0, 0, 0, 0, 0, {task.Start})],");
                            coloration = coloration + "'#000000',";
                            setupCount++;
                        }

                        //Schreibe Zeile für Task. Setze zufällige Farbe aus Array
                        sw.WriteLine($"[ 'Machine {task.Machine.Id}' , '{task.Job.Id}_{task.Id}', 'Duration: {task.Duration} Start: {task.Start} End: {task.End}' , new Date(0, 0, 0, 0, 0, {task.Start}) , new Date(0, 0, 0, 0, 0, {task.End})],");
                        coloration = coloration + $"'{jobColors[task.Job.Id]}',";

                    }
                }
                coloration.Remove(coloration.Length - 1); //Lösche letztes Komma 

                //Schreibe weitere notwendige Zeilen
                sw.WriteLine("]);");
                sw.WriteLine($$"""var options = {colors: [{{coloration}}]};""");
                sw.WriteLine("chart.draw(dataTable, options);");
                sw.WriteLine("}");
                sw.WriteLine("</script>");
                sw.WriteLine($"<div><p>Makespan: {makespan} Seconds. Seed Value: {seedValue}. Processing Time: {watchTime}</p></div>");
                sw.WriteLine("<div id=\"example3.1\" style=\"height: 1000px;\"></div>");
            }

            //Öffne Diagramm direkt nach Schreiben des Diagramms
            if (openOnWrite == true) //Öffne wenn Parameter ist wahr
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process(); //Starte neuen Process
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filepath)); //Definiere Dateinamen zum öffnen
                    process.Start(); //Starte Prozess
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        //Problem als Txt-Datei in den angegebenen Pfad exportieren
        public void ProblemAsFile(string filepath)
        {
            //Erstelle neues Text-File in Dateipfad
            using (StreamWriter instanceWriter = File.CreateText(filepath))
            {
                //Schreibe Meta-Informationen
                instanceWriter.WriteLine("#Meta infos");
                instanceWriter.WriteLine($"{Jobs.Count},{Machines.Count}");
                instanceWriter.WriteLine("#Processing times");

                //Schreibe Zeile für jeden Job
                foreach (Job job in jobs)
                {
                    List<int> jobLine = new List<int>();
                    jobLine.Add(job.Tasks.Count); //Erster Wert enthält Anzahl der Operationen

                    //Schreibe einen Wert für jeden Task im Job
                    foreach (Task task in job.Tasks)
                    {
                        jobLine.Add(task.Machine.Id + 1);
                        jobLine.Add(task.Duration);
                    }
                    instanceWriter.WriteLine(String.Join(",", jobLine)); //Konkateniere alle Strings mit ,
                }

                //Schreibe Zeile für jeden Job
                instanceWriter.WriteLine("#Setup times");
                for (int rowJob = 0; rowJob < jobs.Count; rowJob++)
                {
                    List<int> setupLine = new List<int>();
                    //Schreibe Wert für jeden Job
                    for (int colJob = 0; colJob < jobs.Count; colJob++)
                    {
                        setupLine.Add(setups[Tuple.Create(rowJob, colJob)]);
                    }
                    instanceWriter.WriteLine(String.Join(",", setupLine)); //Konkateniere alle Strings mit ,
                }
            }
        }

        //Kalkuliere den Makespan und Update den Load
        public int CalculateMakespan()
        {
            int makespan = 0;

            //Iteriere durch alle Maschinen
            foreach (Machine machine in machines)
            {
                Task lastTask = machine.Schedule[machine.Schedule.Count - 1]; //Speichere den letzten Task auf der Maschine
                machine.Load = lastTask.End;

                if (lastTask.End > makespan)
                {
                    makespan = lastTask.End;
                }
            }
            return makespan;
        }

        //Tausche zwei Tasks auf einer Maschine
        public void SwapTasks(Task task1, Task task2)
        {
            int index1 = Jobs[task1.Job.Id].Tasks[task1.Id].Position; //Position des ersten Tasks auf seiner Maschine
            int index2 = Jobs[task2.Job.Id].Tasks[task2.Id].Position; //Position des zweiten Tasks auf seiner Maschine
            int machineId = task1.Machine.Id;

            Task tempTask = machines[machineId].Schedule[index1]; //Temporärer Task speichert Task der überschrieben wird
            machines[machineId].Schedule[index1] = machines[machineId].Schedule[index2]; //Überschreibe ersten Task mit zweitem Task im Plan der Maschine
            machines[machineId].Schedule[index2] = tempTask; //Überschreibe zweiten Task mit erstem Task im Plan der Maschine

            SetRelatedTasks(); //Kalkuliere Vorgänger und Nachfolder neu
            Recalculate(); //Kalkuliere Releases und Tails neu

        }

        //Gebe eine Liste von allen kritischen Tasks in einem Problem zurück.
        public Dictionary<Machine, List<Task>> GetCriticalTasks()
        {
            Dictionary<Machine, List<Task>> critTasks = new Dictionary<Machine, List<Task>>(); //Initialisiere Dictionary

            //Iteriere durch alle Maschinen 
            for (int machineId = 0; machineId < machines.Count(); machineId++)
            {
                critTasks.Add(machines[machineId], new List<Task>()); //Füge diese Maschine als Schlüssel hinzu

                //Iteriere durch alle Tasks auf der Maschine
                for (int taskOnMachine = 0; taskOnMachine < machines[machineId].Schedule.Count; taskOnMachine++)
                {
                    Task task = machines[machineId].Schedule[taskOnMachine];

                    //Wenn Release und Tail addiert Makespan ergeben ist der Task kritisch
                    if (task.Start + task.Tail == makespan)
                    {
                        critTasks[machines[machineId]].Add(task); //Füge Task dem Dict mit der Maschine als Schlüssel hinzu
                    }
                }
            }
            return critTasks; //Dieses Dictionary ist für jede Maschine sortiert. d.h. Maschine1[0] kommt vor Maschine1[1] usw.
        }

        //Setze alle Vorgänger und Nachfolger in einem Problem neu
        public void SetRelatedTasks()
        {
            foreach (Job job in jobs)
            {
                int taskJobCount = job.Tasks.Count;

                //Iteriere durch alle Tasks im Job
                foreach (Task task in job.Tasks)
                {
                    //Erster Task im Job hat keinen Vorgänger im Job
                    if (task.Id - 1 >= 0)
                    {
                        task.preJobTask = job.Tasks[task.Id - 1];
                    }
                    else
                    {
                        task.preJobTask = null;
                    }

                    //Letzer Task im Job hat keinen Nachfolger
                    if (task.Id + 1 < taskJobCount)
                    {
                        task.sucJobTask = job.Tasks[task.Id + 1];
                    }
                    else
                    {
                        task.sucJobTask = null;
                    }
                }
            }

            foreach (Machine machine in machines)
            {
                int taskMachineCount = machine.Schedule.Count;

                //Iteriere durch alle Tasks im Job
                for (int taskCounter = 0; taskCounter < taskMachineCount; taskCounter++)
                {
                    Task task = machine.Schedule[taskCounter];

                    //Erster Task auf der Maschine hat keinen Vorgänger
                    if (taskCounter - 1 >= 0)
                    {
                        task.preMachineTask = machine.Schedule[taskCounter - 1];
                    }
                    else
                    {
                        task.preMachineTask = null;
                    }

                    //Letzter Task auf der Maschine hat keinen Nachfolger
                    if (taskCounter + 1 < taskMachineCount)
                    {
                        task.sucMachineTask = machine.Schedule[taskCounter + 1];
                    }
                    else
                    {
                        task.sucMachineTask = null;
                    }
                    task.Position = taskCounter;
                }
            }          
        }

        public void Recalculate()
        {
            Queue<Task> releaseQueue = new Queue<Task>();
            Queue<Task> tailQueue = new Queue<Task>();

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.preMachineTask is not null)
                    {
                        task.Setup = Setups[Tuple.Create(task.preMachineTask.Job.Id, task.Job.Id)];
                    }
                    else
                    {
                        task.Setup = 0;
                    }

                    if (task.preMachineTask == null && task.preJobTask == null)
                    {
                        releaseQueue.Enqueue(task);
                    }
                    task.Start = -1;

                    if (task.sucMachineTask == null && task.sucJobTask == null)
                    {
                        tailQueue.Enqueue(task);
                    }
                    task.Tail = -1;
                }
            }

            while (releaseQueue.Count != 0)
            {
                //Entferne ersten Task aus der Queue
                releaseQueue.TryDequeue(out Task currentTask);

                int releasePM = 0, releasePJ = 0;

                if (currentTask.preMachineTask is not null)
                {
                    releasePM = currentTask.preMachineTask.Start + currentTask.preMachineTask.Duration + currentTask.Setup;
                }

                if (currentTask.preJobTask is not null)
                {
                    releasePJ = currentTask.preJobTask.Start + currentTask.preJobTask.Duration;
                }

                currentTask.Start = Math.Max(releasePM, releasePJ);
                currentTask.End = currentTask.Start + currentTask.Duration;
                currentTask.Machine.Load = currentTask.End;

                if (currentTask.sucJobTask is not null && (currentTask.sucJobTask.preMachineTask is null || currentTask.sucJobTask.preMachineTask.Start != -1))
                {
                    //if (currentTask.sucJobTask.preMachineTask is null || currentTask.sucJobTask.preMachineTask.Start != -1)
                    //{
                        releaseQueue.Enqueue(currentTask.sucJobTask);
                    //}
                }

                if (currentTask.sucMachineTask is not null && (currentTask.sucMachineTask.preJobTask == null || currentTask.sucMachineTask.preJobTask.Start != -1))
                {
                    //if (currentTask.sucMachineTask.preJobTask == null || currentTask.sucMachineTask.preJobTask.Start != -1)
                    //{
                        releaseQueue.Enqueue(currentTask.sucMachineTask);
                    //} 
                }
            }

            while (tailQueue.Count != 0)
            {
                //Entferne ersten Task aus der Queue
                tailQueue.TryDequeue(out Task currentTask);

                //Setze die Tailzeiten initial
                int tailSM = currentTask.Duration, tailSJ = currentTask.Duration;

                //Identifiziere Tail des nachfolgenden Tasks auf dieser Maschine
                if (currentTask.sucMachineTask is not null)
                {
                    tailSM = currentTask.sucMachineTask.Tail + currentTask.Duration + currentTask.sucMachineTask.Setup;
                }

                //Identifiziere Tail des nachfolgenden Tasks im Job dieses Tasks
                if (currentTask.sucJobTask is not null)
                {
                    tailSJ = currentTask.sucJobTask.Tail + currentTask.Duration;
                }

                currentTask.Tail = Math.Max(tailSM, tailSJ); //Update Tail mit dem Job oder Maschinen Maximum

                //Füge der Liste den Vorgänger im Job dieses Tasks hinzu
                if (currentTask.preJobTask is not null && (currentTask.preJobTask.sucMachineTask == null || currentTask.preJobTask.sucMachineTask.Tail != -1))
                {
                    //if (currentTask.preJobTask.sucMachineTask == null || currentTask.preJobTask.sucMachineTask.Tail != -1)
                    //{
                        tailQueue.Enqueue(currentTask.preJobTask);
                    //}
                }

                //Füge der Liste den Vorgänger auf der Maschine dieses Tasks hinzu
                if (currentTask.preMachineTask is not null && (currentTask.preMachineTask.sucJobTask == null || currentTask.preMachineTask.sucJobTask.Tail != -1))
                {
                    //if (currentTask.preMachineTask.sucJobTask == null || currentTask.preMachineTask.sucJobTask.Tail != -1)
                    //{
                        tailQueue.Enqueue(currentTask.preMachineTask);
                    //}
                }
            }
            makespan = CalculateMakespan(); //Setze den Makespan
        }

        public bool CheckCyclicity()
        {
            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Tail == -1 || task.Start == -1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Switch-Case Anweisung zur Auswahl der Nachbarschaft
        public Dictionary<int, List<Tuple<Task, Task>>> GetNeighboorhood(string searchMethod)
        {
            Dictionary<int, List<Tuple<Task, Task>>> newDict = new Dictionary<int, List<Tuple<Task, Task>>>();

            switch (searchMethod)
            {
                case "N1":
                    newDict = N1();
                    break;
                case "N3":
                    newDict = N3();
                    break;
                case "N5":
                    newDict = N5();
                    break;
                default:
                    break;
            }
            return newDict;
        }

        public Dictionary<int, List<Tuple<Task, Task>>> N1()
        {
            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task>>>();

            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                //foreach (Task task in critPair.Value)
                for (int taskCounter = 0; taskCounter < critPair.Value.Count; taskCounter++)
                {
                    Task task = critPair.Value[taskCounter];
                    if (task.sucMachineTask is not null && critTasks[task.Machine][taskCounter + 1] == task.sucMachineTask)
                    {
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task, task.sucMachineTask) });
                    }
                }
            }
            return swapOperations;
        }

        public Dictionary<int, List<Tuple<Task, Task>>> N3()
        {

            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task>>>();

            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                int tasksOnMachineCount = critPair.Value.Count;

                //foreach (Task task in critPair.Value)
                for (int taskCounter = 0; taskCounter < tasksOnMachineCount; taskCounter++) 
                {
                    Task task = critPair.Value[taskCounter];
                    bool firstNeighbor = false;

                    //if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasks[task.Machine].Contains(task.sucMachineTask))
                    if (task.preMachineTask is not null && task.sucMachineTask is not null)
                    {
                        //Der Maschinennachfolger muss kritisch und damit der nächste Task auf dieser Maschine sein
                        if (taskCounter + 1 < tasksOnMachineCount && critTasks[task.Machine][taskCounter + 1] == task.sucMachineTask)
                        {
                            //p(i), i, j --> task = i

                            //p(i), j, i
                            swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task, task.sucMachineTask) });

                            //j, p(i), i
                            swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task.preMachineTask, task), Tuple.Create(task, task.sucMachineTask) });

                            //j, i, p(i) --> Wenn diese Nachbarschaft abgespeichert ist, muss s(j), j, i nicht gespeichert werden.
                            firstNeighbor = swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task.preMachineTask, task.sucMachineTask) });
                        }

                        //Der Maschinenvorgänger muss kritisch und damit der vorherige Task auf dieser Maschine sein
                        if (taskCounter - 1 >= tasksOnMachineCount && critTasks[task.Machine][taskCounter - 1] == task.preMachineTask)
                        {
                            //i, j, s(j) --> task = j 

                            //j, i, s(j)
                            swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task.preMachineTask, task) });

                            //j, s(j), i
                            swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task, task.sucMachineTask), Tuple.Create(task.preMachineTask, task) });

                            //s(j), j, i
                            if (!firstNeighbor)
                            {
                                swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(task.preMachineTask, task.sucMachineTask) });
                            }
                        }
                    }                  
                }
            }
            return swapOperations;
        }

        public Dictionary<int, List<Tuple<Task, Task>>> N5()
        {
            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task>>>();
            Dictionary<Tuple<Machine, int>, List<Task>> critBlocks = new Dictionary<Tuple<Machine, int>, List<Task>>(); //Der Schlüssel des Dictionaries enthält die Maschine und einen Zähler wie viele Blöcke es auf der Maschine gibt

            //Wiederhole für die kritischen Tasks auf jeder Maschine
            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                int currentBlock = 0;

                if (critPair.Value.Count > 1)
                {
                    //Iteriere durch alle kritischen Tasks
                    for (int taskCounter = 0; taskCounter < critPair.Value.Count - 1; taskCounter++)
                    {
                        var blockKey = Tuple.Create(critPair.Key, currentBlock); //Definiere Key für aktuellen Block
                        
                        //Wenn noch kein Block mit diesem Key existiert und der kritische Task einen Nachfolger hat, erstelle neuen Block
                        if ((!critBlocks.ContainsKey(blockKey) && critPair.Value[taskCounter].sucMachineTask is not null))
                        {
                            critBlocks.Add(blockKey, new List<Task> { critPair.Value[taskCounter] }); //Füge aktuellen Task zum Block hinzu
                        }

                        //Wenn der nächste kritische Task der Maschinennachfolger des aktuellen Tasks ist füge diesen zum Block hinzu
                        if (critPair.Value[taskCounter + 1] == critPair.Value[taskCounter].sucMachineTask)
                        {
                            critBlocks[blockKey].Add(critPair.Value[taskCounter + 1]); //Füge nächsten Task zum Block hinzu
                        }

                        //Wenn der nächste Task nicht der Maschinennachfolger ist und der aktuellen Block nur einen Task lang ist, lösche den Block
                        else if (critBlocks[blockKey].Count == 1)
                        {
                            critBlocks.Remove(blockKey); //Lösche aktuellen Block
                        }

                        //Wenn der aktuelle Block länger als einen Task ist, beginne neuen Block
                        else
                        {
                            currentBlock++;
                        }
                    }
                }
            }

            //Iteriere durch jeden Block an Tasks
            foreach (KeyValuePair<Tuple<Machine, int>, List<Task>> blockPair in critBlocks)
            {
                int lastTaskIndex = blockPair.Value.Count - 1; //Einmal definieren um Laufzeit zu sparen
                int operationsCount = swapOperations.Count;

                //Für den ersten Block werden die letzten zwei Tasks getauscht
                if (blockPair.Value[0].Start == 0)
                {
                    swapOperations.TryAdd(operationsCount, new List<Tuple<Task, Task>> { Tuple.Create(blockPair.Value[lastTaskIndex], blockPair.Value[lastTaskIndex].preMachineTask) }); //Tausche den letzten und vorletzten Task im Block
                }

                //Für den letzten Block werden die ersten zwei Tasks getauscht
                else if (blockPair.Value[lastTaskIndex].Start + blockPair.Value[lastTaskIndex].Duration == makespan) //Wenn Release und Dauer addiert den Makespan ergeben ist der letzte Task im Block der absolut letzte.
                {
                    swapOperations.TryAdd(operationsCount, new List<Tuple<Task, Task>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask) }); //Tausche der ersten und zweiten Task im Block
                }

                //In allen anderen Fällen
                else
                {
                    //Wenn der Block nur eine Länge von 2 hat, reicht ein Tausch
                    if ((lastTaskIndex + 1) > 2)
                    {
                        swapOperations.TryAdd(operationsCount, new List<Tuple<Task, Task>> { Tuple.Create(blockPair.Value[lastTaskIndex], blockPair.Value[lastTaskIndex].preMachineTask) }); //Tausche den letzten und vorletzten Task im Block
                    }
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask) }); //Tausche der ersten und zweiten Task im Block. Hier muss gezählt werden, falls if eingetreten ist hat sich count geändert
                }
            }
            return swapOperations;
        }
    }
}