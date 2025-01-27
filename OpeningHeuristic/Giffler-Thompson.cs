using Projektseminar.Instance;

namespace Projektseminar.OpeningHeuristic
{
    internal class GifflerThompson : Solver
    {
        //Eigenschaften
        public string PriorityRule
        {
            get => priorityRule; 
            set => priorityRule = value;
        }


        //Variablen
        private string priorityRule;
        
        //Konstruktoren
        public GifflerThompson(Problem problem, string priorityRule)
        {
            this.BestProblem = problem;
            this.priorityRule = priorityRule;
        }

        //Methoden
        public Problem InitialSolution()
        {
            //Initialisierungfunktion für Load, Releasezeit, usw.
            Init();

            //While-Schleife die läuft solange plannbare Task vorhanden sind
            while (true)
            {

                //Erstelle eine Liste mit Tasks die aktuell einplanbar sind
                List<Instance.Task> plannableTasks = GetPlannableTasks();

                //Wenn keine Task mehr verplant werden können beende den Algorithmus.
                if (plannableTasks.Count == 0)
                {
                    BestProblem.SetRelatedTasks();
                    BestProblem.Recalculate();

                    return BestProblem;
                }

                //Initialisiere großen Integer
                int minAllTask = BestProblem.Horizon;

                //Bestimme Task der kleinsten Funktionswert hat.
                Instance.Task scopeTask = null;

                foreach (Instance.Task task in plannableTasks)
                {
                    int maxOneTask = Math.Max(task.Machine.Load, task.End) + task.Duration;

                    //Iteriere durch Tasks und bestimme kleinsten Funktionswert.
                    if (maxOneTask < minAllTask)
                    {
                        minAllTask = maxOneTask;
                        scopeTask = task;
                    }
                }

                //Bestimme all Tasks die auf der gleichen Maschine laufen sollen.
                List<Instance.Task> sameMachineTasks = new();
                foreach (Instance.Task task in plannableTasks)
                {
                    if (scopeTask.Machine == task.Machine)
                    {
                        sameMachineTasks.Add(task);
                    }
                }

                //Bestimme mit Prioritätsregel den ersten Task der eingeplant werden soll.                
                Instance.Task planTask = GetTaskByPriorityRule(priorityRule, sameMachineTasks);
                Instance.Task prevTask = null;

                if (planTask.Machine.Schedule.Count > 0)
                {
                    prevTask = planTask.Machine.Schedule[planTask.Machine.Schedule.Count - 1];

                    planTask.Setup = BestProblem.Setups[Tuple.Create(prevTask.Job.Id, planTask.Job.Id)];

                    planTask.Start = Math.Max(planTask.Machine.Load + planTask.Setup, planTask.Release);
                }
                else
                {
                    planTask.Start = Math.Max(planTask.Machine.Load, planTask.Release);
                }
              
                //Füge dem Schedule der Maschine den identifizierten Task hinzu.
                planTask.Position = planTask.Machine.Schedule.Count();
                planTask.Machine.Schedule.Add(planTask);


                //Setze die Endzeit des Tasks auf die Startzeit + Verarbeitungszeit
                planTask.End = planTask.Start + planTask.Duration;

                //Setze den aktuellen Load der Maschine auf das Ende des eingeplanten Tasks.
                planTask.Machine.Load = planTask.End;

                //Setze die Releasezeit des nächsten Tasks im Job auf das Ende des aktuellen Tasks
                if (planTask.Id + 1 < planTask.Job.Tasks.Count)
                {
                    planTask.Job.Tasks[planTask.Id + 1].Release = planTask.End;
                }
            }
        }

        private List<Instance.Task> GetPlannableTasks()
        {
            List<Instance.Task> plannableTasks = new List<Instance.Task>();
            foreach (Job job in BestProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    //Wenn die Startzeit kleiner als BigN ist und der Task noch nicht eingeplant, wird er der Liste hinzugefügt.
                    if (task.Release < BestProblem.Horizon && task.Start == 0 && task.End == 0)
                    {
                        plannableTasks.Add(task);
                        //Pro Job wird nur der erste Task eingeplant.
                        break;
                    }
                }
            }
            return plannableTasks;
        }

        private void Init()
        {
            foreach (Job job in this.BestProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    task.Start = 0;
                    task.End = 0;

                    //Setze den Release für alle einplanbaren Jobs auf 0
                    if (task.Id == 0)
                    {
                        task.Release = 0;
                    }

                    //Setze den Release für alle NICHT einplanbaren Jobs auf näherungsweise unendlich
                    else
                    {
                        task.Release = BestProblem.Horizon;
                    }
                    task.Machine.Load = 0;
                }
            }
        }
        private Instance.Task GetTaskByPriorityRule(string priorityRule, List<Instance.Task> sameMachineTasks)
        {
            int prio;
            Instance.Task planTask = null;
            HashSet<Job> applicableJobs = new HashSet<Job>();

            switch (priorityRule)
            {
                case "STT":
                    prio = BestProblem.Horizon;
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Duration < prio)
                        {
                            prio = task.Duration;
                            planTask = task;
                        }
                    }
                    break;
                case "LTT":
                    prio = 0;
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Duration > prio)
                        {
                            prio = task.Duration;
                            planTask = task;
                        }
                    }
                    break;
                case "LPT":
                    prio = 0;
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Job.TotalDuration > prio)
                        {
                            prio = task.Job.TotalDuration;
                            planTask = task;
                        }
                    }
                    break;
                case "SPT":
                    prio = BestProblem.Horizon;
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Job.TotalDuration < prio)
                        {
                            prio = task.Job.TotalDuration;
                            planTask = task;
                        }
                    }
                    break;
                case "LRPT":

                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        applicableJobs.Add(task.Job);
                    }

                    List<Tuple<Job, int>> jobsLRPT = new List<Tuple<Job, int>>();

                    foreach (Job job in applicableJobs)
                    {
                        int remainingTime = 0;
                        foreach (var task in job.Tasks)
                        {
                            if (task.Start < BestProblem.Horizon)
                            {
                                remainingTime = remainingTime + task.Duration;
                            }
                        }
                        jobsLRPT.Add(Tuple.Create(job, remainingTime));
                    }
                    Job longestJob = jobsLRPT.FirstOrDefault(job => job.Item2 == jobsLRPT.Max(j => j.Item2)).Item1;

                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Job == longestJob)
                        {
                            planTask = task;
                            break;
                        }
                    }
                    break;
                case "SRPT":
                    
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        applicableJobs.Add(task.Job);
                    }

                    List<Tuple<Job, int>> jobsSRPT = new List<Tuple<Job, int>>();

                    foreach (Job job in applicableJobs)
                    {
                        int remainingTime = 0;
                        foreach (var task in job.Tasks)
                        {
                            if (task.Start < BestProblem.Horizon)
                            {
                                remainingTime = remainingTime + task.Duration;
                            }
                        }
                        jobsSRPT.Add(Tuple.Create(job, remainingTime));
                    }
                    Job shortestJob = jobsSRPT.FirstOrDefault(job => job.Item2 == jobsSRPT.Min(j => j.Item2)).Item1;

                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Job == shortestJob)
                        {
                            planTask = task;
                            break;
                        }
                    }
                    break;
            }
            return planTask;
        }

    }
}
