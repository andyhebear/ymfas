using System;
using Mogre;

namespace Ymfas
{
    class ArcballCamera
    {
        // rotation radius
        private float radius;

        // target view
        private SceneNode target;

        private Camera camera;
		public float pitch, yaw;

        public Camera Camera
        {
          get { return camera; }
          set { camera = value; }
        }
        public SceneNode Target
        {
            get { return target; }
            set { target = value; }
        }
        public float Radius
        {
            get { return radius; }
            set { if (radius >= 0) radius = value; }
        }

        public ArcballCamera(Camera _cam)
        { 
            Camera = _cam;
        }

		public void PitchBy(float rad)
		{
			pitch += rad;
		}
		public void YawBy(float rad)
		{
			yaw += rad;
		}

        // update method
        // adjusts the position to match the direction
        public void Update()
        {
			Camera.Yaw(yaw);
			Camera.Pitch(pitch);
			
            // update the position based on the orientation
            camera.Position = Target.Position + 
				Target.Orientation * new Vector3(0, Radius/6, -Radius);

            camera.Orientation = Target.Orientation;
            camera.Orientation = new Quaternion(Mogre.Math.PI, Camera.Up)*camera.Orientation;
        }
    }
}
