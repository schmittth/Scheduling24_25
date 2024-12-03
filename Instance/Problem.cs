using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Projektseminar.Instance
{
    internal class Problem
    {
        // Eigenschaften
        public int Id { get; }
        public int Horizon { get => horizon; set => horizon = value; }

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

            SetRelatedTasks(false);

            /*Performance*/
            //copyWatch.Stop();
            //Console.WriteLine($"Copy Watch ran {copyWatch.Elapsed.Minutes} Minutes {copyWatch.Elapsed.Seconds} Seconds {copyWatch.Elapsed.Milliseconds} Milliseconds {copyWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Methoden

        //Problem in der Konsole ausgeben
        public void PrintProblem()
        {
            string output = "";
            foreach (Machine machine in machines)
            {
                // Sort by starting time.
                machine.Schedule.Sort();
                string solLineTasks = $"Machine {machine.Id}: ";
                string solLine = "           ";

                int previousJob = -1;

                foreach (var task in machine.Schedule)
                {
                    /*if (previousJob != -1)
                    {
                        String setupTimeName = $"setup_{previousJob}_{assignedTask.jobID}";
                        solLineTasks += $"{setupTimeName,-15}";

                        String setupTime = $"[{assignedTask.start - setupTimes[previousJob, assignedTask.jobID]},{assignedTask.start}]";
                        solLine += $"{setupTime,-15}";
                    }*/

                    string name = $"job__task_{task.Id}";
                    // Add spaces to output to align columns.
                    solLineTasks += $"{name,-15}";


                    string solTmp = $"[{task.Start},{task.End}]";
                    // Add spaces to output to align columns.
                    solLine += $"{solTmp,-15}";


                }
                output += solLineTasks + "\n";
                output += solLine + "\n";
            }

            Console.WriteLine($"\n{output}");
        }

        //Problem als Diagramm in den angegebenen Pfad schreiben
        public void ProblemAsDiagramm(string filepath)
        {
            File.Copy(@"..\..\..\template.html", filepath, true);

            using (StreamWriter sw = File.AppendText(filepath))
            {
                foreach (Machine machine in machines)
                {
                    foreach (Task task in machine.Schedule)
                    {
                        var random = new Random();
                        var color = String.Format("#{0:X6}", random.Next(0x1000000));

                        sw.WriteLine($"[ 'Machine {machine.Id}' , '{task.Duration}' , new Date(0, 0, 0, 0, 0, {task.Start}) , new Date(0, 0, 0, 0, 0, {task.End}), '{color}' ],");

                        if (task.Setup != 0)
                        {
                            sw.WriteLine($"[ 'Machine {machine.Id}' , '{task.Setup}' , new Date(0, 0, 0, 0, 0, {task.Start - task.Setup}) , new Date(0, 0, 0, 0, 0, {task.Start}), '#111557' ],");
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
        public void SwapTasks(Task task1, Task task2, Machine machine, bool parallelMode)
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

            SetRelatedTasks(parallelMode);
            CalculateSetups(parallelMode);

            if (parallelMode)
            {
                Parallel.Invoke(
                  CalculateReleases, CalculateTail
                );
            }
            else
            {
                CalculateReleases();
                CalculateTail();
            }
        }

        //Gebe eine Liste von allen kritischen Tasks in einem Problem zurück
        public List<Task> GetCriticalTasks()
        {
            /*Performance*/
            //Stopwatch critWatch = new Stopwatch();
            //critWatch.Start();

            List<Task> critPath = new List<Task>();
            int makespan = CalculateMakespan();

            foreach (Machine machine in machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Release + task.Tail == makespan)
                    {
                        critPath.Add(task);
                        //Console.WriteLine($"INFORMATION:Task {task.Id} in Job {task.Job.Id} is critical");
                    }
                }
            }
            return critPath;

            /*Performance*/
            //critWatch.Stop();
            //Console.WriteLine($"Related Watch ran {critWatch.Elapsed.Minutes} Minutes {critWatch.Elapsed.Seconds} Seconds {critWatch.Elapsed.Milliseconds} Milliseconds {critWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        //Setze alle Vorgänger und Nachfolger in einem Problem neu
        public void SetRelatedTasks(bool parallelMode)
        {
            /*Performance*/
            //Stopwatch relatedWatch = new Stopwatch();
            //relatedWatch.Start();

            switch (parallelMode)
            {
                case true:
                    Parallel.ForEach(Jobs, job =>
                    {
                        Parallel.ForEach(job.Tasks, task =>
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
                        });
                    });

                    Parallel.ForEach(Machines, machine =>
                    {
                        Parallel.For(0, machine.Schedule.Count, i =>
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
                                //Console.WriteLine($"INFORMATION:Couldn't assign sucMachineTask to Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} Task has no Successor in his Machine");
                                machine.Schedule[i].sucMachineTask = null;
                            }
                            machine.Schedule[i].Position = i;
                        });
                    });
                    break;

                case false:
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
                    break;

                default:
                    break;

            }

            /*Performance*/
            //relatedWatch.Stop();
            //Console.WriteLine($"Related Watch ran {relatedWatch.Elapsed.Minutes} Minutes {relatedWatch.Elapsed.Seconds} Seconds {relatedWatch.Elapsed.Milliseconds} Milliseconds {relatedWatch.Elapsed.Nanoseconds} Nanoseconds");
        }

        public void CalculateSetups(bool parallelMode)
        {
            /*Performance*/
            //Stopwatch setupWatch = new Stopwatch();
            //setupWatch.Start();

            switch (parallelMode)
            {
                case true:
                    Parallel.ForEach(Machines, machine =>
                    {
                        Parallel.ForEach(machine.Schedule, task =>
                        {
                            if (task.preMachineTask is not null)
                            {
                                task.Setup = Setups[Tuple.Create(task.preMachineTask.Job.Id, task.Job.Id)];
                            }
                            else
                            {
                                task.Setup = 0;
                            }
                        });
                    });
                    break;

                case false:
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
                    break;

                default:
                    break;
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

            //Initialisiere i mit 0 und iteriere solange Elemente in Liste
            //for (int i = 0; tailUpdate.Count > 0; i++)
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

        public ConcurrentDictionary<int, Tuple<Problem, int>> GenerateNeighboorhood (string searchMethod)
        {
            /*Performance*/
            Stopwatch neighborWatch = new Stopwatch();
            neighborWatch.Start();

            ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> allOperations = GetNeighboorhood(searchMethod);
            ConcurrentDictionary<int, Tuple<Problem,int>> fullNeighboorhood = new ConcurrentDictionary<int, Tuple<Problem, int>>();

            //Parallel.ForEach(allOperations, operationPair =>
            foreach (KeyValuePair<int, List<Tuple<Task, Task, Machine>>> operationPair in allOperations)
            {
                Problem newProblem = new Problem(this);

                foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in operationPair.Value)
                {
                    newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3, false);
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
        public ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> GetNeighboorhood(string searchMethod)
        //public Dictionary<int, List<Tuple<Task, Task, Machine>>> GetNeighboorhood(string searchMethod)
        {
            ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> newDict = new ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>>();
            //Dictionary<int, List<Tuple<Task, Task, Machine>>> newDict = new Dictionary<int, List<Tuple<Task, Task, Machine>>>();

            switch (searchMethod)
            {
                case "N1":
                    newDict = N1();
                    break;
                case "N3":
                    newDict = N3(false);
                    break;
                default:
                    break;
            }
            return newDict;

        }

        public ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> N1()
        {
            List<Task> critTasks = GetCriticalTasks();
            ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> dict = new ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>>();

            //Problem returnProblem = problem;

            int makespan = CalculateMakespan();

            foreach (Task task in critTasks)
            {
                if (task.sucMachineTask != null && critTasks.Contains(task.sucMachineTask))
                {
                    dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine) });
                }
            }
            return dict;
        }

        public ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> N3(bool parallelMode)
        //public Dictionary<int, List<Tuple<Task, Task, Machine>>> N3(bool parallelMode)
        {
            switch (parallelMode)
            {
                case true:
                    List<Task> critTasksParallel = GetCriticalTasks();
                    ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> dict = new ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>>();

                    Parallel.ForEach(critTasksParallel, task =>
                    {
                        if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasksParallel.Contains(task.sucMachineTask))
                        {
                            //Erster Fall
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine) });

                            //ZweiterFall -- geht noch nicht
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine), Tuple.Create(task, task.sucMachineTask, task.Machine) });

                            //Dritter Fall
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                        }
                        else if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasksParallel.Contains(task.preMachineTask))
                        {
                            //Erster Fall
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine) });

                            //ZweiterFall
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine), Tuple.Create(task.preMachineTask, task, task.Machine) });

                            //Dritter Fall
                            dict.TryAdd(dict.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                        }
                    });
                    return dict;

                case false:
                    List<Task> critTasksSequential = GetCriticalTasks();
                    ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>> dictSequential = new ConcurrentDictionary<int, List<Tuple<Task, Task, Machine>>>();

                    foreach (Task task in critTasksSequential)
                    {
                        if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasksSequential.Contains(task.sucMachineTask))
                        {
                            //Erster Fall
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine) });

                            //ZweiterFall -- geht noch nicht
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine), Tuple.Create(task, task.sucMachineTask, task.Machine) });

                            //Dritter Fall
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                        }
                        else if (task.preMachineTask is not null && task.sucMachineTask is not null && critTasksSequential.Contains(task.preMachineTask))
                        {
                            //Erster Fall
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task, task.Machine) });

                            //ZweiterFall
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task, task.sucMachineTask, task.Machine), Tuple.Create(task.preMachineTask, task, task.Machine) });

                            //Dritter Fall
                            dictSequential.TryAdd(dictSequential.Count, new List<Tuple<Task, Task, Machine>> { Tuple.Create(task.preMachineTask, task.sucMachineTask, task.Machine) });
                        }
                    }
                    return dictSequential;

            }
        }
    }
}