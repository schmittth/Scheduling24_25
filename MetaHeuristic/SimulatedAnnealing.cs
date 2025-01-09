using Projektseminar.Instance;
using System.ComponentModel.Design;

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

            while (Temperature > 1 && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds) 
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature}");
                //CurrentProblem = BestProblem; //Alternative mit bestem Problem weitermachen

                //Iteriere über die Anzahl an Iterationen
                for (int i = 0; i < Iterations && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds; i++)
                {
                        Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood(Neighboorhood); //Instanziiere Dict mit Nachbarschaften
                        List<int> cyclicNeighbors = new List<int>(); //Erstelle Liste mit zkylischen Nachbarschaften

                        //Iteriere bis eine nicht-zyklische Lösung gefunden wurde
                        do
                        {
                            newProblem = new Problem(CurrentProblem); //Kopiere aktuelles Problem
                            int chooseNeighbor;

                            //Iteriere solange bis ein nicht-zyklischer Nachbar gewählt wurde
                            do
                            {
                                chooseNeighbor = random.Next(0, dict.Count); //Wähle Zufallszahl zwischen 0 und allen Nachbarschaften
                                /*if (cyclicNeighbors.Count > 1)
                                {
                                    string cyclicString = cyclicNeighbors.ToString();
                                    Console.WriteLine($"Invalids: {cyclicString}");
                                }*/
                            }
                            while (cyclicNeighbors.Contains(chooseNeighbor));
                            cyclicNeighbors.Add(chooseNeighbor); //Füge ausgewählte Nachbarschaft, Liste hinzu

                            //Führe alle Tauschschritte aus
                            foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in dict[chooseNeighbor])
                            {
                                newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                            }
                        }
                        while (!newProblem.CheckCyclicity());

                        //Wenn neuer Makespan besser als aktueller, ersetze aktuelles Problem
                        if (CurrentProblem.Makespan > newProblem.Makespan)
                        {
                            //Wenn neuer Makespan besser als bester, ersetze bestes Problem
                            if (BestProblem.Makespan > newProblem.Makespan)
                            {
                                BestProblem = newProblem;
                            }
                            CurrentProblem = newProblem;
                        }
                        else
                        {
                            int delta = newProblem.Makespan - CurrentProblem.Makespan; //Berechne ganzzahlige Differenz zwischen aktuellem und neuem Makespan als Delta
                            double exponent = -delta / Temperature; //Berechne Quotient aus Delta und aktueller Temperatur als Gleitkommazahl
                            double probability = Math.Pow(Math.E, exponent); //Berechne Wahrscheinlichkeit aus eulersche Konstante hoch berechnetem Exponent

                            //Akzeptiere geringeren Makespan mit berechneter Zufallswahrscheinlichkeot
                            if (random.NextDouble() < probability)
                            {
                                CurrentProblem = newProblem;
                            }
                        }                 
                }
                Temperature = Temperature * CoolingFactor; //Reduziere Temperatur entsprechend des Abkühlungsfaktors
            }
            return BestProblem;
        }
    }
}
