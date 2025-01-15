using Google.OrTools.Sat;
using Projektseminar.Instance;
using System.Reflection.PortableExecutable;

namespace Projektseminar.ORToolsSolver
{
    internal class GoogleOR : Standalone.Observer
    {
        public GoogleOR(Problem currentProblem)
        {
            CurrentProblem = currentProblem;
            BestProblem = new Problem(CurrentProblem); //Hier ist wichtig das eine Kopie des aktuellen Projektes gemacht wird.
        }
        public Problem DoORSolver()
        {
            //Instanziiere CP-Modell
            CpModel model = new CpModel();

            //Iteriere durch alle Jobs
            foreach (Job job in CurrentProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    string suffix = $"_{job.Id}_{task.Id}"; //Suffix um Variablen im Modell zu Identifizieren

                    //Erstellen der Variablen für jeden Task
                    task.StartIntVar = model.NewIntVar(0, CurrentProblem.Horizon, "start" + suffix);
                    task.EndIntVar = model.NewIntVar(0, CurrentProblem.Horizon, "end" + suffix);
                    task.DurationIntVar = model.NewIntervalVar(task.StartIntVar, task.Duration, task.EndIntVar, "interval" + suffix);

                    CurrentProblem.Machines[task.Machine.Id].Schedule.Add(task); //Füge Tasks in beliebiger Reihenfolge der Schedule hinzu
                }
            }

            //Für jeden Task muss hinzugefügt werden, wenn Task1 auf Task2 dann so, wenn Task 2 auf Task 1 dann so
            foreach (var machine in CurrentProblem.Machines)
                foreach (var job1 in machine.Schedule)
                    foreach (var job2 in machine.Schedule)
                        if (job1 != job2) //Wenn beIde Jobs gleich sind müssen keine Setup-Zeiten bedacht werden
                        {
                            //Füge Constraints für die Setup-Zeiten hinzu
                            BoolVar logic_var = model.NewBoolVar("");
                            model.Add(job2.StartIntVar >= job1.EndIntVar + CurrentProblem.Setups[Tuple.Create(job1.Job.Id, job2.Job.Id)]).OnlyEnforceIf(logic_var); //Setup-Zeit Job 2 nach Job 1 wird bedacht wenn bool: true
                            model.Add(job1.StartIntVar >= job2.EndIntVar + CurrentProblem.Setups[Tuple.Create(job2.Job.Id, job1.Job.Id)]).OnlyEnforceIf(logic_var.Not()); //Setup-Zeit Job 1 nach Job 2 wird bedacht wenn bool: false
                        }

            //Constraints hinzufügen, sodass die Reihenfolge innerhalb eines Jobs eingehalten wird
            for (int jobId = 0; jobId < CurrentProblem.Jobs.Count; ++jobId)
            {
                var job = CurrentProblem.Jobs[jobId];
                for (int taskId = 0; taskId < job.Tasks.Count - 1; ++taskId)
                {
                    model.Add(CurrentProblem.Jobs[jobId].Tasks[taskId + 1].StartIntVar >= CurrentProblem.Jobs[jobId].Tasks[taskId].EndIntVar); //Constraint
                }
            }

            IntVar objVar = model.NewIntVar(0, CurrentProblem.Horizon, "makespan"); //Definiere Makespanvariable

            List<IntVar> ends = new List<IntVar>(); //Enumerable mit allen End-Variablen
            
            //Füge die End-Variable von jedem letzten Task in jedem Job zum Array hinzi
            for (int jobId = 0; jobId < CurrentProblem.Jobs.Count; ++jobId)
            {
                ends.Add(CurrentProblem.Jobs[jobId].Tasks[CurrentProblem.Jobs[jobId].Tasks.Count - 1].EndIntVar);
            }

            model.AddMaxEquality(objVar, ends); //Der Makespan kann nicht kleiner als der größte Endzeitpunkt aller letzten Tasks werden
            model.Minimize(objVar); //Zielfunktion ist die Minimierung des Makespans

            //Löse das Problem
            CpSolver solver = new CpSolver();
            solver.StringParameters = $"max_time_in_seconds:{MaxRuntimeInSeconds}.0"; //Maximale Laufzeit des Solvers
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            //Exportiere Problem als Diagramm
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine("Solution:");

                //Schreibe alle Werte des gelösten Problems in das beste Problem
                foreach (Job job in BestProblem.Jobs)
                {
                    foreach (Instance.Task task in job.Tasks)
                    {
                        int start = (int)solver.Value(CurrentProblem.Jobs[job.Id].Tasks[task.Id].StartIntVar);

                        task.Start = start;
                        task.End = task.Start + task.Duration;

                        BestProblem.Machines[task.Machine.Id].Schedule.Add(task);
                    }
                }

                //Sortiere alle Maschinen
                foreach (var machine in BestProblem.Machines)
                {
                    // Sort by starting time.
                    machine.Schedule.Sort();
                }

                BestProblem.SetRelatedTasks();
                BestProblem.Recalculate();
            }
            return BestProblem;
        }
    }
}
