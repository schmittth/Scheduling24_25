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
                    planTask.Start = Math.Max(planTask.Machine.Load + planTask.Setup, planTask.Start);
                }
                else
                {
                    planTask.Start = Math.Max(planTask.Machine.Load, planTask.Start);
                }

                //Füge dem Schedule der Maschine den identifizierten Task hinzu.
                planTask.Position = planTask.Machine.Schedule.Count();
                planTask.Machine.Schedule.Add(planTask);

                planTask.End = planTask.Start + planTask.Duration; //Setze die Endzeit des Tasks auf die Startzeit + Verarbeitungszeit
                planTask.Machine.Load = planTask.End; //Setze den aktuellen Load der Maschine auf das Ende des eingeplanten Tasks.

                //Setze die Releasezeit des nächsten Tasks im Job auf das Ende des aktuellen Tasks
                if (planTask.Id + 1 < planTask.Job.Tasks.Count)
                {
                    planTask.Job.Tasks[planTask.Id + 1].Start = planTask.End;
                }
            }
        }

        //Methode um Liste an allen einplanparen Tasks zurückzugeben
        private List<Instance.Task> GetPlannableTasks()
        {
            List<Instance.Task> plannableTasks = new List<Instance.Task>();
            foreach (Job job in BestProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    //Wenn die Startzeit kleiner als BigN ist und der Task noch nicht eingeplant, wird er der Liste hinzugefügt.
                    if (task.Start < BestProblem.Horizon && task.End == 0)
                    {
                        plannableTasks.Add(task);
                        //Pro Job wird nur der erste Task eingeplant.
                        break;
                    }
                }
            }
            return plannableTasks;
        }

        //Initiale Methode um initiale Releases zu bestimmen
        private void Init()
        {
            foreach (Job job in this.BestProblem.Jobs)
            {
                //Setze alle initialen Werte
                foreach (Instance.Task task in job.Tasks)
                {
                    task.Start = 0;
                    task.End = 0;
                    task.Start = BestProblem.Horizon;

                    task.Machine.Load = 0;
                }
                job.Tasks[0].Start = 0; //Setze den Release für alle einplanbaren Jobs auf 0
            }
        }

        //Methode um nach Prioritätsregel den nächsten Task zurückzugeben
        private Instance.Task GetTaskByPriorityRule(string priorityRule, List<Instance.Task> sameMachineTasks)
        {
            int value;
            Instance.Task planTask = null;
            switch (priorityRule)
            {
                case "STT":
                    value = BestProblem.Horizon;

                    //Iteriere durch alle Tasks auf der gleichen Maschine um den mit der kleinsten Bearbeitungszeit zu finden
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        //Wenn die Laufzeit dieses Tasks kleiner als die aktuell kleinste, setze Wert neu
                        if (task.Duration < value)
                        {
                            value = task.Duration;
                            planTask = task;
                        }
                    }
                    break;
                case "LTT":
                    value = 0;

                    //Iteriere durch alle Tasks auf der gleichen Maschine um den mit der längsten Bearbeitungszeit zu finden
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        //Wenn die Laufzeit dieses Tasks größer als die aktuell größte, setze Wert neu
                        if (task.Duration > value)
                        {
                            value = task.Duration;
                            planTask = task;
                        }
                    }
                    break;
                case "LPT":
                    value = 0;

                    //Iteriere durch alle Tasks auf der gleichen Maschine um den zu finden dessen Job die längste Bearbeitungszeit hat
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        //Wenn die gesamte Joblaufzeit größer als die aktuell größte, setze Wert neu
                        if (task.Job.TotalDuration > value)
                        {
                            value = task.Job.TotalDuration;
                            planTask = task;
                        }
                    }
                    break;
                case "SPT":
                    value = BestProblem.Horizon;

                    //Iteriere durch alle Tasks auf der gleichen Maschine um den zu finden dessen Job die kürzeste Bearbeitungszeit hat
                    foreach (Instance.Task task in sameMachineTasks)
                    {
                        //Wenn die gesamte Joblaufzeit kleiner als die aktuell kleinste, setze Wert neu
                        if (task.Job.TotalDuration < value)
                        {
                            value = task.Job.TotalDuration;
                            planTask = task;
                        }
                    }
                    break;
            }
            return planTask;
        }

    }
}