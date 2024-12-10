using Projektseminar.Instance;

namespace Projektseminar.OpeningHeuristic
{
    internal class Giffler_Thompson
    {
        //Eigenschaften
        public string PriorityRule
        {
            get => priorityRule; 
            set => priorityRule = value;
        }


        //Variablen
        private Problem problem;

        private string priorityRule;
        
        //Konstruktoren
        public Giffler_Thompson(Problem problem, string priorityRule)
        {
            this.problem = problem;
            this.priorityRule = priorityRule;
        }

        //Methoden
        public Problem InitialSolution()
        {
            //Initialisierungfunktion für Load, Releasezeit, usw.
            Init(problem);

            //While-Schleife die läuft solange plannbare Task vorhanden sind
            while (true)
            {

                //Erstelle eine Liste mit Tasks die aktuell einplanbar sind
                List<Instance.Task> plannableTasks = GetPlannableTasks(problem);

                //Wenn keine Task mehr verplant werden können beende den Algorithmus.
                if (plannableTasks.Count == 0)
                {
                    problem.SetRelatedTasks();
                    problem.CalculateSetups();
                    problem.CalculateReleases();
                    problem.CalculateTail();

                    return problem;
                }
                //Initialisiere großen Integer
                int minAllTask = problem.Horizon;

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
                Instance.Task planTask = GetTaskByPriorityRule(priorityRule, sameMachineTasks, problem);
                Instance.Task prevTask = null;

                if (planTask.Machine.Schedule.Count > 0)
                {
                    prevTask = planTask.Machine.Schedule[planTask.Machine.Schedule.Count - 1];

                    planTask.Setup = problem.Setups[Tuple.Create(prevTask.Job.Id, planTask.Job.Id)];

                    planTask.Start = Math.Max(planTask.Machine.Load + planTask.Setup, planTask.Release);
                }
                else
                {
                    planTask.Start = Math.Max(planTask.Machine.Load, planTask.Release);
                }

                /*if (prevTask is not null)
                {
                    //problem.GetSetup(prevTask.Job, planTask.Job, planTask.Machine);
                    planTask.Setup = problem.Setups[Tuple.Create(prevTask.Job.Id, planTask.Job.Id)];

                    planTask.Start = Math.Max(planTask.Machine.Load + planTask.Setup, planTask.Release);
                }
                else
                {
                    planTask.Start = Math.Max(planTask.Machine.Load, planTask.Release);
                }*/

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

        private List<Instance.Task> GetPlannableTasks(Problem problem)
        {
            List<Instance.Task> plannableTasks = new List<Instance.Task>();
            foreach (Job job in problem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    //Wenn die Startzeit kleiner als BigN ist und der Task noch nicht eingeplant, wird er der Liste hinzugefügt.
                    if (task.Release < problem.Horizon && task.Start == 0 && task.End == 0)
                    {
                        plannableTasks.Add(task);
                        //Pro Job wird nur der erste Task eingeplant.
                        break;
                    }
                }
            }
            return plannableTasks;
        }

        private static void Init(Problem problem)
        {
            foreach (Job job in problem.Jobs)
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
                        task.Release = problem.Horizon;
                    }
                    task.Machine.Load = 0;
                }
            }
        }
        private static Instance.Task GetTaskByPriorityRule(string priorityRule, List<Instance.Task> sameMachineTasks, Problem problem)
        {
            int prio;
            Instance.Task planTask = null;
            switch (priorityRule)
            {
                case "STT":
                    prio = problem.Horizon;
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
                    prio = problem.Horizon;
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        if (task.Job.TotalDuration < prio)
                        {
                            prio = task.Job.TotalDuration;
                            planTask = task;
                        }
                    }
                    break;
                default:
                    break;
            }
            return planTask;
        }

    }
}
