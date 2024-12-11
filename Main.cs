using Projektseminar.Instance;
using Projektseminar.LocalSearch;
using Projektseminar.MetaHeuristic;
using Projektseminar.OpeningHeuristic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen

        //Methoden
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch(); //Initialisiere eine Stopwatch um die Laufzeit zu messen.
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            //Initialisiere neues Importer-Objekt, dass über den Dialog die zu importierende Instanz erhält.
            Importer importer = new Importer();
            string instanceChoice = Dialog.ChooseInstance();
            if (instanceChoice == "Random")
            {
                Tuple<int, int, int, int, int> randomSize = Dialog.ChooseRandomInstanceSize();
                seedValue = Dialog.SeedAlgorithm();
                importer.ImportRandomInstance(randomSize.Item1, randomSize.Item2, seedValue, randomSize.Item3, randomSize.Item4, randomSize.Item5);
            }
            else
            {
                importer.ImportInstanceFromFile(instanceChoice);
            }

            Problem problem = importer.GenerateProblem(); //Erstelle aus importierter Instanz ein Problem.

            switch (Dialog.ChooseSolver()) //Switch-Case Anweisungen basierend auf der Solver-Auswahl.
            {
                //Solver: Google OR-Tools
                case 1:
                    stopwatch.Start();

                    //Erstelle neues OR-Solver Objekt und löse das Problem.
                    ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR(problem);
                    newSolver.DoORSolver();

                    stopwatch.Stop();

                    newSolver.Log(instanceChoice, seedValue, stopwatch.Elapsed); //Logge die Ausführung

                    break;
                //Solver: Simulated Annealing
                case 2:
                    Giffler_Thompson giffler_Thompson = new Giffler_Thompson(problem, Dialog.ChoosePriorityRule());
                    stopwatch.Start();
                    problem = giffler_Thompson.InitialSolution();

                    stopwatch.Stop();

                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\diagrammInitial.html", false);

                    Tuple<double, int> simAnnealParams = Dialog.ChooseSimAnnealParameters();

                    SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, Dialog.ChooseNeighboorhood());
                    
                    if (seedValue != 0)
                    {
                        stopwatch.Start();
                        problem = simAnneal.DoSimulatedAnnealing(seedValue);
                    }
                    else
                    {
                        seedValue = Dialog.SeedAlgorithm();
                        stopwatch.Start();
                        problem = simAnneal.DoSimulatedAnnealing(seedValue);
                    }

                    stopwatch.Stop();

                    simAnneal.Log(instanceChoice, seedValue, stopwatch.Elapsed, giffler_Thompson.PriorityRule);

                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\diagramm.html", true);

                    break;
                case 3:
                    stopwatch.Start();

                    Giffler_Thompson localgiffler_Thompson = new Giffler_Thompson(problem, Dialog.ChoosePriorityRule());
                    problem = localgiffler_Thompson.InitialSolution();
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\diagrammInitial.html", false);

                    LocalSearch.LocalSearch localSearch = new LocalSearch.LocalSearch(problem, Dialog.ChooseNeighboorhood());
                    problem = localSearch.DoLocalSearch();

                    stopwatch.Stop();

                    localSearch.Log(instanceChoice, seedValue, stopwatch.Elapsed, localgiffler_Thompson.PriorityRule);

                    break;
            }

            stopwatch.Stop();
        }
    }
}