using Projektseminar.Instance;

namespace Projektseminar.Algorithms
{
    internal class LocalSearch : Standalone.Observer
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
            while (Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds)
            {
                List<List<Tuple<Instance.Task, Instance.Task>>> neighborhoodOperations = CurrentProblem.GetNeighbors(Neighborhood); //Erlange alle Nachbarschaften
                Problem currentBestProblem = CurrentProblem; //Speichere bestes Problem abhängig vom aktuellen Problem, am Anfang aktuelles Problem

                //Führe alle Nachbarschaften aus
                foreach (List<Tuple<Instance.Task, Instance.Task>> swapSteps in neighborhoodOperations)
                {
                    Problem newProblem = new Problem(CurrentProblem); //Kopiere aktuelles Problem

                    //Führe alle Tauschschritte auf kopiertem Problem  aus
                    foreach (Tuple<Instance.Task, Instance.Task> tuple in swapSteps)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2);
                    }

                    //Wenn Makespan des neuen Problems besser als Makespan des aktuell besten Problems wechsle zu neuem Problem 
                    if (newProblem.Makespan < currentBestProblem.Makespan)
                    {
                        //Bestätige das Problem nicht zyklisch ist
                        if (!newProblem.IsCyclic())
                        {
                            currentBestProblem = newProblem; //Setze aktuell bestes Problem auf neues Problem
                        }
                    } 
                }

                //Vergleiche ob sich der Makespan verbessert hat
                if (BestProblem.Makespan <= currentBestProblem.Makespan)
                {
                    break; //Wenn sich der Makespan nicht verbessert hat, beende die lokale Suche
                }
                else
                {
                    CurrentProblem = currentBestProblem;
                    BestProblem = currentBestProblem; //Wenn sich der Makespan verbessert hat wechsle zu besserem Problem
                }
            }

            return BestProblem;
        }

    }
}
