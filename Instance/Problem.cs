using Google.OrTools.LinearSolver;
using Google.OrTools.ModelBuilder;

namespace Projektseminar.Instance
{
    internal class Problem
    {
        // Eigenschaften
        public int Id { get; }
        public int Horizon { get => horizon; set => horizon = value; }
        public int Makespan { get; set; }

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

        private List<Job> jobs = new List<Job>();
        private List<Machine> machines = new List<Machine>();
        private Dictionary<Tuple<int, int>, int> setups = new Dictionary<Tuple<int, int>, int>();

        private int horizon = 0;

        //Konstruktoren

        //Parameterloser Konstruktor
        public Problem()
        {

        }

        //Klon-Konstruktor der zu kopierendes Problem enthält. Kopiertes Projekt ist this.
        public Problem(Problem existingProblem)
        {
            Horizon = existingProblem.Horizon; //Kopiere den Horizon in ein neues Element

            //Kopiere alle Maschinen in Liste in das neue Projekt
            foreach (Machine machine in existingProblem.Machines)
            {
                Machine cloneMachine = new Machine(machine.Id);
                cloneMachine.Load = machine.Load;

                machines.Add(cloneMachine);
            }

            //Kopiere alle Jobs in Listen im neuen Projekt
            foreach (Job job in existingProblem.Jobs)
            {
                Job cloneJob = new Job(job.Id);
                jobs.Add(cloneJob);

                //Kopiere alle Tasks in Liste in neuerstellten Job
                foreach (Task task in job.Tasks)
                {
                    Task cloneTask = new Task(machines[task.Machine.Id], cloneJob, task.Duration, task.Id);
                    cloneJob.Tasks.Add(cloneTask);

                    cloneTask.Start = task.Start;
                    cloneTask.End = task.End;

                    cloneTask.Release = task.Release;
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

                int makespan = CalculateMakespan(); //Instanziiere Integer mit Makespan
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
                            sw.WriteLine($"[ 'Machine {task.Machine.Id}' , 'Setup{setupCount}',  'Duration: {task.Setup}' , new Date(0, 0, 0, 0, 0, {task.Start - task.Setup}) , new Date(0, 0, 0, 0, 0, {task.Start})],");
                            coloration = coloration + "'#000000',";
                            setupCount++;
                        }

                        //Schreibe Zeile für Task. Setze zufällige Farbe aus Array
                        sw.WriteLine($"[ 'Machine {task.Machine.Id}' , '{task.Job.Id}_{task.Id}', 'Duration: {task.Duration}' , new Date(0, 0, 0, 0, 0, {task.Start}) , new Date(0, 0, 0, 0, 0, {task.End})],");
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
                sw.WriteLine($"<div><p>Makespan: {makespan} Seconds. Seed Value: {seedValue} Processing Time: {watchTime}</p></div>");
                sw.WriteLine("<div id=\"example3.1\" style=\"height: 1000px;\"></div>");
            }

            //Öffne Diagramm direkt nach Abschluss der Methode
            if (openOnWrite == true)
            {
                string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string sFile = System.IO.Path.Combine(sCurrentDirectory, filepath);
                string sFilePath = Path.GetFullPath(sFile);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = sFilePath;
                    process.Start();
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
            foreach (Machine machine in machines)
            {
                Task lastTask = machine.Schedule[machine.Schedule.Count - 1];
                machine.Load = lastTask.End;

                if (lastTask.End > makespan)
                {
                    makespan = lastTask.End;
                }
            }
            return makespan;
        }

        //Tausche zwei Tasks auf einer Maschine
        public void SwapTasks(Task task1, Task task2, Machine machine)
        {
            int index1 = Jobs[task1.Job.Id].Tasks[task1.Id].Position;
            int index2 = Jobs[task2.Job.Id].Tasks[task2.Id].Position;

            Task tempTask = machines[machine.Id].Schedule[index1];
            machines[machine.Id].Schedule[index1] = machines[machine.Id].Schedule[index2];
            machines[machine.Id].Schedule[index2] = tempTask;

            //Console.WriteLine($"INFORMATION:Swapped Task Job: {Jobs[task1.Job.Id].Tasks[task1.Id].Job.Id} Task: {Jobs[task1.Job.Id].Tasks[task1.Id].Id} from {index1} with Task Job: {Jobs[task2.Job.Id].Tasks[task2.Id].Job.Id} Task: {Jobs[task2.Job.Id].Tasks[task2.Id].Id} from {index2} on Machine {machine.Id}");

            /*Parallel.Invoke(
                this.SetRelatedTasks, this.CalculateSetups
            );*/

            SetRelatedTasks();
            Recalculate();

        }

        //Gebe eine Liste von allen kritischen Tasks in einem Problem zurück
        public Dictionary<Machine, List<Task>> GetCriticalTasks()
        {
            /*Performance*/
            //Stopwatch critWatch = new Stopwatch();
            //critWatch.Start();

            Dictionary<Machine, List<Task>> critTasks = new Dictionary<Machine, List<Task>>();

            for (int j = 0; j < machines.Count(); j++)
            {
                critTasks.Add(machines[j], new List<Task>());
                for (int i = 0; i < machines[j].Schedule.Count; i++)
                {
                    Task task = machines[j].Schedule[i];
                    if (task.Release + task.Tail == CalculateMakespan())
                    {
                        critTasks[machines[j]].Add(task);
                        //Console.WriteLine($"INFORMATION:Task {task.Id} in Job {task.Job.Id} is critical");
                    }
                }
            }
            return critTasks;

            /*Performance*/
            //critWatch.Stop();
            //Console.WriteLine($"Related Watch ran {critWatch.Elapsed.Minutes} Minutes {critWatch.Elapsed.Seconds} Seconds {critWatch.Elapsed.Milliseconds} Milliseconds {critWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Setze alle Vorgänger und Nachfolger in einem Problem neu
        public void SetRelatedTasks()
        {
            /*Performance*/
            //Stopwatch relatedWatch = new Stopwatch();
            //relatedWatch.Start();

            foreach (Job job in jobs)
            {
                foreach (Task task in job.Tasks)
                {
                    if (task.Id - 1 >= 0)
                    {
                        task.preJobTask = job.Tasks[task.Id - 1];
                        //Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned preJobTask Job{job.Id}_Task{task.preJobTask.Id}");
                    }
                    else
                    {
                        //Console.WriteLine($"INFORMATION:Couldn't assign preJobTask to Job_{job.Id} Task_{task.Id} Task has no Predecessor in his Job");
                        task.preJobTask = null;
                    }

                    if (task.Id + 1 < job.Tasks.Count)
                    {
                        task.sucJobTask = job.Tasks[task.Id + 1];
                        //Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned sucJobTask Job{job.Id}_Task{task.sucJobTask.Id}");
                    }
                    else
                    {
                        //Console.WriteLine($"INFORMATION:Couldn't assign sucJobTask to Job{job.Id}_Task{task.Id} has no Successor in his Job");
                        task.sucJobTask = null;
                    }
                }
            }

            foreach (Machine machine in machines)
            {
                for (int i = 0; i < machine.Schedule.Count; i++)
                {
                    if (i - 1 >= 0)
                    {
                        machine.Schedule[i].preMachineTask = machine.Schedule[i - 1];
                        //Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has preMachineTask Job{machine.Schedule[i].preMachineTask.Job.Id}_Task{machine.Schedule[i].preMachineTask.Id}");
                    }
                    else
                    {
                        //Console.WriteLine($"INFORMATION:Couldn't assign preMachineTask to Job{machine.Schedule[i].Job.Id}_Task {machine.Schedule[i].Id} Task has no Predecessor in his Machine");
                        machine.Schedule[i].preMachineTask = null;
                    }

                    if (i + 1 < machine.Schedule.Count)
                    {
                        machine.Schedule[i].sucMachineTask = machine.Schedule[i + 1];
                        //Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has sucMachineTask Job{machine.Schedule[i].sucMachineTask.Job.Id}_Task{machine.Schedule[i].sucMachineTask.Id}");
                    }
                    else
                    {
                        // Console.WriteLine($"INFORMATION:Couldn't assign sucMachineTask to Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} Task has no Successor in his Machine");
                        machine.Schedule[i].sucMachineTask = null;
                    }
                    machine.Schedule[i].Position = i;
                }
            }

            /*Performance*/
            //relatedWatch.Stop();
            //Console.WriteLine($"Related Watch ran {relatedWatch.Elapsed.Minutes} Minutes {relatedWatch.Elapsed.Seconds} Seconds {relatedWatch.Elapsed.Milliseconds} Milliseconds {relatedWatch.Elapsed.Nanoseconds} Nanoseconds");
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
                    task.Release = -1;

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
                    releasePM = currentTask.preMachineTask.Release + currentTask.preMachineTask.Duration + currentTask.Setup;
                }

                if (currentTask.preJobTask is not null)
                {
                    releasePJ = currentTask.preJobTask.Release + currentTask.preJobTask.Duration;
                }

                currentTask.Release = Math.Max(releasePM, releasePJ);
                currentTask.Start = currentTask.Release;
                currentTask.End = currentTask.Start + currentTask.Duration;
                //Console.WriteLine($"INFORMATION:Updated Release of Task {currentTask.Id} in Job {currentTask.Job.Id} ");      


                if (currentTask.sucJobTask is not null)
                {
                    if (currentTask.sucJobTask.preMachineTask is null || currentTask.sucJobTask.preMachineTask.Release != -1)
                    {
                        releaseQueue.Enqueue(currentTask.sucJobTask);
                        //Console.WriteLine($"INFORMATION:Adding JobSuccessor Job{currentTask.sucJobTask.Job.Id}_Task{currentTask.sucJobTask.Id} of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                    }
                }

                if (currentTask.sucMachineTask is not null)
                {
                    if (currentTask.sucMachineTask.preJobTask == null || currentTask.sucMachineTask.preJobTask.Release != -1)
                    {
                        releaseQueue.Enqueue(currentTask.sucMachineTask);
                        //Console.WriteLine($"INFORMATION:Adding MachineSuccessor Job{currentTask.sucMachineTask.Job.Id}_Task{currentTask.sucMachineTask.Id} of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                    }
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

                //Update Tail mit dem Job oder Maschinen Maximum
                currentTask.Tail = Math.Max(tailSM, tailSJ);
                if (currentTask.sucMachineTask == null && currentTask.sucJobTask == null)
                {
                    this.Makespan = currentTask.Release + currentTask.Tail;
                }
                //Console.WriteLine($"RESULT:Updated Tail of Job{currentTask.Job.Id}_Task{currentTask.Id}");

                //Füge der Liste den Vorgänger im Job dieses Tasks hinzu
                if (currentTask.preJobTask is not null)
                {
                    if (currentTask.preJobTask.sucMachineTask == null || currentTask.preJobTask.sucMachineTask.Tail != -1)
                    {
                        tailQueue.Enqueue(currentTask.preJobTask);
                    }
                }

                //Füge der Liste den Vorgänger auf der Maschine dieses Tasks hinzu
                if (currentTask.preMachineTask is not null)
                {
                    if (currentTask.preMachineTask.sucJobTask == null || currentTask.preMachineTask.sucJobTask.Tail != -1)
                    {
                        tailQueue.Enqueue(currentTask.preMachineTask);
                    }
                }
            }
        }

        public bool ConfirmFeasability()
        {
            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Tail == -1 || task.Release == -1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Switch-Case Anweisung zur Auswahl der Nachbarschaft
        public Dictionary<int, List<Tuple<Task, Task, Machine>>> GetNeighboorhood(string searchMethod)
        {
            Dictionary<int, List<Tuple<Task, Task, Machine>>> newDict = new Dictionary<int, List<Tuple<Task, Task, Machine>>>();

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

        public Dictionary<int, List<Tuple<Task, Task, Machine>>> N1()
        {
            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task, Machine>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task, Machine>>>();

            //Problem returnProblem = problem;

            //int makespan = CalculateMakespan();

            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                foreach (Task task in critPair.Value)
                {
                    if (task.sucMachineTask != null && critTasks[task.Machine].Contains(task.sucMachineTask))
                    {
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine) });
                    }
                }
            }
            return swapOperations;
        }

        public Dictionary<int, List<Tuple<Task, Task, Machine>>> N3()
        {

            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task, Machine>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task, Machine>>>();

            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                foreach (Task task in critPair.Value)
                {
                    bool firstNeighbor = false;

                    if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasks[task.Machine].Contains(task.sucMachineTask))
                    {
                        //p(i), i, j --> task = i

                        //p(i), j, i
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine) });

