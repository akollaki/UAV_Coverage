using System;
using System.Collections.Generic;
namespace Coverage
{
    namespace Navigation
    {

        public class CostFunctions{

            public static double vehicleAction = 2;
            public static double alpha = 0;

            public static double euclideanDist(NavNode s, NavNode t){
                double sqrSum = Math.Pow(t.yCoord - s.yCoord, 2) + Math.Pow(t.xCoord - s.xCoord, 2);
                return Math.Sqrt(sqrSum);
            }
			public static double commCost(NavNode s, NavNode t, double resolution, int r, int c , List<Node> nodes)
			{

				//Bad way to check if line intersects a cell but fast to implement.
                double euclDistToTarget = euclideanDist(s, t);
				var samplingN = resolution * 10;
				var delta = vehicleAction / samplingN;
				var versor = new double[2];
				versor[0] = (t.xCoord - s.xCoord) / euclDistToTarget;
				versor[1] = (t.yCoord - s.yCoord) / euclDistToTarget;

				//Sample the line and count how many points lie inside an obstacle
				int pInside = 0;
				for (int i = 0; i < samplingN; i++)
				{
                    
                    double x = versor[0] * delta * i + s.xCoord;
					double y = versor[1] * delta * i + s.yCoord;

                    int xC = Map.getCellFromCoord(x, y, resolution, r, c)[0];
					int yC = Map.getCellFromCoord(x, y, resolution, r, c)[1];
                    //Clamp xC and yC
                    if (xC >= r)
                        xC = r - 1;
					if (yC >= c)
						yC = c - 1;
					
                    //Check if point lies inside an obstacle. 
                    if (!Graph.getNodeByID(Map.getNodeIdFromCell(xC,yC,c),nodes).isActive)
					{
						pInside++;
					}

				}
                return 1 * euclideanDist(s,t) + 20* pInside / 10.0;    			
            }

        }

        public class NavNode : Node {

            public double xCoord { get ; set; }
            public double yCoord { get; set; }

            public int xCell { get; set; }
            public int yCell { get; set; }

            public int nvisited { get; set; }

            public NavNode(int id, int x, int y) : base(id)
            {
                xCell = x;
                yCell = y;
                nvisited = 0;
            }
            public void setPosition(double x, double y){
                xCoord = x;
                yCoord = y;
            }

        }


        public class Map : Graph
        {

            public int rows { get; }
            public int cols { get; }
            //Distance between cells centers
            double resolution;

            double[,] fixedWeight;

            public double vehicleAction { get; set; }


            public Map(int rows, int cols, double res)
            {
                this.rows = rows;
                this.cols = cols;
                resolution = res;
                vehicleAction = CostFunctions.vehicleAction;
                fixedWeight = new double[rows, cols];
                //Fill the node list
                generateNodes();
            }

            public override void init(){
				//Generate weight matrix as Dijkstra input, initialize links and keep a comm cost copy
				initLinks();
				computeWeights();
                fixedWeight = (double[,])w_matrix.Clone();
            }

            void generateNodes()
            {
                int id = 0;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        var tempNode = new NavNode(id++, i, j); 
                        tempNode.setPosition(resolution * (i + 0.5),resolution * (j + 0.5));
                        nodes.Add(tempNode);
                    }
                }
                base.init();
            }
            //Set a cell to be an obstacle.
            public void addObstacle(int xCell,int yCell){
                getNodeByID(getNodeIdFromCell(xCell, yCell)).isActive = false;
                //Update weight matrix
                for (int i = 0; i < getNumVert(); i++)
                {
                    w_matrix[getNodeIdFromCell(xCell, yCell), i] = -1;
                    w_matrix[i, getNodeIdFromCell(xCell, yCell)] = -1;
                }
            }

