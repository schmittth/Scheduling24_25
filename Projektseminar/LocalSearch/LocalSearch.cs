using JobShopSchedulingProblemCP.Instance;
using OperationsResearch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP.LocalSearch
{
    internal class LocalSearch
    {              
        public Problem DoLocalSearch(bool parallelMode, Problem problem, string searchMethod)
        {
            ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = problem.GetNeighboorhood(searchMethod);

            int makespan = problem.CalculateMakespan();
            Problem returnProblem = problem;

            switch (parallelMode)
            {
                case true:
                    ConcurrentDictionary<int, Tuple<Instance.Problem, int>> conDict = new ConcurrentDictionary<int, Tuple<Instance.Problem, int>>();

                    conDict.TryAdd(0, Tuple.Create(problem, makespan));

                    Parallel.ForEach(dict.Values, list =>
                    {
                        Problem newProblem = new Problem(problem);

                        foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                        {
                            newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                            conDict.TryAdd(conDict.Last().Key + 1, Tuple.Create(newProblem, newProblem.CalculateMakespan()));

                        }
                    });
                    return conDict.First(x => x.Value.Item2 == conDict.Min(x => x.Value.Item2)).Value.Item1;

                case false:
                    foreach (List<Tuple<Instance.Task, Instance.Task, Machine>> list in dict.Values)
                    {
                        Problem newProblem = new Problem(problem);

                        foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                        {
                            newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                        }

                        int newMakespan = newProblem.CalculateMakespan();

                        if (newMakespan < makespan)
                        {
                            returnProblem = newProblem;
                            makespan = newMakespan;
                        }
                    }
                    return returnProblem;
                default:
                    break;
            }
        }
    }
}
