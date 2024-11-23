using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JobShopSchedulingProblemCP.Instance
{
    internal class Problem
    {
        // Eigenschaften
        public Guid Guid { get; }
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
        private List<Machine> machines = new List <Machine>();
        private Dictionary<Tuple<int, int>, int> setups = new Dictionary<Tuple<int, int>, int>();

        private int horizon = 0;

        //Konstruktoren

        //Parameterloser Konstruktor
        public Problem()
        {
            Guid = new Guid();
        }

        //Klon-Konstruktor
        public Problem (Problem existingProblem)
        {
            Console.WriteLine("INFORMATION: Building new Problem");

            this.Guid = new Guid();
            this.Horizon = existingProblem.Horizon;

            foreach (Machine machine in existingProblem.Machines)
            {
                Machine cloneMachine = new Machine(machine.Id);
                cloneMachine.Load = machine.Load;

                machines.Add(cloneMachine);
            }

            foreach (Job job in existingProblem.Jobs)
            {
                Job cloneJob = new Job(job.Id);
                this.jobs.Add(cloneJob);

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
                    machines[machine.Id].Schedule.Add(this.Jobs[task.Job.Id].Tasks[task.Id]);
                }
            }

            this.Setups = existingProblem.Setups;

            this.SetRelatedTasks(false);
        }

        //Methoden

        //Problem in der Konsole ausgeben
        public void PrintProblem()
        {
            System.String output = "";
            foreach (Machine machine in this.machines)
            {
                // Sort by starting time.
                machine.Schedule.Sort();
                System.String solLineTasks = $"Machine {machine.Id}: ";
                System.String solLine = "           ";

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

                    System.String name = $"job__task_{task.Id}";
                    // Add spaces to output to align columns.
                    solLineTasks += $"{name,-15}";


                    System.String solTmp = $"[{task.Start},{task.End}]";
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
            File.Copy(@"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\template.html", filepath, true);

            using (StreamWriter sw = File.AppendText(filepath))
            {
                foreach (Machine machine in this.machines)
                {
                    foreach (Task task in machine.Schedule)
                    {
                        sw.WriteLine($"[ '{machine.Id}' , 'Job{task.Job.Id}_Task{task.Id}' , new Date(0, 0, 0, 0, 0, {task.Start}) , new Date(0, 0, 0, 0, 0, {task.End}) ],");

                        if (task.Setup != 0)
                        {
                            sw.WriteLine($"[ '{machine.Id}' , 'Setup' , new Date(0, 0, 0, 0, 0, {task.Start - task.Setup}) , new Date(0, 0, 0, 0, 0, {task.Start}) ],");
                        }
                    }
                }
                sw.WriteLine("]);");
                sw.WriteLine("");
                sw.WriteLine("chart.draw(dataTable);");
                sw.WriteLine("}");
                sw.WriteLine("</script >");
                sw.WriteLine("");
                sw.WriteLine("<div id = \"example3.1\" style = \"height: 200px;\" ></ div >");
            }

        }

        //Kalkuliere den Makespan und Update den Load
        public int CalculateMakespan()
        {
            int makespan = 0;
            foreach (Machine machine in machines)
            {
                Instance.Task lastTask = machine.Schedule[machine.Schedule.Count - 1];
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



            Console.WriteLine($"INFORMATION:Swapped Task Job: {Jobs[task1.Job.Id].Tasks[task1.Id].Job.Id} Task: {Jobs[task1.Job.Id].Tasks[task1.Id].Id} from {index1} with Task Job: {Jobs[task2.Job.Id].Tasks[task2.Id].Job.Id} Task: {Jobs[task2.Job.Id].Tasks[task2.Id].Id} from {index2} on Machine {machine.Id}");

            /*Parallel.Invoke(
                this.SetRelatedTasks, this.CalculateSetups
            );*/

            this.SetRelatedTasks(false);
            this.CalculateSetups(false);

            /*Parallel.Invoke(
                this.CalculateReleases,this.CalculateTail
            );*/

            this.CalculateReleases();
            this.CalculateTail();
        }

        //Gebe eine Liste von allen kritischen Tasks in einem Problem zurück
        public List<Task> GetCriticalPath()
        {
            List<Task> critPath = new List<Task>();
            int makespan = CalculateMakespan();

            foreach (Machine machine in machines)
            {              
                foreach (Task task in machine.Schedule)
                {
                    if (task.Release + task.Tail == makespan)
                    {
                        critPath.Add(task);
                        Debug.WriteLine($"INFORMATION:Task {task.Id} in Job {task.Job.Id} is critical");
                    }
                }
            }
            return critPath;
        }

        //Setze alle Vorgänger und Nachfolger in einem Problem neu
        public void SetRelatedTasks(bool parallelMode)
        {
            switch (parallelMode)
            {
                case true:
                    Parallel.ForEach(Jobs, job =>
                    {
                        Parallel.ForEach(job.Tasks, task =>
                        {
                            try
                            {
                                task.PredecessorJob = job.Tasks[task.Id - 1];
                                Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned PredeccessorJob Job{job.Id}_Task{task.PredecessorJob.Id}");
                            }
                            catch (ArgumentOutOfRangeException noPredecessorInJob)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign PredecessorJob to Job_{job.Id} Task_{task.Id} Task has no Predecessor in his Job");
                                task.PredecessorJob = null;
                            }

                            try
                            {
                                task.SuccessorJob = job.Tasks[task.Id + 1];
                                Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned SuccessorJob Job{job.Id}_Task{task.SuccessorJob.Id}");
                            }
                            catch (ArgumentOutOfRangeException noSuccessorInJob)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign SuccessorJob to Job{job.Id}_Task{task.Id} has no Successor in his Job");
                                task.SuccessorJob = null;
                            }
                        });
                    });

                    Parallel.ForEach(Machines, machine =>
                    {
                        Parallel.For(0, machine.Schedule.Count, i =>
                        {
                            try
                            {
                                machine.Schedule[i].PredecessorMachine = machine.Schedule[i - 1];
                                Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has PredecessorMachine Job{machine.Schedule[i].PredecessorMachine.Job.Id}_Task{machine.Schedule[i].PredecessorMachine.Id}");
                            }
                            catch (ArgumentOutOfRangeException noPredecessorInTask)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign PredecessorMachine to Job{machine.Schedule[i].Job.Id}_Task {machine.Schedule[i].Id} Task has no Predecessor in his Machine");
                                machine.Schedule[i].PredecessorMachine = null;
                            }

                            try
                            {
                                machine.Schedule[i].SuccessorMachine = machine.Schedule[i + 1];
                                Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has SuccessorMachine Job{machine.Schedule[i].SuccessorMachine.Job.Id}_Task{machine.Schedule[i].SuccessorMachine.Id}");
                            }
                            catch (ArgumentOutOfRangeException noSuccessorInTask)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign SuccessorMachine to Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} Task has no Successor in his Machine");
                                machine.Schedule[i].SuccessorMachine = null;
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
                            try
                            {
                                task.PredecessorJob = job.Tasks[task.Id - 1];
                                Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned PredeccessorJob Job{job.Id}_Task{task.PredecessorJob.Id}");
                            }
                            catch (ArgumentOutOfRangeException noPredecessorInJob)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign PredecessorJob to Job{job.Id}_Task_{task.Id} Task has no Predecessor in his Job");
                                task.PredecessorJob = null;
                            }

                            try
                            {
                                task.SuccessorJob = job.Tasks[task.Id + 1];
                                Console.WriteLine($"RESULT:Job{job.Id}_Task{task.Id} assigned SuccessorJob Job{job.Id}_Task{task.SuccessorJob.Id}");
                            }
                            catch (ArgumentOutOfRangeException noSuccessorInJob)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign SuccessorJob to Job{job.Id}_Task{task.Id} has no Successor in his Job");
                                task.SuccessorJob = null;
                            }
                        }
                    }

                    foreach (Machine machine in  machines)
                    {
                        for (int i = 0; i < machine.Schedule.Count; i++) 
                        {
                            try
                            {
                                machine.Schedule[i].PredecessorMachine = machine.Schedule[i - 1];
                                Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has PredecessorMachine Job{machine.Schedule[i].PredecessorMachine.Job.Id}_Task{machine.Schedule[i].PredecessorMachine.Id}");
                            }
                            catch (ArgumentOutOfRangeException noPredecessorInTask)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign PredecessorMachine to Job{machine.Schedule[i].Job.Id}_Task {machine.Schedule[i].Id} Task has no Predecessor in his Machine");
                                machine.Schedule[i].PredecessorMachine = null;
                            }

                            try
                            {
                                machine.Schedule[i].SuccessorMachine = machine.Schedule[i + 1];
                                Console.WriteLine($"RESULT:Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} has SuccessorMachine Job{machine.Schedule[i].SuccessorMachine.Job.Id}_Task{machine.Schedule[i].SuccessorMachine.Id}");
                            }
                            catch (ArgumentOutOfRangeException noSuccessorInTask)
                            {
                                Debug.WriteLine($"INFORMATION:Couldn't assign SuccessorMachine to Job{machine.Schedule[i].Job.Id}_Task{machine.Schedule[i].Id} Task has no Successor in his Machine");
                                machine.Schedule[i].SuccessorMachine = null;
                            }

                            machine.Schedule[i].Position = i;
                        }
                    }
                    break;

                default:
                    break;

            }           
        }

        public void CalculateSetups(bool parallelMode)
        {
            switch (parallelMode)
            {
                case true:
                    Parallel.ForEach(Machines, machine =>
                    {
                        Parallel.ForEach(machine.Schedule, task =>
                        {
                            try
                            {
                                task.Setup = Setups[Tuple.Create(task.PredecessorMachine.Job.Id, task.Job.Id)];
                            }
                            catch (NullReferenceException noSetup)
                            {
                                Debug.WriteLine("INFORMATION: Setup set to 0 because no Predecessor on this Machine");
                                task.Setup = 0;
                            }
                        });
                    });
                    break;

                case false:
                    foreach (Machine machine in Machines)
                    {
                        foreach (Instance.Task task in machine.Schedule)
                        {
                            try
                            {
                                task.Setup = Setups[Tuple.Create(task.PredecessorMachine.Job.Id, task.Job.Id)];
                            }
                            catch (NullReferenceException noSetup)
                            {
                                Debug.WriteLine("INFORMATION: Setup set to 0 because no Predecessor on this Machine");
                                task.Setup = 0;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        //Kalkuliere die Releaselzeiten für alle Tasks
        public void CalculateReleases()
        {
            Queue<Task> releaseQueue = new Queue<Task>();

            foreach (Machine machine in machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.PredecessorMachine == null && task.PredecessorJob == null)
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

                try
                {
                    releasePM = currentTask.PredecessorMachine.Release + currentTask.PredecessorMachine.Duration + currentTask.Setup;
                }
                catch (NullReferenceException predecessorInMachineUndefined)
                {
                    //Console.WriteLine($"INFORMATION:Couldn't find Release of PredecessorMachine for Job {currentTask.Job.Id} Task {currentTask.Id} Task has no Predecessor in his Machine");
                }

                try
                {
                    releasePJ = currentTask.PredecessorJob.Release + currentTask.PredecessorJob.Duration;
                }
                catch (Exception predecessorInJobUndefined)
                {
                    //Console.WriteLine($"INFORMATION:Couldn't find Release of PredecessorJob for Job {currentTask.Job.Id} Task {currentTask.Id} Task has no Predecessor in his Job");
                }

                currentTask.Release = Math.Max(releasePM, releasePJ);
                currentTask.Start = currentTask.Release;
                currentTask.End = currentTask.Start + currentTask.Duration;
                //Console.WriteLine($"INFORMATION:Updated Release of Task {currentTask.Id} in Job {currentTask.Job.Id} ");      

                try
                {
                    if (currentTask.SuccessorJob.PredecessorMachine == null || currentTask.SuccessorJob.PredecessorMachine.Release != -1)
                    {
                        releaseQueue.Enqueue(currentTask.SuccessorJob);
                        //Console.WriteLine($"INFORMATION:Adding JobSuccessor Job{currentTask.SuccessorJob.Job.Id}_Task{currentTask.SuccessorJob.Id} of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                    }
                }
                catch (Exception successorinJobUndefined)
                {

                    //Console.WriteLine($"INFORMATION:Not adding JobSuccessor of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                }

                try
                {
                    if (currentTask.SuccessorMachine.PredecessorJob == null || currentTask.SuccessorMachine.PredecessorJob.Release != -1)
                    {
                        releaseQueue.Enqueue(currentTask.SuccessorMachine);
                        //Console.WriteLine($"INFORMATION:Adding MachineSuccessor Job{currentTask.SuccessorMachine.Job.Id}_Task{currentTask.SuccessorMachine.Id} of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                    }
                }
                catch (Exception successorInMachineUndefined)
                {

                    Console.WriteLine($"INFORMATION:Not adding MachineSuccessor of Job{currentTask.Job.Id}_Task{currentTask.Id} to release queue");
                }
            }

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Release == -1)
                    {
                        throw new Exception("Not all Releases updated");
                    }
                }
            }
        }

        //Kalkuliere die Tailzeiten für alle Tasks
        public void CalculateTail()
        {
            Queue<Task> tailQueue = new Queue<Task>();

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.SuccessorMachine == null && task.SuccessorJob == null)
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
                try
                {
                    tailSM = currentTask.SuccessorMachine.Tail + currentTask.Duration + currentTask.SuccessorMachine.Setup;
                }
                catch (NullReferenceException successorInMachineUndefined)
                {
                    Debug.WriteLine($"INFORMATION:Couldn't find Tail of SuccessorMachine for Job{currentTask.Job.Id}_Task{currentTask.Id} Task has no Successor in his Machine");
                }

                //Identifiziere Tail des nachfolgenden Tasks im Job dieses Tasks
                try
                {
                    tailSJ = currentTask.SuccessorJob.Tail + currentTask.Duration;
                }
                catch (NullReferenceException successorInJobUndefined)
                {
                    Debug.WriteLine($"INFORMATION:Couldn't find Tail of SuccessorJob for Job {currentTask.Job.Id} Task {currentTask.Id} Task has no Successor in his Job");

                }

                //Update Tail mit dem Job oder Maschinen Maximum
                currentTask.Tail = Math.Max(tailSM, tailSJ);
                Console.WriteLine($"RESULT:Updated Tail of Task{currentTask.Id} in Job {currentTask.Job.Id} ");

                //Füge der Liste den Vorgänger im Job dieses Tasks hinzu
                try
                {
                    if (currentTask.PredecessorJob.SuccessorMachine == null || currentTask.PredecessorJob.SuccessorMachine.Tail != -1)
                    {
                        tailQueue.Enqueue(currentTask.PredecessorJob);
                    }
                }
                catch (NullReferenceException predecessorInJobUndefined)
                {
                    Debug.WriteLine($"INFORMATION:Not adding Job Predecessor of {currentTask.Job.Id} Task {currentTask.Id} Task to Tail calculation");
                }

                //Füge der Liste den Vorgänger auf der Maschine dieses Tasks hinzu
                try
                {
                    if (currentTask.PredecessorMachine.SuccessorJob == null || currentTask.PredecessorMachine.SuccessorJob.Tail != -1)
                    {
                        tailQueue.Enqueue(currentTask.PredecessorMachine);
                    }
                }
                catch (NullReferenceException predecessorInMachineUndefined)
                {
                    Debug.WriteLine($"INFORMATION:Not adding Machine Predecessor of {currentTask.Job.Id} Task {currentTask.Id} Task to Tail calculation");
                }
            }

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Tail == -1)
                    {
                        throw new Exception("Not all Tails updated");
                    }
                }
            }
        }

        //Switch-Case Anweisung zur auswahl der Nachbarschaft
        public ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> GetNeighboorhood(string searchMethod)
        {
            ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> newDict = new ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>>();

            switch (searchMethod)
            {
                case "N1":
                    newDict = this.N1();
                    break;
                case "N3":
                    newDict = this.N3(false);
                    break;
                default:
                    break;
            }
            return newDict;

        }

        public ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> N1()
        {
            List<Instance.Task> critTasks = this.GetCriticalPath();
            ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = new ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>>();

            //Problem returnProblem = problem;

            int makespan = this.CalculateMakespan();

            foreach (Instance.Task task in critTasks)
            {
                if (task.SuccessorMachine != null && critTasks.Contains(task.SuccessorMachine))
                {
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task, task.SuccessorMachine, task.Machine) });
                }
            }
            return dict;
        }

        public ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> N3(bool parallelMode)
        {
            List<Instance.Task> critTasks = this.GetCriticalPath();
            ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = new ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>>();

            Parallel.ForEach(critTasks, task =>
            {
                if (task.PredecessorMachine is not null && task.SuccessorMachine is not null && critTasks.Contains(task.SuccessorMachine))
                {
                    //Erster Fall
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task, task.SuccessorMachine, task.Machine) });

                    //ZweiterFall -- geht noch nicht
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task.PredecessorMachine, task, task.Machine), Tuple.Create(task, task.SuccessorMachine, task.Machine) });

                    //Dritter Fall
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task.PredecessorMachine, task.SuccessorMachine, task.Machine) });
                }
                else if (task.PredecessorMachine is not null && task.SuccessorMachine is not null && critTasks.Contains(task.PredecessorMachine))
                {
                    //Erster Fall
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task.PredecessorMachine, task, task.Machine) });

                    //ZweiterFall
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task, task.SuccessorMachine, task.Machine), Tuple.Create(task.PredecessorMachine, task, task.Machine) });

                    //Dritter Fall
                    dict.TryAdd(dict.LastOrDefault().Key + 1, new List<Tuple<Instance.Task, Instance.Task, Machine>> { Tuple.Create(task.PredecessorMachine, task.SuccessorMachine, task.Machine) });
                }
            });
            return dict;
        }
    }
}