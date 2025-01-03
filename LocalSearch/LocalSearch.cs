﻿using Projektseminar.Instance;

namespace Projektseminar.LocalSearch
{
    internal class LocalSearch : Solver
    {
        public string Neighborhood { get; set; }

        public LocalSearch(Problem problem, string neighborhood)
        {
            BestProblem = problem;
            CurrentProblem = problem;
            Neighborhood = neighborhood;
        }

        public Problem DoLocalSearch()
        {
            //Iteriere bis keine Verbesserung mehr gefunden oder Zeit abgelaufen
            while(Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds)
            {
                Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood(Neighborhood); //Erlange alle Nachbarschaften

                //Führe alle Nachbarschaften aus
                foreach (List<Tuple<Instance.Task, Instance.Task, Machine>> list in dict.Values)
                {
                    Problem newProblem = new Problem(CurrentProblem); //Kopiere aktuelles Problem

                    //Führe alle Tauschschritte aus
                    foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                    }

                    //Wenn Makespan des neuen Problems besser als Makespan des aktuellen Problems wechsle zu neuem Problem 
                    if (newProblem.Makespan < CurrentProblem.Makespan)
                    {
                        //Bestätige das Problem nicht zyklisch ist
                        if (newProblem.CheckCyclicity())
                        {
                            CurrentProblem = newProblem;
                        }
                    }
                }

                //Vergleiche ob sich der Makespan verbessert hat
                if (BestProblem.Makespan <= CurrentProblem.Makespan)
                {
                    break; //Wenn sich der Makespan nicht verbessert hat, beende die lokale Suche
                }
                else
                {
                    BestProblem = CurrentProblem; //Wenn sich der Makespan verbessert hat wechsle zu besserem Problem
                }
            }

            return BestProblem;
        }

    }
}
