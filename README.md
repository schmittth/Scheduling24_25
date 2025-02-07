Github-CI:<br>
[![Build Status][github_linux_status]][github_linux_link]
[![Build Status][github_macos_status]][github_macos_link]
[![Build Status][github_windows_status]][github_windows_link]

[![Build Status][github_amd64_docker_status]][github_amd64_docker_link]

[github_linux_status]: ./../../actions/workflows/amd64_linux.yml/badge.svg
[github_linux_link]: ./../../actions/workflows/amd64_linux.yml
[github_macos_status]: ./../../actions/workflows/amd64_macos.yml/badge.svg
[github_macos_link]: ./../../actions/workflows/amd64_macos.yml
[github_windows_status]: ./../../actions/workflows/amd64_windows.yml/badge.svg
[github_windows_link]: ./../../actions/workflows/amd64_windows.yml

[github_amd64_docker_status]: ./../../actions/workflows/amd64_docker.yml/badge.svg
[github_amd64_docker_link]: ./../../actions/workflows/amd64_docker.yml

# Introduction

Scheduling Project of Thomas and Bastian for WiSe 24/25.

# IMPORTANT
- Before running the metaheurisic always close LogFile.csv on your pc.
- Open Main.cs on a runtime environment of your choice and hit the "Run"- (or Build) Button
- Choose "Release" in the configuration manager or at least not "Debug", as code will be slower.

# Final Metaheuristic

- Branch FixedParameters / main (contains the same)
Complete Metaheuristic without any room for change (without changing code), other than choosing the instance. 

- Dynamic Parameters 
Metaheuristic with ability to set custom values like IterationMax, CoolingFactor etc.
Please do not try to pass funky values, but keep it within the limitations of simulated annealing. E.g. CoolingFactor < 1

# Test Branches
Branches on which testing was performed
- N(X)CF(X)
- GifflerThompson
- LocalSearch
