using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coverage
{
    namespace Navigation
    {
        public class DualAscent
        {
            //Creating variables for the class
            Map map;
            double alpha_actual;
            int n_vehicles;
            double eps_min;

            //setting them public for other files
            public DualAscent(Map m, int n_vehicles)
            {
                map = m;
                alpha_actual = CostFunctions.alpha;
                this.n_vehicles = n_vehicles;
                eps_min = Double.MaxValue;

            }
            public void setNVehicles(int n){
                n_vehicles = n;
            }
			public void reset()
			{
                alpha_actual = 0;
				eps_min = Double.MaxValue;
                map.updateWeights();
			}

            //Checks if current link is in a general path
            bool isLinkInPath(LinkedList<Node> path, Link link){

                int id_s = link.getOwnerNode().getId();
                int id_t = link.getAdj().getId();

                bool sourceFound = false;

                List<int> tempId = new List<int>();

                foreach (var item in path)
                {
                    if(sourceFound){
                        if (tempId.Exists((obj) => { return obj == id_t; }))
                            return true;
                    }

                    //Link starts in this node
                    if (item.getId() == id_s)
                    {
                        sourceFound = true;
                        tempId.Clear();
                        foreach (var testLink in item.links)
                        {
                            //Store adjoint ids for next iteration
                            tempId.Add(testLink.getAdj().getId());
                        }

                    }
                    else
                        sourceFound = false;
                    
                }
                //no mathing link
                return false;
            }


            //Dual ascent algorithm here, return empty list if it fails
            public LinkedList<Node> run(int source_id, int target_id)
            {
                var ql = 1;     //nr of optimisation loops
                //Init stuctures
                var qy = new double[map.getNumVert(),2];
                bool done = false;
                var alpha_prev = alpha_actual;
                var path = new LinkedList<Node>();
                path = map.getShortestPath(source_id, target_id);
				if (path.Count - 2 <= n_vehicles)
                {
                    done = true;
                }

                while (!done)
		        {
                    
                    path = map.getShortestPath(source_id, target_id);
                    if (path.Count - 2 <= n_vehicles) //Check if path exists
                    {
                        done = true;
                        break;
                    }
                   
					
	                //Calculate qn,yn
                    int i = 0;		 
                    
                    foreach (var item in map.nodes)		               
                    {	                        	                      	                     
                        var path_temp = map.traverse(map.getNodeByID(source_id), item);             
                        var hops = path_temp.Count - 1 ;		                     
                        if (hops < 0)		                     
                            hops = 0;
		                      
                        qy[item.getId(), 0] = hops;		                     
                        qy[item.getId(), 1] = item.distFromSource;
                        i++;		                   
                    }

                    //Check for shorter links 
                    bool shorterPathFound = false;
                    foreach (var node in map.nodes)
                    {
                        foreach (var link in node.links)
                        {
                            int n = node.getId();
                            int n_p = link.getAdj().getId();
                            double qnp = qy[n_p, 0];
                            double qn = qy[n, 0];
                            bool check = qnp > qn + 1;

                            if (check)
                            {
                                shorterPathFound = true;
                                double eps = (qy[n, 1] + link.getWeight() - qy[n_p, 1]) / (qy[n_p, 0] - (qy[n, 0] + 1));
                                if (eps < eps_min  && eps > 0.001) //Magic number, avoid to get stuck with eps very low
                                    eps_min = eps;
                            }
                        }
                    }

                    //See if we found a better path, otherwise return an empty list
                    if (!shorterPathFound)
                    {
                        done = true;
                        path.Clear();
                        MapGenerator.Instance.errorText();
                        
                        return path;

                    }

                    alpha_actual += eps_min;

                    alpha_prev = alpha_actual;
                    map.updateWeights(alpha_actual);
                    ql++;
                }

                //Calculated path;
                MapGenerator.Instance.pathText();

                return path;
            } 
        }//CLASS DualAscent
    }//NAMESPACE Navigation
}//NAMESPACE Coverage
