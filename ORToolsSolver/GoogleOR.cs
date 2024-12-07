using Google.OrTools.Sat;
using Projektseminar.Instance;

namespace Projektseminar.ORToolsSolver
{
    internal class GoogleOR
    {
        public Problem CurrentProblem { get; set; }

        public GoogleOR(Problem currentProblem)
        {
            CurrentProblem = currentProblem;
        }

        public void DoORSolver()
        {
            int numMachines = 0;
            int horizon = 0;

            foreach (Job job in CurrentProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    numMachines = Math.Max(numMachines, 1 + task.Machine.Id);
                    horizon += task.Duration;
                }
            }

            int[] allMachines = Enumerable.Range(0, numMachines).ToArray();

            // Creates the model.
            CpModel model = new CpModel();

            Dictionary<Tuple<int, int>, Tuple<int, IntVar, IntVar, IntervalVar>> allTasks = new Dictionary<Tuple<int, int>, Tuple<int, IntVar, IntVar, IntervalVar>>(); // (start, end, duration)
            Dictionary<int, List<Tuple<int, IntVar, IntVar, IntervalVar>>> machineToIntervals = new Dictionary<int, List<Tuple<int, IntVar, IntVar, IntervalVar>>>();


            foreach (Job job in CurrentProblem.Jobs)
            {
                foreach (Instance.Task task in job.Tasks)
                {
                    string suffix = $"_{job.Id}_{task.Id}";
                    IntVar start = model.NewIntVar(0, horizon, "start" + suffix);
                    IntVar end = model.NewIntVar(0, horizon, "end" + suffix);
                    IntervalVar interval = model.NewIntervalVar(start, task.Duration, end, "interval" + suffix);
                    var key = Tuple.Create(job.Id, task.Id);
                    var taskTuple = Tuple.Create(job.Id, start, end, interval);
                    allTasks[key] = taskTuple;
                    if (!machineToIntervals.ContainsKey(task.Machine.Id))
                    {
                        machineToIntervals.Add(task.Machine.Id, new List<Tuple<int, IntVar, IntVar, IntervalVar>>());
                    }
                    machineToIntervals[task.Machine.Id].Add(taskTuple);

                }
            }


            foreach (int machine in allMachines)
                foreach (var job_j in machineToIntervals[machine])
                    foreach (var job_u in machineToIntervals[machine])
                        if (job_j != job_u)
                        {
                            BoolVar logic_var = model.NewBoolVar("");
                            model.Add(job_u.Item2 >= job_j.Item3 + CurrentProblem.Setups[Tuple.Create(job_j.Item1, job_u.Item1)]).OnlyEnforceIf(logic_var);
                            model.Add(job_j.Item2 >= job_u.Item3 + CurrentProblem.Setups[Tuple.Create(job_u.Item1, job_j.Item1)]).OnlyEnforceIf(logic_var.Not());
                        }



            // Precedences inside a job.
            for (int jobID = 0; jobID < CurrentProblem.Jobs.Count; ++jobID)
            {
                var job = CurrentProblem.Jobs[jobID];
                for (int taskID = 0; taskID < job.Tasks.Count - 1; ++taskID)
                {
                    var key = Tuple.Create(jobID, taskID);
                    var nextKey = Tuple.Create(jobID, taskID + 1);
                    model.Add(allTasks[nextKey].Item2 >= allTasks[key].Item3);
                }
            }

            // Makespan objective.
            IntVar objVar = model.NewIntVar(0, horizon, "makespan");

            List<IntVar> ends = new List<IntVar>();
            for (int jobID = 0; jobID < CurrentProblem.Jobs.Count; ++jobID)
            {
                var job = CurrentProblem.Jobs[jobID];
                var key = Tuple.Create(jobID, job.Tasks.Count - 1);
                ends.Add(allTasks[key].Item3);
            }
            model.AddMaxEquality(objVar, ends);
            model.Minimize(objVar);

            // Solve
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine("Solution:");
                
                Dictionary<int, List<Instance.Task>> assignedJobs = new Dictionary<int, List<Instance.Task>>();
                foreach (Job job in CurrentProblem.Jobs)
                {
                    foreach (Instance.Task task in job.Tasks)
                    {      
                        var key = Tuple.Create(job.Id, task.Id);
                        int start = (int)solver.Value(allTasks[key].Item2);

                        task.Start = start;
                        task.End = task.Start + task.Duration;

                        CurrentProblem.Machines[task.Machine.Id].Schedule.Add(task);
                    }
                }
                    foreach (Machine machine in CurrentProblem.Machines)
                    {
                        // Sort by starting time.
                        machine.Schedule.Sort();
                    }

                    CurrentProblem.SetRelatedTasks();
                    CurrentProblem.CalculateSetups();

                    CurrentProblem.ProblemAsDiagramm(@"..\Or.html");

            }
        }

        public void Log(string instanceName, int seedValue)
        {

            int minTaskAmount = 0;
            int minTaskTime = 0;
            int maxTaskTime = 0;

            foreach (Job job in CurrentProblem.Jobs)
            {
                if (minTaskAmount > job.Tasks.Count)
                {
                    minTaskAmount = job.Tasks.Count;
                }

                foreach (Instance.Task task in job.Tasks)
                {
                    if (task.Duration < minTaskTime)
                    {
                        minTaskTime = task.Duration;
                    }
                    if (task.Duration > maxTaskTime)
                    {
                        maxTaskTime = task.Duration;
                    }

                }
            }

            using (StreamWriter sw = File.AppendText((@$"..\..\..\LogFile.csv")))
            {
                sw.WriteLine($"{instanceName};{CurrentProblem.Jobs.Count};{CurrentProblem.Machines.Count};{minTaskAmount};{minTaskTime};{maxTaskTime};GoogleOR;;;;{seedValue}"); 
            }            
        }
    }
}
