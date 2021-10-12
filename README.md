# DOSP_FALL_2021

## Project 2: Gossip Simulator

Group member:
  1. Yizhe Wu, UFID: 4491-6383;
  2. Yepeng Liu, UFID: 1980-9506;

### Usage:
  1. cd to project folder;
  2. Run command dotnet fsi project2.fsx [nodesNum] [topology] [algorithm] in CLI;

### What is working:
In this project, we designed algorithm to run all Line, Full, 3D, and Imp3D with gossip or pushsum protocol. After convergence, the node stops transmitting the message to its neighbor. Once the network is converged, the total time for convergence is printed out.

![图片 1](https://user-images.githubusercontent.com/40141652/136863103-8154f95c-3315-462c-8352-2a0d78eee438.png)
![图片 2](https://user-images.githubusercontent.com/40141652/136863127-33e37654-319a-488a-8bda-2daf1380ef6d.png)
![图片 3](https://user-images.githubusercontent.com/40141652/136863128-b443e4e1-af01-4b12-9dfc-efd035abd7ba.png)
![图片 4](https://user-images.githubusercontent.com/40141652/136863132-f56615c1-c2ce-4ffa-b5f1-830e6b20edbb.png)

### What is the largest network you managed to deal with for each type of topology and algorithm:
We tested 10K nodes in our algorithm, and it works well.
