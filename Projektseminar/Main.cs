using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Google.OrTools.Sat;
using JobShopSchedulingProblemCP.OpeningHeuristic;
using JobShopSchedulingProblemCP.Instance;
using System.Diagnostics;
using Projektseminar.MetaHeuristic;

namespace JobShopSchedulingProblemCP
{
    public class ScheduleRequestsSat
    {
        //Variablen
        string[] chars = new string[] { "/", "-", "\\", " | " };

        int charCounter = 0;

        //Methoden
        public static void Main(String[] args)
        {
            Importer importer = new Importer();
            importer.ImportInstance(@"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\instance1.csv");
            importer.ImportSetup(@"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\instance1_setups.csv");
            Problem problem = importer.GenerateProblem();

            Console.WriteLine("Problem imported, Press any key to continue:");
            Console.ReadKey();
            
            //Instance.Problem newProblem = new Instance.Problem(1);
            //newProblem.importProblem(@"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\instance2.csv");
            //newProblem.ImportSetup(@"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\instance2_setups.csv");

            //ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR();
            //newSolver.SolveProblem(problem);
            
            
            OpeningHeuristic.Giffler_Thompson giffler_Thompson = new Giffler_Thompson();
            Problem enhancedProblem = giffler_Thompson.InitialSolution(problem);
            enhancedProblem.ProblemAsDiagramm(@$"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\diagrammInitial.html");


            Console.WriteLine("Initial solution created, Press any key to continue:");
            Console.ReadKey();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Lokale Suche
            LocalSearch.LocalSearch local_search = new LocalSearch.LocalSearch();
            
            for (int i = 0; i < 200; i++)
            {
                if (stopwatch.Elapsed > new TimeSpan(0, 5, 0))
                {
                     break;
                }
                else
                {
                    int oldMakespan = enhancedProblem.CalculateMakespan(); 
                    enhancedProblem = local_search.DoLocalSearch(false, enhancedProblem, "N3");
                    enhancedProblem.ProblemAsDiagramm(@$"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\diagrammLocalSearchIteration{i}.html");

                    Console.WriteLine($"Iteration {i}");

                if (oldMakespan == enhancedProblem.CalculateMakespan())
                {
                    break;
                }
                }                
            }

            //Simulated Annealing
            /*SimulatedAnnealing simAnneal = new SimulatedAnnealing(enhancedProblem, 100, 0.88, 10);
            enhancedProblem = simAnneal.DoSimulatedAnnealing();*/

            enhancedProblem.ProblemAsDiagramm(@$"C:\Users\tommi\Documents\GitHub\Scheduling24_25\Projektseminar\diagramm.html");

            stopwatch.Stop();
            Console.WriteLine($"Local Search ran {stopwatch.Elapsed.Minutes} Minutes {stopwatch.Elapsed.Seconds} Seconds {stopwatch.Elapsed.Milliseconds} Milliseconds");
        }
    }
} 