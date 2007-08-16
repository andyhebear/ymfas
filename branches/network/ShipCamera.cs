using System;
using Mogre;

namespace Ymfas
{
    class ShipCamera
    {// rotation radius
        private float radius;

        // target view
        private SceneNode target;

        private Camera camera;

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

        public ShipCamera(Camera _cam)
        { 
            Camera = _cam;
        }

        // update method
        // adjusts the position to match the direction
        public void Update()
        {
            camera.Orientation = Target.Orientation;

            // update the position based on the orientation
            camera.Position = Target.Position - 
				Target.Orientation * new Vector3(0, 0, .1f);
            camera.Orientation =
                new Quaternion(Mogre.Math.PI, camera.Up) * Target.Orientation;
        }
    }
}