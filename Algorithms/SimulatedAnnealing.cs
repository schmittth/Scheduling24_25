using Projektseminar.Instance;

namespace Projektseminar.Algorithms
{
    internal class SimulatedAnnealing : Standalone.Observer
    {
        //Eigenschaften
        public double Temperature { get; set; }
        public double CoolingFactor { get; set; }
        public int Iterations { get; set; }
        public string Neighborhood { get; set; }

        //Variablen

        //Konstruktoren
        public SimulatedAnnealing(Problem problem, double coolingFactor, int iterations, string neighboorhood)
        {
            CurrentProblem = problem; //Übergebenes Problem wird als aktuelles Problem gesetzt
            BestProblem = problem; //Bei Instanzierung ist bestes Problem = aktuelles Problem
            Temperature = 100; //Temperatur hardcoded auf 100
            CoolingFactor = coolingFactor; //Abkühlungfaktor wird von Konsole übergeben.
            Iterations = iterations; //Anzahl an Iterationen wird von Konsole übergeben.
            Neighborhood = neighboorhood; //Nachbarschaft wird von Konsole übergeben.
        }


        //Methoden
        public Problem DoSimulatedAnnealing(int seedValue)
        {
            //CalculateTemperature(GenerateSamples());

            Random random = new Random(seedValue); //Initialisiere Zufallswert
            Problem newProblem; //Deklariere Variable für neues Problem

            while (Temperature > 1 && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds)
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature} with {Iterations} Iterations planned");

                //Iteriere über die Anzahl an Iterationen
                for (int i = 0; i < Iterations && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds; i++)
                {
                    List<List<Tuple<Instance.Task, Instance.Task>>> neighborhoodOperations = CurrentProblem.GetNeighbors(Neighborhood); //Instanziiere Dict mit Nachbarschaften

                    //Iteriere bis eine nicht-zyklische Lösung gefunden wurde
                    do
                    {
                        newProblem = new Problem(CurrentProblem); //Kopiere aktuelles Problem

                        int chooseNeighbor = random.Next(0, neighborhoodOperations.Count); //Wähle Zufallszahl zwischen 0 und allen Nachbarschaften

                        //Führe alle Tauschschritte aus
                        foreach (var tuple in neighborhoodOperations[chooseNeighbor])
                        {
                            newProblem.SwapTasks(tuple.Item1, tuple.Item2);
                        }

                        neighborhoodOperations.RemoveAt(chooseNeighbor);
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