                        //j, p(i), i
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine), Tuple.Create(task, task.sucMachineTask, task.Machine) });

                        //j, i, p(i) --> Wenn diese Nachbarschaft abgespeichert ist, muss s(j), j, i nicht gespeichert werden.
                        firstNeighbor = swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                    }

                    if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasks[task.Machine].Contains(task.preMachineTask))
                    {
                        //i, j, s(j) --> task = j 

                        //j, i, s(j)
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine) });

                        //j, s(j), i
                        swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine), Tuple.Create(task.preMachineTask, task, task.Machine) });

                        //s(j), j, i
                        if (!firstNeighbor)
                        {
                            swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                        }
                    }
                }
            }
            return swapOperations;
        }

        public Dictionary<int, List<Tuple<Task, Task, Machine>>> N5()
        {
            Dictionary<Machine, List<Task>> critTasks = GetCriticalTasks();
            Dictionary<int, List<Tuple<Task, Task, Machine>>> swapOperations = new Dictionary<int, List<Tuple<Task, Task, Machine>>>();

            Dictionary<Tuple<Machine, int>, List<Task>> critBlocks = new Dictionary<Tuple<Machine, int>, List<Task>>();

            foreach (KeyValuePair<Machine, List<Task>> critPair in critTasks)
            {
                int currentBlock = 0;

                if (critPair.Value.Count > 1)
                {
                    //critBlocks.Add(Tuple.Create(critPair.Key, currentBlock), new List<Task> { critPair.Value[0] });

                    for (int i = 0; i < critPair.Value.Count - 1; i++)
                    {
                        if ((!critBlocks.ContainsKey(Tuple.Create(critPair.Key, currentBlock)) && critPair.Value[i].sucMachineTask is not null))
                        {
                            critBlocks.Add(Tuple.Create(critPair.Key, currentBlock), new List<Task> { critPair.Value[i] });
                        }

                        if (critPair.Value[i + 1] == critPair.Value[i].sucMachineTask)
                        {
                            critBlocks[Tuple.Create(critPair.Key, currentBlock)].Add(critPair.Value[i + 1]);
                        }
                        else if (critBlocks[Tuple.Create(critPair.Key, currentBlock)].Count == 1)
                        {
                            critBlocks.Remove(Tuple.Create(critPair.Key, currentBlock));
                        }
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

                //Für den ersten Block werden die letzten zwei Tasks getauscht
                if (blockPair.Value[0].Release == 0)
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[lastTaskIndex], blockPair.Value[lastTaskIndex].preMachineTask, blockPair.Key.Item1) });
                }

                //Für den letzten Block werden die ersten zwei Tasks getauscht
                else if (blockPair.Value[lastTaskIndex].Release + blockPair.Value[lastTaskIndex].Duration == this.CalculateMakespan())
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }

                //Für den 
                else if ((lastTaskIndex + 1) == 2)
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }
                else
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[lastTaskIndex], blockPair.Value[lastTaskIndex].preMachineTask, blockPair.Key.Item1) });
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }




            }
            return swapOperations;
        }
    }
}