            //Links creation based on obstacles, vehicle action and initial weights
            void initLinks(){

                foreach (var item in nodes)
                {
                    if (item is NavNode && item.isActive)
					{
                        
                        //Find max y and x respect to vehicle action

                        double xMaxCoord = ((NavNode)item).xCoord + vehicleAction;
                        double yMaxCoord = ((NavNode)item).yCoord + vehicleAction;

                        int maxX = getCellFromCoord(xMaxCoord, yMaxCoord)[0];
                        int maxY = getCellFromCoord(xMaxCoord, yMaxCoord)[1];

						//Find min y and x respect to vehicle action
						int minX;
						int minY;

						double xMinCoord = ((NavNode)item).xCoord - vehicleAction;
						double yMinCoord = ((NavNode)item).yCoord - vehicleAction;
						minX = getCellFromCoord(xMinCoord, yMinCoord)[0];
						minY = getCellFromCoord(xMinCoord, yMinCoord)[1];
                       
                        //Clamp
                        if (maxX > rows - 1)
                            maxX = rows - 1;
                        if (minX < 0)
							minX = 0;
                        if (maxY > cols - 1)
                            maxY = cols - 1;
						if (minY < 0)
							minY = 0;

                        //Connect nodes depending on vehicle action and obstacles. Initial weight based on initial cost function params
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                //Find attachable nodes
                                NavNode target = (NavNode)getNodeByID(getNodeIdFromCell(i, j));
                                double euclDistToTarget = CostFunctions.euclideanDist((NavNode)item, target);
                                bool check = ((item.getId() != target.getId()) && (target.isActive) && ((euclDistToTarget <= vehicleAction)));
                                if (check)
                                {
                                   
                                    //TODO: find a smart way to manage weights
                                    //choose initial weights
                                    int count = target.nvisited;
                                    double alpha = CostFunctions.alpha;
                                    double comm = CostFunctions.commCost((NavNode)item, target, resolution, rows, cols, nodes);
                                    double W = count + comm + alpha;
                                    // And finally, link
                                    item.addLink(target, W);
                                }

                            }

                        }

                    }

				}

            }
            public void updateWeights(double alpha = 0){

                foreach (var node in nodes)
                {
                    if (node.isActive)
                    {
                        foreach (var link in node.links)
                        {
                            NavNode target = (NavNode)link.getAdj();
                            double comm = fixedWeight[node.getId(), target.getId()];
                            var newWeight = comm + target.nvisited + alpha;
                            link.setWeight(newWeight);
                        }
                    }
                }
                computeWeights();

            }
            //returns the cell x and y from the spatial point
            public int[] getCellFromCoord(double xCoord, double yCoord){

                var t = new int[2];
                double x;
                double y;

                if(Math.Abs(xCoord % resolution) <= Double.Epsilon)
                    x = (xCoord / resolution) - 1;
                else
                    x = (xCoord / resolution);

				if (Math.Abs(yCoord % resolution) <= Double.Epsilon)
                    y = (yCoord / resolution) - 1;
				else
                    y = (yCoord / resolution);
                
                if (x<0)
                    x = 0;
                if (y<0)
                    y = 0;
                if (x > rows - 1)
                    x = rows - 1;
				if (y > cols - 1)
					y = cols - 1;
                t[0] = (int)x;
				t[1] = (int)y;

                return t;

			}
            //Static version for external usage
			public static int[] getCellFromCoord(double xCoord, double yCoord,double resolution,int rows,int cols)
			{

				var t = new int[2];
				double x;
				double y;

				if (Math.Abs(xCoord % resolution) <= Double.Epsilon)
					x = (xCoord / resolution) - 1;
				else
					x = (xCoord / resolution);

				if (Math.Abs(yCoord % resolution) <= Double.Epsilon)
					y = (yCoord / resolution) - 1;
				else
					y = (yCoord / resolution);

				if (x < 0)
					x = 0;
				if (y < 0)
					y = 0;
				if (x > rows - 1)
					x = rows - 1;
				if (y > cols - 1)
					y = cols - 1;
				t[0] = (int)x;
				t[1] = (int)y;

				return t;

			}
            //returns cell center of mass from x and y
			public double[] getCoordFromCell(int xCell, int yCell)
			{
      
                var t = new double[2];
                t[0] =  resolution * (xCell + 0.5);
                t[1] =  resolution * (yCell + 0.5);

                return t;
			}
            public int  getNodeIdFromCell(int xCell, int yCell){
                return xCell*cols + yCell;
            }
            //Static version
			public static int getNodeIdFromCell(int xCell, int yCell, int c)
			{
				return xCell * c + yCell;
			}

        }//Class Map
    }//Namespace Navigation
}//Namespace Coverage
