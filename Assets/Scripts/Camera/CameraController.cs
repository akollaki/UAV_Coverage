using System;
using UnityEngine;

namespace Coverage
{

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
       
        // Set Parameters
        public object Map;
        int[] MapSize = new int[2];
        float d;
        private new Camera camera;
        private Vector3 cameraView;

        // Start Camera
        public void Start()
        {
        
            Instance = this;
            Distance();
            camera = GetComponent<Camera>();
            cameraView = camera.transform.position;
            cameraView = new Vector3(cameraView.x, d, cameraView.z); //set final camera position


        }

        //Updates camera position by frame
        private void Update()
        {
            transform.position = cameraView;
        }


        // Calculate distance based on the size of the map
        public float Distance()
        {
            // Get mapsize from Mapgenerator
            MapSize[0] = MapGenerator.Instance.MS[0];
            MapSize[1] = MapGenerator.Instance.MS[1];


            if (MapSize[0] >= MapSize[1])
            {
                d = Convert.ToSingle(Mathf.Tan(0.96f) * MapSize[0] * 0.76);
            }

            else
            { 

                d = Convert.ToSingle(Mathf.Tan(0.96f) * MapSize[1] * 0.76);

            }

            return d;

        }


    }
}


