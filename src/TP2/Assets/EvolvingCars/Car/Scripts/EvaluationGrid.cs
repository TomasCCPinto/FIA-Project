using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace GeneticSharp.Runner.UnityApp.Car
{
    public class EvaluationGrid : MonoBehaviour
    {
        private ConcurrentQueue<FollowChromosomeCam> m_availableCameras;

        public Object CameraPrefab;

		private void Awake()
		{
            m_availableCameras = new ConcurrentQueue<FollowChromosomeCam>();

            var cam = ((GameObject)Instantiate(CameraPrefab, Vector3.zero, Quaternion.identity)).GetComponent<Camera>();
            cam.transform.parent = transform.parent;
                
            var width = cam.pixelRect.width;
            var height = cam.pixelRect.height;

            cam.pixelRect = new Rect(0,0, width, height);
            m_availableCameras.Enqueue(cam.GetComponent<FollowChromosomeCam>());
		}

		public FollowChromosomeCam AddChromosome(GameObject chromosome)
        {
            FollowChromosomeCam cam;

            if (m_availableCameras.TryDequeue(out cam))
            {
                cam.StartFollowing(chromosome);
            }
            else
            {
                Debug.LogError("Cannot dequeue camera");    
            }

            return cam;
        }
    }
}