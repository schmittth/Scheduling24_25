using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Google.OrTools.Sat;
using JobShopSchedulingProblemCP;
using JobShopSchedulingProblemCP.Instance;

namespace JobShopSchedulingProblemCP.ORToolsSolver
{
    internal class GoogleOR
    {
        public void SolveProblem(Instance.Problem instance)
        {
            int numMachines = 0;
            int horizon = 0;

            foreach (Instance.Job job in instance.Jobs)
            {
                foreach (JobShopSchedulingProblemCP.Instance.Task task in job.Tasks)
                {
                    numMachines = Math.Max(numMachines, 1 + task.Machine.Id);
                    horizon += task.Duration;
                }
            }

            int[] allMachines = Enumerable.Range(0, numMachines).ToArray();

            /*int[,] setupTimes = {
                // To:  0  1  2  3  4  5  6  7  8  9
                { 0, 2, 3, 4, 1, 2, 5, 3, 4, 2 }, // From Job 0
                { 4, 0, 1, 3, 2, 4, 3, 5, 1, 3 }, // From Job 1
                { 3, 2, 0, 1, 5, 2, 4, 2, 3, 4 }, // From Job 2
                { 2, 3, 1, 0, 4, 5, 2, 3, 4, 3 }, // From Job 3
                { 1, 2, 5, 3, 0, 1, 3, 2, 5, 4 }, // From Job 4
                { 2, 4, 3, 5, 1, 0, 2, 4, 1, 3 }, // From Job 5
                { 5, 3, 4, 2, 3, 2, 0, 1, 2, 5 }, // From Job 6
                { 3, 5, 2, 3, 2, 4, 1, 0, 4, 3 }, // From Job 7
                { 4, 1, 3, 4, 5, 1, 2, 4, 0, 2 }, // From Job 8
                { 2, 3, 4, 3, 4, 3, 5, 3, 2, 0 }  // From Job 9
            };*/


            // Creates the model.
            CpModel model = new CpModel();

            Dictionary<Tuple<int, int>, Tuple<int, IntVar, IntVar, IntervalVar>> allTasks = new Dictionary<Tuple<int, int>, Tuple<int, IntVar, IntVar, IntervalVar>>(); // (start, end, duration)
            Dictionary<int, List<Tuple<int, IntVar, IntVar, IntervalVar>>> machineToIntervals = new Dictionary<int, List<Tuple<int, IntVar, IntVar, IntervalVar>>>();


                    foreach (Instance.Job job in instance.Jobs)
                    {
                        foreach (Instance.Task task in job.Tasks)
                        {
                            String suffix = $"_{job.Id}_{task.Id}";
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
                                        model.Add(job_u.Item2 >= job_j.Item3 + instance.Setups[Tuple.Create(job_j.Item1, job_u.Item1)]).OnlyEnforceIf(logic_var);
                                        model.Add(job_j.Item2 >= job_u.Item3 + instance.Setups[Tuple.Create(job_u.Item1, job_j.Item1)]).OnlyEnforceIf(logic_var.Not());
                                }
                


                    // Precedences inside a job.
                    for (int jobID = 0; jobID < instance.Jobs.Count; ++jobID)
                    {
                        var job = instance.Jobs[jobID];
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
                    for (int jobID = 0; jobID < instance.Jobs.Count; ++jobID)
                    {
                        var job = instance.Jobs[jobID];
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

                        Dictionary<int, List<AssignedTask>> assignedJobs = new Dictionary<int, List<AssignedTask>>();
                        foreach (Instance.Job job in instance.Jobs)
                        {
                            foreach (Instance.Task task in job.Tasks)
                            {
                                var key = Tuple.Create(job.Id, task.Id);
                                int start = (int)solver.Value(allTasks[key].Item2);
                                if (!assignedJobs.ContainsKey(task.Machine.Id))
                                {
                                    assignedJobs.Add(task.Machine.Id, new List<AssignedTask>());
                                }
                                assignedJobs[task.Machine.Id].Add(new AssignedTask(job.Id, task.Id, start, task.Duration));
                            }
                        }

                        // Create per machine output lines.
                        String output = "";
                        foreach (int machine in allMachines)
                        {
                            // Sort by starting time.
                            assignedJobs[machine].Sort();
                            String solLineTasks = $"Machine {machine}: ";
                            String solLine = "           ";

                            int previousJob = -1;

                            foreach (var assignedTask in assignedJobs[machine])
                            {

                                int taskEnd = assignedTask.start + assignedTask.duration;

                                if (previousJob != -1)
                                {
                                    String setupTimeName = $"setup_{previousJob}_{assignedTask.jobID}";
                                    solLineTasks += $"{setupTimeName,-15}";

                                    String setupTime = $"[{assignedTask.start - instance.Setups[Tuple.Create(previousJob, assignedTask.jobID)]},{assignedTask.start}]";
                                    solLine += $"{setupTime,-15}";
                                }
                                previousJob = assignedTask.jobID;

                                String name = $"job_{assignedTask.jobID}_task_{assignedTask.taskID}";
                                // Add spaces to output to align columns.
                                solLineTasks += $"{name,-15}";


                                String solTmp = $"[{assignedTask.start},{taskEnd}]";
                                // Add spaces to output to align columns.
                                solLine += $"{solTmp,-15}";


                            }
                            output += solLineTasks + "\n";
                            output += solLine + "\n";
                        }
                        // Finally print the solution found.
                        Console.WriteLine($"Optimal Schedule Length: {solver.ObjectiveValue}");
                        Console.WriteLine($"\n{output}");
                    }
                    else
                    {
                        Console.WriteLine("No solution found.");
                    }

                    Console.WriteLine("Statistics");
                    Console.WriteLine($"  conflicts: {solver.NumConflicts()}");
                    Console.WriteLine($"  branches : {solver.NumBranches()}");
                    Console.WriteLine($"  wall time: {solver.WallTime()}s");

                }
            }
        }
 