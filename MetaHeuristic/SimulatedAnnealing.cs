using Projektseminar.Instance;

namespace Projektseminar.MetaHeuristic
{
    internal class SimulatedAnnealing : Solver
    {
        //Eigenschaften
        public double Temperature { get; set; }
        public double CoolingFactor { get; set; }
        public int Iterations { get; set; }
        public string Neighboorhood { get; set; }

        //Variablen

        //Konstruktoren
        public SimulatedAnnealing(Problem problem, double coolingFactor, int iterations, string neighboorhood)
        {
            this.CurrentProblem = problem; //Übergebenes Problem wird als aktuelles Problem gesetzt
            this.BestProblem = problem; //Bei Instanzierung ist bestes Problem = aktuelles Problem
            this.Temperature = 100; //Temperatur hardcoded auf 100
            this.CoolingFactor = coolingFactor; //Abkühlungfaktor wird von Konsole übergeben.
            this.Iterations = iterations; //Anzahl an Iterationen wird von Konsole übergeben.
            this.Neighboorhood = neighboorhood; //Nachbarschaft wird von Konsole übergeben.
        }

        //Methoden
        public Problem DoSimulatedAnnealing(int seedValue)
        {
            Random random = new Random(seedValue); //Initialisiere Zufallswert
            Problem newProblem; //Deklariere Variable für neues Problem

            while (Temperature > 1)
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature}");
                //CurrentProblem = BestProblem; //Alternative mit bestem Problem weitermachen

                for (int i = 0; i < Iterations; i++)
                {
                    Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood(Neighboorhood);

                    newProblem = new Problem(CurrentProblem);
                    List<Tuple<Instance.Task, Instance.Task, Machine>> randomNeighbor = dict[random.Next(0, dict.Count)];

                    foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in randomNeighbor)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                    }

                    if (CurrentProblem.Makespan > newProblem.Makespan)
                    {
                        if (BestProblem.Makespan > newProblem.Makespan)
                        {
                            BestProblem = newProblem;
                        }
                        CurrentProblem = newProblem;
                    }
                    else
                    {
                        int delta = newProblem.Makespan - CurrentProblem.Makespan;
                        double exponent = -delta / Temperature;
                        double probability = Math.Pow(Math.E, exponent);
                        if (random.NextDouble() < probability)
                        {
                            CurrentProblem = newProblem;
                        }
                    }
                }
                Temperature = Temperature * CoolingFactor;
            }
            return BestProblem;
        }
    }
}
