using System.Collections.Generic;
using UnityEngine;


namespace Coverage
{

    using Navigation;

    public class MainClass
    {
        //Creating Instances 
        public MainClass MC;
        public static MainClass Instance;

        // Creating Variables for MapGenerator
        public static List<MapGenerator.Coord> Obstacle = new List<MapGenerator.Coord>();
        public static LinkedList<Node>  p_main;
		public int NrDrones;
        public int[] target = new int [2];
        public int[] sizeMap = new int [2];
        public int[] source = new int[2];
		public int NrObstacles;

            public void JustDoIt()
        {

            //Get Variables from Mapgenerator
			NrDrones=MapGenerator.Instance.DC;
			target [0] = MapGenerator.Instance.TP [0];
			target [1] = MapGenerator.Instance.TP [1];
            MapGenerator.Coord targetCoord = new MapGenerator.Coord(target[0], target[1]);
			source [0]= MapGenerator.Instance.SP [0];
			source [1]= MapGenerator.Instance.SP [1];
            MapGenerator.Coord sourceCoord = new MapGenerator.Coord(source[0], source[1]);
            sizeMap[0]= MapGenerator.Instance.MS [0];
			sizeMap[1]= MapGenerator.Instance.MS [1];
			NrObstacles = MapGenerator.Instance.OC;

			//Get Obstacles 
	            for (int i = 0; i < NrObstacles; i++)
	            {
	                MapGenerator.Coord randomCoord = MapGenerator.Instance.GetRandomCoord();
                if (((randomCoord.x != targetCoord.x) & (randomCoord.y != targetCoord.y)) && ((randomCoord.x != sourceCoord.x) & (randomCoord.y != sourceCoord.y)))
                {
                    Obstacle.Add(randomCoord);

                }
                else i--;
	            }

            //You can create an arbitrary graph
            Graph g = new Graph();
            Node n0 = new Node(0);
            Node n1 = new Node(1);
            Node n2 = new Node(2);
            Node n3 = new Node(3);
            Node n4 = new Node(4);
            Node n5 = new Node(5);
            Node n6 = new Node(6);
            Node n7 = new Node(7);

            //Add nodes to it
            g.nodes.Add(n0);
            g.nodes.Add(n1);
            g.nodes.Add(n2);
            g.nodes.Add(n3);
            g.nodes.Add(n4);
            g.nodes.Add(n5);
            g.nodes.Add(n6);

            // You can directly provide a weight matrix instead
            double[,] L ={
                {-1,  5, -1, -1, -1,  3, -1, -1},
                { 5, -1,  2, -1, -1, -1,  3, -1},
                {-1,  2, -1,  6, -1, -1, -1, 10},
                {-1, -1,  6, -1,  3, -1, -1, -1},
                {-1, -1, -1,  3, -1,  8, -1,  5},
                { 3, -1, -1, -1,  8, -1,  7, -1},
                {-1,  3, -1, -1, -1,  7, -1,  2},
                {-1, -1, 10, -1,  5, -1,  2, -1}
            };

            //Init must be called once before operations
            g.init();

            //This generates nodes structure from matrix. Otherwise you can use computeWeights(); if you provided nodes; this will create the weight matrix
            g.computeWeights();

            //Return a linked list with the shortest path
            var path = g.getShortestPath(0, 2);


            //Dual ascent test
            Map m = new Map(sizeMap[0], sizeMap[1], 1);

            // for every coordinate generated in mapgenerator add obstacle
            foreach (MapGenerator.Coord el in Obstacle)
            {
                m.addObstacle(el.x, el.y);
            }



            //Init must be called after adding obstacles
            m.init();
            //Debug.Log("Started m");
            var asc = new DualAscent(m,NrDrones);
            p_main = asc.run(m.getNodeIdFromCell(source[0], source[1]), m.getNodeIdFromCell(target[0], target[1]));




        } //end JustDoIt

    }// End Class
}// End Coverage
