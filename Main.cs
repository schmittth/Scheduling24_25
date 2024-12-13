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
            int seedChoice = 0;

            int instanceAmount = Dialog.ChooseInstanceAmount(); //Bestimme wie oft Instanzen generiert werden sollen
            string instanceChoice = Dialog.ChooseInstance(); //Bestimme ob randomisierte oder feste Instanz

            //Wenn Anzahl an Instanzen gleich 1 lasse Seed auswählen
            if (instanceAmount == 1)
            {
                seedChoice = Dialog.SeedAlgorithm();
            }

            //Wenn randomisierte Instanz lasse Instanzgröße auswählen
            Tuple<int, int, int, int, int> randomInstanceSize = null;
            if (instanceChoice == "Random")
            {
                randomInstanceSize = Dialog.ChooseRandomInstanceSize();

            }

            int solverChoice = Dialog.ChooseSolver(); //Lasse Lösungsansatz auswählen
            string priorityRule = ""; //Initialisiere PriortityRule String
            string neighboorhood = ""; //Initialisiere Nachbarschafts String


            if (solverChoice == 2 || solverChoice == 3)
            {
                priorityRule = Dialog.ChoosePriorityRule();
                neighboorhood = Dialog.ChooseNeighboorhood();
            }

            Tuple<double, int> simAnnealParams = null;
            if (solverChoice == 2)
            {
                simAnnealParams = Dialog.ChooseSimAnnealParameters();
            }

            for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
            {
                if (seedChoice == 0)
                {
                    Random randSeed = new Random();
                    seedValue = randSeed.Next(0, Int32.MaxValue);
                }
                else
                {
                    seedValue = seedChoice;
                }

                Importer importer = new Importer();
                if (instanceChoice == "Random")
                {
                    importer.ImportRandomInstance(randomInstanceSize.Item1, randomInstanceSize.Item2, seedValue, randomInstanceSize.Item3, randomInstanceSize.Item4, randomInstanceSize.Item5);
                }
                else
                {
                    importer.ImportInstanceFromFile(instanceChoice);
                }
                Problem problem = importer.GenerateProblem();
                stopwatch.Start();
                GifflerThompson gifflerThompson = new GifflerThompson(problem, priorityRule);
                stopwatch.Stop();
                if (solverChoice == 2 || solverChoice == 3)
                {
                    problem = gifflerThompson.InitialSolution();
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}{instanceCounter}\diagrammInitial.html", false, seedValue, stopwatch.Elapsed);
                }
                stopwatch.Reset();

                switch (solverChoice) //Switch-Case Anweisungen basierend auf der Solver-Auswahl.
                {
                    //Solver: Google OR-Tools
                    case 1:

                        stopwatch.Start();

                        //Erstelle neues OR-Solver Objekt und löse das Problem.
                        ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR(problem);
                        newSolver.DoORSolver();

                        stopwatch.Stop();

                        newSolver.Log(instanceChoice, seedValue, stopwatch.Elapsed, "GoogleOR"); //Logge die Ausführung

                        break;
                    //Solver: Simulated Annealing
                    case 2:

                        stopwatch.Start();
                        SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, neighboorhood);

                        problem = simAnneal.DoSimulatedAnnealing(seedValue);

                        stopwatch.Stop();

                        simAnneal.Log(instanceChoice, seedValue, stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighboorhood, gifflerThompson.PriorityRule);

                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}{instanceCounter}\diagramm.html", true, seedValue, stopwatch.Elapsed);

                        break;
                    case 3:
                        LocalSearch.LocalSearch localSearch = new LocalSearch.LocalSearch(problem, neighboorhood);
                        problem = localSearch.DoLocalSearch();

                        stopwatch.Stop();

                        localSearch.Log(instanceChoice, seedValue, stopwatch.Elapsed, "Local Search", iterations: 0, priorityRule: gifflerThompson.PriorityRule);

                        break;
                }
            }
            stopwatch.Stop();
        }
    }
}