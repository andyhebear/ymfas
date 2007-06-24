using System;
using Tao.Ode;
using IrrlichtNETCP;

namespace Ymfas
{
    /// <summary>
    /// Ship encapsulates the graphical and 
    /// physical components of the player's spaceship.
    /// </summary>
    public class Ship : IDisposable
    {
        /**
         * private memebers fields
         **/
        private IntPtr body;
        private Vector3D position;
        private Vector3D orientation;
        private float drag;
        private float mass;
        private AnimatedMeshSceneNode meshNode;

        public Ship(SceneManager sMgr, IntPtr world)
        {
            // create the ship node
            meshNode = sMgr.AddAnimatedMeshSceneNode(sMgr.GetMesh("../../data/ship.obj"));
			if (meshNode == null || meshNode.Null())
				throw new Exception("Couldn't create ship.");
            // set default position/orientation
            Position = new Vector3D();
            orientation = new Vector3D();

            body = Ode.dBodyCreate(world);
            mass = 1.0f;
            drag = 0.2f;
            
        }

        public void ApplyThruster(bool forward)
        {
            ApplyForce((forward ? 1 : -1) * 1.0f, 0.0f, 0.0f);
        }

        private void ApplyForce(float x, float y, float z)
        {
            Ode.dBodyAddForce((IntPtr)body, x, y, z);
        }
        private void ApplyForce(Ode.dVector3 v)
        {
            ApplyForce(v.X, v.Y, v.Z);
        }
        public void ApplyEnvironmentalForces()
        {
            Ode.dVector3 vel = Ode.dBodyGetLinearVel(body);
            // for now, apply drag
            ApplyForce(-drag * vel.X, -drag * vel.Y, -drag * vel.Z);
        }

        public void Update()
        {
            Ode.dVector3 v = Ode.dBodyGetPosition((IntPtr)body);
            Position = new Vector3D(v.X, v.Y, v.Z);
        }

        public void MoveCameraToPosition(CameraSceneNode cam)
        {
            if (cam != null)
            {
                // move the camera so that it's behind the ship
                cam.Position = Position + new Vector3D(-10, 5, 0);
                cam.Target = Position;
            }
        }

        public void Dispose()
        {
            Ode.dBodyDestroy(body);
        }

        /**
         * public properties
         **/
        public AnimatedMeshSceneNode ShipMeshNode
        {
            get { return meshNode; }
        }
                
        public Vector3D Position
        {
            get { return position; }
            set
            {
                position = meshNode.Position = value;
            }
        }
        public float MaximumSpeed
        {
            get { return 1 / drag; }
            set { if (value <= 0) throw new Exception(); drag = 1 / value; }
        }
        // set the time it takes to get to 50% of maximum acceleration
        public float HalfTime
        {
            get { return mass / drag * (float)Math.Log(2.0); }
            set { if (HalfTime <= 0) throw new Exception(); 
                  SetMass(mass = drag * value / (float)Math.Log(2.0)); }
        }

        /**
         * private helper functions
         **/
        private void SetMass(float f)
        {
            Ode.dMass m = new Ode.dMass(f, new Ode.dVector4(), new Ode.dMatrix3());
            Ode.dBodySetMass(body, ref m);
        }
    }
}
