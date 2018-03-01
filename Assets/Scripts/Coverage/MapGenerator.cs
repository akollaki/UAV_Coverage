using System.Collections.Generic;
using UnityEngine;
using Coverage.Navigation;
using UnityEngine.UI;

namespace Coverage
{
    public class MapGenerator : MonoBehaviour
    {
        //Instances
        public static MapGenerator Instance;
        public MainClass MC= new MainClass();


        //Lists
        List<Coord> allTileCoords;
        Queue<Coord> shuffledTileCoords;
        // Prefabs
        public Transform tilePrefab;
        public Transform obstaclePrefab;
        public Transform sourcePrefab;
        public Transform dronePrefab;

        //Variables controlled by user
        public Vector2 mapSize;
        public int DroneCount;
        public Vector2 sourcePosition;
        public Vector2 targetPosition;
        public int obstacleCount;
        public int seed;
        [Range(0, 1)]
        public float outlinePercent;
        public Text eText;
        public Text pText;


        //Variables used in script
        string holderName = "Generated Map";
        [HideInInspector]public int DC;
		[HideInInspector]public int OC;
		[HideInInspector]public int [] SP= new int[2] ;
		[HideInInspector]public int [] TP= new int[2];
		[HideInInspector]public int [] MS = new int[2];
        bool stop = false;




        public void Start() //Methods needed to start Unity
        {
            Instance = this;
            ReadInitial();
            if (stop == false)
            {
                TilesList();
                MC.JustDoIt();
                generateMap();
                CameraController.Instance.Start();
            }
            else
            {
                Debug.Log("Please choose appropriate source and target positions");
            }


        }

        //Create error text on Unity
        public void errorText()
        {
            eText.text = "Path NOT Found!";
        }

        public void pathText()
        {
            pText.text = "Path Found!";
        }

        //create list for tiles / grids and select random as obstacles
        public void TilesList()
        {
            allTileCoords = new List<Coord>();
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    allTileCoords.Add(new Coord(x, y));
                }
            }

            shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), seed));
        }


        public void generateMap()
        {                  

            // Destroy previously created parts of "Generated Map", eliminate remnants of older tests
            if (transform.Find(holderName))
            {
                Destroy();

            }

            // Creates the objects that will be destroyed
            Transform mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = transform;


            // Create the map (clone the existing tile with instantiate)
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 tilePosition = CoordToPosition(x, y, 0f);
                    Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                    newTile.localScale = Vector3.one * (1 - outlinePercent);
                    newTile.parent = mapHolder;

                }
            }

            //create Source and Target squares on Unity
            Vector3 sourcePosVec = CoordToPosition(sourcePosition.x, sourcePosition.y, 0.1f);
            Transform newSource = Instantiate(sourcePrefab, sourcePosVec, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newSource.parent = mapHolder;

            Vector3 targetPosVec = CoordToPosition(targetPosition.x, targetPosition.y, 0.1f);
            Transform newTarget = Instantiate(sourcePrefab, targetPosVec, Quaternion.Euler(Vector3.right * 90)) as Transform;
            newTarget.parent = mapHolder;

            // Creates objects on Unity
            foreach (Coord el in MainClass.Obstacle)
            {
                Vector3 obstaclePosition = CoordToPosition(el.x, el.y, 0f);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * .5f, Quaternion.identity) as Transform;

                newObstacle.parent = mapHolder;

            }

            // Creates drones on unity from positions in list
            foreach (NavNode item in MainClass.p_main)
            {
                Vector3 DronePosition = CoordToPosition((item.xCell), (item.yCell)-0.2f, 0.1f);
                Transform NewDrone = Instantiate(dronePrefab, DronePosition, Quaternion.identity) as Transform;
                NewDrone.parent = mapHolder;

            }


        } // Map Generator


        // Method for calculating position for coordinate
        public Vector3 CoordToPosition(float x, float y, float z)
        {
            return new Vector3(-mapSize.x/2 +x , z, -mapSize.y/2 +y);
        }


        // Get random coordinate for obstacle placement
        public Coord GetRandomCoord()
        {
            Coord randomCoord = shuffledTileCoords.Dequeue();
            shuffledTileCoords.Enqueue(randomCoord);
            return randomCoord;
        }
        

        public struct Coord
        {
            public int x;
            public int y;

            public Coord(int _x, int _y)
            {

                x = _x;
                y = _y;
            }
        }

        // Destroys old maps
        public void Destroy()
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        //Methods for initialization
        public int getDroneCount()
        {
            return DroneCount; //changed from instance setup from before
        }

        public int[] getSourcePosition()
        {
            int[] x = new int[] { (Mathf.RoundToInt(sourcePosition.x)), (Mathf.RoundToInt(sourcePosition.y)) };
            return x;
        }

        public int[] getTargetPosition()
        {
            int[] x = new int[] { (Mathf.RoundToInt(targetPosition.x)), (Mathf.RoundToInt(targetPosition.y)) };
            return x;
        }

        public int[] getMapSize()
        {
            int[] x = new int[] { (Mathf.RoundToInt(mapSize.x)), (Mathf.RoundToInt(mapSize.y)) };
            return x;
        }

        public int getObstacleCount()
        {
            return obstacleCount;     
        }

        // Variables from MapGenerator in a method to make it easier to use in different files
        public void ReadInitial()
        {
            DC = getDroneCount();
            SP = getSourcePosition();
            TP = getTargetPosition();
            MS = getMapSize();
            OC = getObstacleCount();


            //if user has chosen source and target position out of reach, display error message
            if (SP[0] > (MS[0] - 1))
            {
                Debug.Log("Source Position (x) out of desired Map size");
                stop = true;

            }
            if (SP[1] > (MS[1] - 1))
            {
                Debug.Log("Source Position (y) out of desired Map size");
                stop = true;
            }
            if (TP[0] > (MS[0] - 1))
            {
                Debug.Log("Target Position (x) out of desired Map size");
                stop = true;
            }
            if (TP[1] > (MS[1] - 1))
            {
                Debug.Log("Target Position (y) out of desired Map size");
                stop = true;
            }
        }

    }

}