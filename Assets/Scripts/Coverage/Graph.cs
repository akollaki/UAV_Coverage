using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coverage
{
    namespace Navigation
    {
        static class Param
        {

            public static bool verbose = false;


        }
        //Basic class for link representation between nodes
        public class Link
        {
            

            Node adj;
            Node start;
            double weight;

            public Link(Node start, Node adj, double weight)
            {
                this.adj = adj;
                this.weight = weight;
                if (Param.verbose)
                {
                    Console.WriteLine("Link  created");
                }

                this.start = start;
            }
            public Node getAdj()
            {
                return adj;
            }
            public Node getOwnerNode()
            {
                return start;
            }
            public double getWeight()
            {
                return weight;
            }
            public void setWeight(double w)
            {
                weight = w;
            }
        }//CLASS Link

        //Simple graph node
        public class Node
        {
            //Node unique id set by programmer
            int id;
            //Position in node list
            public int listPosition { get; set; }
            //Is occupied / active ?
            public bool isActive { get; set; }
            //Connections with other nodes
            public List<Link> links;
            //Incoming connections 
            public List<Link> incomingLinks;
            //Cost from source node
            public double distFromSource;
            //Previous Node ID for optimal path
            public int prevNode;
            //Node from source
            public Node(int id)
            {
                this.id = id;
                isActive = true;
                links = new List<Link>();
                incomingLinks = new List<Link>();
                //Console.WriteLine("Node" + id + " created");
                distFromSource = Double.MaxValue;
                prevNode = -1;
            }

            public int getId()
            {
                return id;
            }
            public void setId(int idValue)
            {
                id = idValue;
            }
            public void addLink(Node adj, double weight)
            {
                var lin = new Link(this, adj, weight);
                links.Add(lin);
                adj.addIncomingLink(lin);
            }
            public void addIncomingLink(Link l)
            {

                incomingLinks.Add(l);

            }
            public void removeLink(int linkId)
            {

                links.Remove(links[linkId]);

            }
        }//CLASS Node

        //Simple graph
        public class Graph
        {

            int num_vert = 0;
            bool weight_generated;
            //Weight representation used for convenience
            protected double[,] w_matrix;
            public List<Node> nodes = new List<Node>();

            public Graph()
            {
                weight_generated = false;
                if (Param.verbose)
                {
                    Debug.Log("Graph created!");
                }
            }

            public virtual void init()
            {
                //Nodes are sorted by ID number
                nodes.Sort((Node x, Node y) => x.getId().CompareTo(y.getId()));
                //Get number of total nodes
                num_vert = nodes.Count;

                //Helper variable listPosition after sorting
                int c = 0;
                foreach (var item in nodes)
                {
                    item.listPosition = c++;
                }

                w_matrix = new double[num_vert, num_vert];

            }
            public int getNumVert()
            {
                return num_vert;
            }
            //Generate weight matric from node list set by programmer
            public void computeWeights()
            {
                //Only if we added every node to the list
                if (num_vert > 0)
                {
                    w_matrix = new double[num_vert, num_vert];

                    //Fill weighted matrix
                    for (int i = 0; i < num_vert; i++)
                    {

                        {
                            for (int j = 0; j < num_vert; j++)
                            {
                                //Set diag to zero
                                if (i == j)
                                {
                                    w_matrix[i, j] = -1;
                                }
                                else
                                {
                                    //Unreachable by default
                                    w_matrix[i, j] = -1;

                                    //Check possible links
                                    if (nodes[i].links.Count > 0)
                                    {
                                        foreach (var l in nodes[i].links)
                                        {

                                            if (l.getAdj().getId() == nodes[j].getId() && l.getAdj().isActive)
                                            {
                                                if (Param.verbose)
                                                {
                                                    Console.WriteLine("Node " + nodes[i].getId() + " is connected to node: " + nodes[j].getId() + " with a weight of: " + l.getWeight());
                                                }

                                                w_matrix[i, j] = l.getWeight();

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("ERROR, node list is empty");
                    weight_generated = false;
                    return;
                }
                weight_generated = true;
            }
            //Generate node list from matrix given by programmer
            public void computeWeights(double[,] matrix, int rank)
            {

                num_vert = rank;

                if (matrix.GetLength(0) != rank || matrix.GetLength(1) != rank)
                {
                    Console.WriteLine("ERROR: matrix must be a " + rank + "X" + rank);
                    return;
                }

                w_matrix = matrix;

                //We need to populate node list at this point
                nodes.Clear();
                for (int i = 0; i < rank; i++)
                {
                    var nodeTemp = new Node(i);
                    nodeTemp.listPosition = i;
                    nodes.Add(nodeTemp);
                }
                //Start linking
                for (int i = 0; i < num_vert; i++)
                {
                    for (int j = 0; j < num_vert; j++)
                    {
                        double tempW = w_matrix[i, j];
                        if (tempW > 0)
                        {
                            nodes[i].addLink(nodes[j], tempW);
                        }
                    }
                }
                weight_generated = true;
            }
            //Print weight matrix
            public void printL()
            {

                for (int i = 0; i < num_vert; i++)
                {
                    for (int j = 0; j < num_vert; j++)
                    {
                        Console.Write(w_matrix[i, j] + " ");
                    }
                    Console.Write("\n");
                }
            }
            public double[,] getWeightMatrix()
            {
                return w_matrix;
            }
            //Return the node by its id
            public virtual Node getNodeByID(int id)
            {

                Node temp;
                //Find the Node in list with the corresponding id

                if (nodes[id].getId() == id) //lucky case
                {
                    temp = nodes[id];
                }
                else
                {                        //at least we tried, search over the list
                    temp = nodes.Find((Node obj) => { return obj.getId() == id; });
                }

                return temp;
            }
            //Static version (used by Cost function static class)
            public static Node getNodeByID(int id, List<Node> n)
			{

				Node temp;
				//Find the Node in list with the corresponding id

				if (n[id].getId() == id) //lucky case
                {
                    temp = n[id];
                }
                else
				{                        //at least we tried, search over the list
					temp = n.Find((Node obj) => { return obj.getId() == id; });
				}

				return temp;
			}
            //Get the shortest path
            public LinkedList<Node> getShortestPath(int sourceId, int destId)
            {
                var path = new LinkedList<Node>();
                var source = getNodeByID(sourceId);
                var dest = getNodeByID(destId);

                if (weight_generated)
                {
                    //Start looking for shortest path
                    double[,] dist = Dijkstra2.Dijkstra(w_matrix, source.listPosition, num_vert);
                    //Fill nodes fields, we may need it
                    for (int i = 0; i < num_vert; i++)
                    {
                            nodes[i].distFromSource = dist[i, 0];
                            nodes[i].prevNode = (int)dist[i, 1];
                    }

                    //Traverse the tree
                    path = traverse(source, dest);
                }
                else
                {
                    Console.WriteLine("You need to call generateWeights method first!");
                }

                return path;
            }
            //Find optimalpath from Dijkstra Tree, scans from target to source (it includes start and destination in the path)
            public LinkedList<Node> traverse(Node source, Node dest)
            {
                var path = new LinkedList<Node>();
                //Traverse the tree
                path.AddFirst(dest);
                int actualPId = dest.listPosition;

                while (actualPId != source.listPosition)
                {
                    actualPId = nodes[actualPId].prevNode;
                    path.AddFirst(nodes[actualPId]);

                }
                return path;
            }

        }//CLASS Graph
    }//NAMESPACE Navigation
}//NAMESPACE Coverage
