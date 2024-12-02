using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
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
        private List<Machine> machines = new List<Machine>();
        Dictionary<Tuple<int, int>, int> setups = new Dictionary<Tuple<int, int>, int>();

        private int horizon = 0;

        //Konstruktoren
        public Problem(int id)
        {
            Guid = new Guid();
            Id = id;
        }

        public Problem()
        {
            Guid = new Guid();
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
            File.Copy(@"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\template.html", filepath, true);

            using (StreamWriter sw = File.AppendText(filepath))
            {
                foreach (Machine machine in Machines)
                {
                    foreach (Task task in machine.schedule)
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

        //Methode um den aktuellen Makespan eines Problems zu erhalten
        public int CalculateMakespan()
        {
            int makespan = 0;

            //Iteriere durch alle Maschinen und suche das Ende des jeweils letzten Tasks
            foreach (Machine machine in Machines)
            {
                if (machine.Schedule[machine.Schedule.Count - 1].End > makespan)
                {
                    //Update den Makespan wenn die aktuelle Maschine größer ist.
                    makespan = machine.Schedule[machine.Schedule.Count - 1].End;
                }
            }
            return makespan;
        }

        public void SwapTasks(Task task1, Task task2, Machine machine)
        {

            //Suche nach den Indizes der Tasks
            int index1 = machine.Schedule.FindIndex(x => x.Id == task1.Id && x.Job.Id == task1.Job.Id);
            int index2 = machine.Schedule.FindIndex(x => x.Id == task2.Id && x.Job.Id == task2.Job.Id);

            Console.WriteLine($"Swapped Task {index1} with Task {index2} on Machine {machine.Id}");

            Task tempTask = machine.Schedule[index1];
            machine.Schedule[index1] = machine.Schedule[index2];
            machine.Schedule[index2] = tempTask;

            //Stelle sicher dass alle Vorgänger und Nachfolger wieder auf den richtigen Task zeigen
            /*if (task1.SuccessorMachine is not null && task1.SuccessorMachine.PredecessorMachine is not null) { task1.SuccessorMachine.PredecessorMachine = task2; }
            if (task1.PredecessorMachine is not null && task1.PredecessorMachine.SuccessorMachine is not null) { task1.PredecessorMachine.SuccessorMachine = task2; }
            if (task2.SuccessorMachine is not null && task2.SuccessorMachine.PredecessorMachine is not null) { task2.SuccessorMachine.PredecessorMachine = task1; }
            if (task2.PredecessorMachine is not null && task2.PredecessorMachine.SuccessorMachine is not null) { task2.PredecessorMachine.SuccessorMachine = task1; }*/

            this.SetRelatedTasks();
            this.CalculateSetups();
            this.CalculateReleases();
            this.CalculateTail();
        }

        //Methode um den kritischen Pfad eines Problems herauszufinden
        public List<Task> GetCriticalPath()
        {
            List<Task> critTasks = new List<Task>();
            int makespan = CalculateMakespan();

            //Iteriere durch alle Maschinen. Speichere die Tasks die kritisch sind. 
            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.Release + task.Tail == makespan)
                    {
                        critTasks.Add(task);
                        Console.WriteLine($"Task {task.Id} in Job {task.Job.Id} is critical");
                    }
                }
            }
            return critTasks;
        }

        //Methode um alle Nachfolger und Vorgänger Variablen zu aktualisieren.
        public void SetRelatedTasks()
        {
            //Setze die Vorgänger und Nachfolger für einen Task im gleichen Job.
            foreach (Job job in Jobs)
            {
                //Versuche für jeden Task den Vorgänger im Job und Nachfolger im Job zu setzen. Setze auf null wenn Task keinen Nachfolger oder Vorgänger im Job hat.
                foreach (Task task in job.Tasks)
                {
                    try { task.PredecessorJob = job.Tasks[task.Id - 1]; } 
                    catch (ArgumentOutOfRangeException noPredecessorInJob) {Console.WriteLine($"INFORMATION:Couldn't assign PredecessorJob to Job {job.Id} Task {task.Id} Task has no Predecessor in his Job"); task.PredecessorJob = null;}

                    try {task.SuccessorJob = job.Tasks[task.Id + 1];}
                    catch (ArgumentOutOfRangeException noSuccessorInJob) {Console.WriteLine($"INFORMATION:Couldn't assign SuccessorJob to Job {job.Id} Task {task.Id} has no Successor in his Job");task.SuccessorJob = null;}
                }
            }

            //Setze die Vorgänger und Nachfolger für einen Task auf der gleichen Maschine. Setze auf null wenn Task keinen Nachfolger oder Vorgänger auf seiner Maschine hat.
            foreach (Machine machine in Machines)
            {
                for (int i = 0; i < machine.Schedule.Count; i++)
                {
                    try
                    {
                        machine.Schedule[i].PredecessorMachine = machine.Schedule[i - 1];
                    }
                    catch (ArgumentOutOfRangeException noPredecessorInMachine)
                    {
                        Console.WriteLine($"INFORMATION:Couldn't assign PredecessorMachine to Job {machine.Schedule[i].Job.Id} Task {machine.Schedule[i].Id} Task has no Predecessor in his Machine");
                        machine.Schedule[i].PredecessorMachine = null;
                    }

                    try
                    {
                        machine.Schedule[i].SuccessorMachine = machine.Schedule[i + 1];
                    }
                    catch (ArgumentOutOfRangeException noSuccessorinMachine)
                    {
                        Console.WriteLine($"INFORMATION:Couldn't assign SuccessorMachine to Job {machine.Schedule[i].Job.Id} Task {machine.Schedule[i].Id} Task has no Successor in his Machine");
                        machine.Schedule[i].SuccessorMachine = null;
                    }
                }
            }
        }

        //Methode um nach einer Änderung die Setups neu zu kalkulieren.
        public void CalculateSetups()
        {
            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    try { task.Setup = Setups[Tuple.Create(task.PredecessorMachine.Job.Id, task.Job.Id)];}
                    catch (NullReferenceException noSetupFound) {Console.WriteLine($"INFORMATION:No Setup found because Job {task.Job.Id} Task {task.Id} doesn't have a Predecessor on his machine");task.Setup = 0;}
                }
            }
        }

        //Methode um nach einer Änderung die Tails neu zu kalulieren.
        public void CalculateTail()
        {
            List<Task> tailUpdate = new List<Task>();

            //Iteriere durch alle Tasks auf Maschinen und füge die ohne Nachfolger direkt der Update-Liste hinzu.
            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.SuccessorMachine == null && task.SuccessorJob == null)
                    {
                        tailUpdate.Add(task);
                    }
                    task.Tail = -1;
                }
            }

            //Iteriere solange bis die Update-Liste leer ist.
            for (int i = 0; tailUpdate.Count > 0; i++)
            {
                int tailSM = tailUpdate[i].Duration, tailSJ = tailUpdate[i].Duration;

                try {tailSM = tailUpdate[i].SuccessorMachine.Tail + tailUpdate[i].Duration + tailUpdate[i].SuccessorMachine.Setup;}
                catch (NullReferenceException noSuccessors) {Console.WriteLine($"WARNING:Couldn't find Tail of SuccessorMachine for Job {tailUpdate[i].Job.Id} Task {tailUpdate[i].Id} Task has no Successor in his Machine");}

                try {tailSJ = tailUpdate[i].SuccessorJob.Tail + tailUpdate[i].Duration;}
                catch (Exception noSuccessors)
                {Console.WriteLine($"WARNING:Couldn't find Tail of SuccessorJob for Job {tailUpdate[i].Job.Id} Task {tailUpdate[i].Id} Task has no Successor in his Machine");}

                tailUpdate[i].Tail = Math.Max(tailSM, tailSJ);
                Console.WriteLine($"Updated Tail of Task{tailUpdate[i].Id} in Job {tailUpdate[i].Job.Id} ");

                try
                {
                    if (tailUpdate[i].PredecessorJob.SuccessorMachine == null || tailUpdate[i].PredecessorJob.SuccessorMachine.Tail != -1)
                    {
                        tailUpdate.Add(tailUpdate[i].PredecessorJob);
                    }
                }
                catch (Exception noPredecessor)
                {

                    Console.WriteLine($"WARNING:Not adding PredecessorJob of {tailUpdate[i].Job.Id} Task {tailUpdate[i].Id} Task to Tail calculation");
                }

                try
                {
                    if (tailUpdate[i].PredecessorMachine.SuccessorJob == null || tailUpdate[i].PredecessorMachine.SuccessorJob.Tail != -1)
                    {
                        tailUpdate.Add(tailUpdate[i].PredecessorMachine);
                    }
                }
                catch (Exception noPredecessor)
                {

                    Console.WriteLine($"WARNING:Not adding PredecessorMachine of {tailUpdate[i].Job.Id} Task {tailUpdate[i].Id} Task to Tail calculation");
                }

                tailUpdate.Remove(tailUpdate[i]);
                i--;
            }
        }
        public void CalculateReleases()
        {
            List<Task> releaseUpdate = new List<Task>();

            foreach (Machine machine in Machines)
            {
                foreach (Task task in machine.Schedule)
                {
                    if (task.PredecessorMachine == null && task.PredecessorJob == null)
                    {
                        releaseUpdate.Add(task);
                    }
                    task.Release = -1;
                }
            }

            for (int i = 0; releaseUpdate.Count > 0; i++)
            {
                int releasePM =0, releasePJ = 0;

                try
                {
                    releasePM = releaseUpdate[i].PredecessorMachine.Release + releaseUpdate[i].PredecessorMachine.Duration + releaseUpdate[i].Setup;
                }
                catch (Exception noPredecessor)
                {
                    Console.WriteLine("Task has no Predecessor in his Machine");
                }

                try
                {
                    releasePJ = releaseUpdate[i].PredecessorJob.Release + releaseUpdate[i].PredecessorJob.Duration;
                }
                catch (Exception noPredecessor)
                {
                    Console.WriteLine("Task has no Predecessor in his Job");

                }

                releaseUpdate[i].Release = Math.Max(releasePM,releasePJ);
                releaseUpdate[i].Start = releaseUpdate[i].Release;
                releaseUpdate[i].End = releaseUpdate[i].Start + releaseUpdate[i].Duration;
                Console.WriteLine($"Updated Release of Task{releaseUpdate[i].Id} in Job {releaseUpdate[i].Job.Id} ");      
                
                try
                {
                    if (releaseUpdate[i].SuccessorJob.PredecessorMachine == null || releaseUpdate[i].SuccessorJob.PredecessorMachine.Release != -1)
                    {
                        releaseUpdate.Add(releaseUpdate[i].SuccessorJob);
                    }
                }
                catch (Exception noSuccessor)
                {

                    Console.WriteLine("Task has no Successor in his Job");
                }

                try
                {
                    if (releaseUpdate[i].SuccessorMachine.PredecessorJob == null || releaseUpdate[i].SuccessorMachine.PredecessorJob.Release != -1)
                    {
                        releaseUpdate.Add(releaseUpdate[i].SuccessorMachine);
                    }
                }
                catch (Exception noSuccessor)
                {

                    Console.WriteLine("Task has no Successor in his Machine");
                }

                releaseUpdate.Remove(releaseUpdate[i]);
                i--;
            }
        }
    }
}