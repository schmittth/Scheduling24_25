﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

        //Klon-Konstruktor
        public Problem(Problem existingProblem)
        {
            /*Debug*/
            //Console.WriteLine("INFORMATION: Building new Problem");

            /*Performance*/
            //Stopwatch copyWatch = new Stopwatch();
            //copyWatch.Start();

            Horizon = existingProblem.Horizon;

            foreach (Machine machine in existingProblem.Machines)
            {
                Machine cloneMachine = new Machine(machine.Id);
                cloneMachine.Load = machine.Load;

                machines.Add(cloneMachine);
            }

            foreach (Job job in existingProblem.Jobs)
            {
                Job cloneJob = new Job(job.Id);
                jobs.Add(cloneJob);

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

            foreach (Machine machine in existingProblem.Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    machines[machine.Id].Schedule.Add(Jobs[task.Job.Id].Tasks[task.Id]);
                }
            }

            Setups = existingProblem.Setups;

            SetRelatedTasks();

            /*Performance*/
            //copyWatch.Stop();
            //Console.WriteLine($"Copy Watch ran {copyWatch.Elapsed.Minutes} Minutes {copyWatch.Elapsed.Seconds} Seconds {copyWatch.Elapsed.Milliseconds} Milliseconds {copyWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Methoden

        //Problem als Diagramm in den angegebenen Pfad schreiben
        public void ProblemAsDiagramm(string filepath, bool openOnWrite)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            File.Copy(@"..\..\..\Diagramms\template.html", filepath, true);

            SortedDictionary<int, string> jobToColor = new SortedDictionary<int, string>();
            var random = new Random();

            foreach (Job job in Jobs)
            {
                jobToColor.Add(job.Id, String.Format("#{0:X6}", random.Next(0x1000000)));
            }

            using (StreamWriter sw = File.AppendText(filepath))
            {
                foreach (Machine machine in machines)
                {
                    foreach (Task task in machine.Schedule)
                    {
                        sw.WriteLine($"[ 'Machine {task.Machine.Id}' , '{task.Job.Id} {task.Id}', '{task.Duration}' , new Date(0, 0, 0, 0, 0, {task.Start}) , new Date(0, 0, 0, 0, 0, {task.End}), '{jobToColor[task.Job.Id]}' ],");

                        if (task.Setup != 0)
                        {
                            sw.WriteLine($"[ 'Machine {task.Machine.Id}' , 'Setup',  '{task.Setup}' , new Date(0, 0, 0, 0, 0, {task.Start - task.Setup}) , new Date(0, 0, 0, 0, 0, {task.Start}), '#111557' ],");
                        }
                    }
                }
                sw.WriteLine("]);");
                sw.WriteLine("");
                sw.WriteLine("chart.draw(dataTable);");
                sw.WriteLine("}");
                sw.WriteLine("</script >");
                sw.WriteLine("");
                sw.WriteLine("<div id = \"example3.1\" style = \"height: 1000px;\" ></ div >");
            }

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
            CalculateSetups();
                CalculateReleases();
                CalculateTail();
            
        }

        //Gebe eine Liste von allen kritischen Tasks in einem Problem zurück
        public Dictionary<Machine, List<Task>> GetCriticalTasks()
        {
            /*Performance*/
            //Stopwatch critWatch = new Stopwatch();
            //critWatch.Start();

            Dictionary<Machine,List<Task>> critTasks = new Dictionary<Machine, List<Task>>();
            int makespan = CalculateMakespan();

            for (int j = 0; j < machines.Count(); j++)
            {
                critTasks.Add(machines[j],new List<Task>());
                for (int i = 0; i < machines[j].Schedule.Count; i++ )
                {
                    Task task = machines[j].Schedule[i];
                    if (task.Release + task.Tail == makespan)
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

        public void CalculateSetups()
        {
            /*Performance*/
            //Stopwatch setupWatch = new Stopwatch();
            //setupWatch.Start();

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
                }
            }

            /*Performance*/
            //setupWatch.Stop();
            //Console.WriteLine($"Setup Watch ran {setupWatch.Elapsed.Minutes} Minutes {setupWatch.Elapsed.Seconds} Seconds {setupWatch.Elapsed.Milliseconds} Milliseconds {setupWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Kalkuliere die Releaselzeiten für alle Tasks

        public void CalculateReleases()
        {
            /*Performance*/
            //Stopwatch releaseWatch = new Stopwatch();
            //releaseWatch.Start();

            Queue<Task> releaseQueue = new Queue<Task>();

            foreach (Machine machine in machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.preMachineTask == null && task.preJobTask == null)
                    {
                        releaseQueue.Enqueue(task);
                    }
                    task.Release = -1;
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
            /*Performance*/
            //releaseWatch.Stop();
            //Console.WriteLine($"Release Watch ran {releaseWatch.Elapsed.Minutes} Minutes {releaseWatch.Elapsed.Seconds} Seconds {releaseWatch.Elapsed.Milliseconds} Milliseconds {releaseWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Kalkuliere die Tailzeiten für alle Tasks
        public void CalculateTail()
        {
            /*Performance*/
            //Stopwatch tailWatch = new Stopwatch();
            //tailWatch.Start();

            Queue<Task> tailQueue = new Queue<Task>();

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.sucMachineTask == null && task.sucJobTask == null)
                    {
                        tailQueue.Enqueue(task);
                    }
                    task.Tail = -1;
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
            /*Performance*/
            //tailWatch.Stop();
            //Console.WriteLine($"Tail Watch ran {tailWatch.Elapsed.Minutes} Minutes {tailWatch.Elapsed.Seconds} Seconds {tailWatch.Elapsed.Milliseconds} Milliseconds {tailWatch.Elapsed.Nanoseconds} Nanoseconds");
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

        public Dictionary<int, Tuple<Problem, int>> GenerateNeighboorhood(string searchMethod)
        {
            /*Performance*/
            Stopwatch neighborWatch = new Stopwatch();
            neighborWatch.Start();

            Dictionary<int, List<Tuple<Task, Task, Machine>>> allOperations = GetNeighboorhood(searchMethod);
            Dictionary<int, Tuple<Problem, int>> fullNeighboorhood = new Dictionary<int, Tuple<Problem, int>>();

            //Parallel.ForEach(allOperations, operationPair =>
            foreach (KeyValuePair<int, List<Tuple<Task, Task, Machine>>> operationPair in allOperations)
            {
                Problem newProblem = new Problem(this);

                foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in operationPair.Value)
                {
                    newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                }

                if (newProblem.ConfirmFeasability())
                {
                    fullNeighboorhood.TryAdd(fullNeighboorhood.Count, Tuple.Create(newProblem, newProblem.CalculateMakespan()));
                }
            }//);
            /*Performance*/
            neighborWatch.Stop();
            Console.WriteLine($"Release Watch ran {neighborWatch.Elapsed.Minutes} Minutes {neighborWatch.Elapsed.Seconds} Seconds {neighborWatch.Elapsed.Milliseconds} Milliseconds {neighborWatch.Elapsed.Nanoseconds} Nanoseconds");
            return fullNeighboorhood;
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

            int makespan = CalculateMakespan();

            foreach (KeyValuePair<Machine,List<Task>> critPair in critTasks)
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

            int makespan = this.CalculateMakespan();
            foreach (KeyValuePair<Tuple<Machine, int>, List<Task>> blockPair in critBlocks)
            {
                if (blockPair.Value[0].Release == 0)
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[blockPair.Value.Count - 1], blockPair.Value[blockPair.Value.Count - 1].preMachineTask, blockPair.Key.Item1) });
                }
                else if (blockPair.Value[0].Release + blockPair.Value[0].Duration == makespan)
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }
                else if (blockPair.Value.Count <= 2)
                {                  
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }
                else
                {
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[blockPair.Value.Count - 1], blockPair.Value[blockPair.Value.Count - 1].preMachineTask, blockPair.Key.Item1) });
                    swapOperations.TryAdd(swapOperations.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(blockPair.Value[0], blockPair.Value[0].sucMachineTask, blockPair.Key.Item1) });
                }
            }
            return swapOperations;
        }
    }
}