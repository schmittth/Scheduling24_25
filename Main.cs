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
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            foreach (string subDir in Directory.GetDirectories("../../../Diagramms/", "ClassroomInstance*"))
            {
                int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.

                List<string> subFiles = new List<string>();
                int instanceAmount = 0;

                subFiles = Directory.GetFiles(subDir, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                instanceAmount = subFiles.Count;

                int solverChoice = 2;
                string priorityRule = ""; //Initialisiere PriortityRule String
                string neighboorhood = ""; //Initialisiere Nachbarschafts String


                if (solverChoice == 2 || solverChoice == 3)
                {
                    priorityRule = "LTT";
                    neighboorhood = "N3";
                }

                Tuple<double, int> simAnnealParams = null;
                if (solverChoice == 2)
                {
                    simAnnealParams = Tuple.Create(0.99, 5000);
                }

                //
                for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
                {

                    Random randSeed = new Random();
                    seedValue = randSeed.Next(0, Int32.MaxValue);

                    Importer importer = new Importer();
                    importer.ImportInstanceFromFile(subFiles[instanceCounter]);

                    Problem problem = importer.GenerateProblem();

                    GifflerThompson gifflerThompson = new GifflerThompson(problem, priorityRule);

                    if (solverChoice == 2 || solverChoice == 3)
                    {
                        problem = gifflerThompson.InitialSolution();
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, gifflerThompson.Stopwatch.Elapsed);
                    }

                    switch (solverChoice) //Switch-Case Anweisungen basierend auf der Solver-Auswahl.
                    {
                        //Solver: Google OR-Tools
                        case 1:
                            //Erstelle neues OR-Solver Objekt und löse das Problem.
                            ORToolsSolver.GoogleOR googleor = new ORToolsSolver.GoogleOR(problem);
                            problem = googleor.DoORSolver();

                            //googleor.Log(instanceChoice, seedValue, googleor.Stopwatch.Elapsed, "GoogleOR"); //Logge die Ausführung
                            problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\googleOr.html", true, seedValue, googleor.Stopwatch.Elapsed);

                            break;
                        //Solver: Simulated Annealing
                        case 2:
                            SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, neighboorhood);
                            problem = simAnneal.DoSimulatedAnnealing(seedValue);

                            simAnneal.Log(subFiles[instanceCounter], seedValue, simAnneal.Stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighboorhood, gifflerThompson.PriorityRule);
                            problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\simAnneal.html", false, seedValue, simAnneal.Stopwatch.Elapsed);
                            //problem.ProblemAsFile($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\instanceExport.txt");
                            break;
                        case 3:
                            LocalSearch.LocalSearch localSearch = new LocalSearch.LocalSearch(problem, neighboorhood);
                            problem = localSearch.DoLocalSearch();

                            //localSearch.Log(instanceChoice, seedValue, localSearch.Stopwatch.Elapsed, "Local Search", iterations: 0, priorityRule: gifflerThompson.PriorityRule);
                            problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\localSearch.html", true, seedValue, localSearch.Stopwatch.Elapsed);

                            break;
                    }
                }
            }


        }
    }
}