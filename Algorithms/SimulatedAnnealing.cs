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
        public List<Tuple<int, int>> GenerateSamples()
        {
            int transitionAmount = 10000;

            Queue<Problem> possibleSuccessors = new Queue<Problem>();
            List<Tuple<int, int>> transitions = new List<Tuple<int, int>>();
            Problem initProblem = new Problem(CurrentProblem);

            possibleSuccessors.Enqueue(initProblem);

            //Deklariere Variable für neues Problem
            while (transitions.Count < transitionAmount)
            {
                Problem newProblem = possibleSuccessors.Dequeue();

                List<List<Tuple<Instance.Task, Instance.Task>>> neighborhoodOperations = newProblem.GetNeighbors(Neighborhood);

                foreach (var list in neighborhoodOperations)
                {
                    Problem copiedProblem = new Problem(newProblem);

                    foreach (var tuple in list)
                    {
                        copiedProblem.SwapTasks(tuple.Item1, tuple.Item2);
                    }

                    if (copiedProblem.CheckCyclicity() == false)
                    {
                        continue;
                    }

                    if (copiedProblem.Makespan < newProblem.Makespan)
                    {
                        transitions.Add(Tuple.Create(newProblem.Makespan - copiedProblem.Makespan, neighborhoodOperations.Count));
                        possibleSuccessors.Enqueue(copiedProblem);
                    }
                }
            }
            return transitions;
        }

        public void CalculateTemperature(List<Tuple<int, int>> samples)
        {

            double desiredAcceptanceProb = 0.8;
            double tolerance = 1e-5;
            double p = 1.0;

            double initTemp = 1;

            while (true)
            {
                double num = 0;
                double denom = 0;

                foreach (var transition in samples)
                {
                    double weight = 1.0 / transition.Item2;
                    num += weight * Math.Exp(-transition.Item1 / initTemp);
                    denom += weight;
                }

                if (Math.Abs((num / denom) - desiredAcceptanceProb) < tolerance)
                {
                    break;
                }

                initTemp = initTemp * Math.Pow(Math.Log((num / denom)) / Math.Log(desiredAcceptanceProb), 1.0 / p);
            }

            Temperature = initTemp;
        }

        /*public void CalculateCoolingFactor(List<Tuple<int, int>> samples)
        {           
            List<int> intList = samples.Select(t => t.Item1).ToList();

            double meanE = intList.Average(); // Mittelwert der Energie
            double meanE2 = intList.Select(e => e * e).Average(); // Mittelwert von E^2
            return (meanE2 - meanE * meanE) / (Temperature * Temperature);
        }*/

        public Problem DoSimulatedAnnealing(int seedValue)
        {
            CalculateTemperature(GenerateSamples());

            Random random = new Random(seedValue); //Initialisiere Zufallswert
            Problem newProblem; //Deklariere Variable für neues Problem

            while (Temperature > 5 && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds)
            //while (Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds)
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature} with {Iterations} Iterations planned");
                //CurrentProblem = BestProblem; //Alternative mit bestem Problem weitermachen

                //Iteriere über die Anzahl an Iterationen
                for (int i = 0; i < Iterations && Stopwatch.Elapsed.TotalSeconds < MaxRuntimeInSeconds; i++)
                {
                    List<List<Tuple<Instance.Task, Instance.Task>>> neighborhoodOperations = CurrentProblem.GetNeighbors(Neighborhood); //Instanziiere Dict mit Nachbarschaften
                    List<int> cyclicNeighbors = new List<int>(); //Erstelle Liste mit zkylischen Nachbarschaften

                    //Iteriere bis eine nicht-zyklische Lösung gefunden wurde
                    do
                    {
                        newProblem = new Problem(CurrentProblem); //Kopiere aktuelles Problem


                        int chooseNeighbor = random.Next(0, neighborhoodOperations.Count); //Wähle Zufallszahl zwischen 0 und allen Nachbarschaften
                        /*if (cyclicNeighbors.Count > 1)
                        {
                            string cyclicString = cyclicNeighbors.ToString();
                            Console.WriteLine($"Invalids: {cyclicString}");
                        }*/


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
