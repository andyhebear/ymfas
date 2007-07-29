using System;
using Mogre;
using MogreNewt;

namespace Ymfas
{
	class Ship : IDisposable
	{
		SceneNode node;
		Entity mesh;
		Body body;
		string id;

        Vector3 standbyForce;
		Vector3 standbyTorque;

        const float FORCE = 10.0f;
        const float TORQUE = 5.0f;

		public Ship(World _w, SceneManager _mgr, SceneNode _parent, string _id, Vector3 _position)
		{
			SceneNode parent = (_parent == null) ? _mgr.RootSceneNode : _parent;

			id = _id;
			mesh = _mgr.CreateEntity(id, "razor.mesh");
			node = parent.CreateChildSceneNode();
			node.AttachObject(mesh);


            MogreNewt.CollisionPrimitives.Box bodyBox = 
                new MogreNewt.CollisionPrimitives.Box(_w, mesh.BoundingBox.Size);
            body = new Body(_w, bodyBox);

            Console.Out.WriteLine("omega");
            Console.Out.WriteLine(body.getOmega().ToString());
            Console.Out.WriteLine("inertia");
            float mass;
            Vector3 inertia;
            body.getMassMatrix(out mass, out inertia);
            Console.Out.WriteLine(inertia.ToString());


			body.attachToNode(node);

            Vector3 v = MogreNewt.MomentOfInertia.CalcBoxSolid(1.0f, mesh.BoundingBox.Size);
            Console.Out.WriteLine(v.ToString());
			body.setMassMatrix(1.0f, v);

            body.setPositionOrientation(_position, node.Orientation);
            body.IsGravityEnabled = false;
            body.ForceCallback += new ForceCallbackHandler(ForceTorqueCallback);
            body.setAutoFreeze(0);
            body.setLinearDamping(0.0f);
            body.setAngularDamping(new Vector3(0.0f));
            standbyForce = new Vector3();


            Console.Out.WriteLine("omega");
            Console.Out.WriteLine(body.getOmega().ToString());
            Console.Out.WriteLine("inertia");
            body.getMassMatrix(out mass, out inertia);
            Console.Out.WriteLine(inertia.ToString());
            Console.Out.WriteLine(mass);
            Console.Out.WriteLine("standbyTorque");
            Console.Out.WriteLine(standbyTorque.ToString());
            Console.Out.WriteLine("");
            
        }


        void ForceTorqueCallback(Body b)
        {
            //debugging

            Console.Out.WriteLine("time");
            Console.Out.WriteLine(body.getWorld().getTimeStep().ToString());
            Console.Out.WriteLine("omega");
            Console.Out.WriteLine(body.getOmega().ToString());
            Console.Out.WriteLine("inertia");
            float mass;
            Vector3 inertia;
            body.getMassMatrix(out mass, out inertia);
            Console.Out.WriteLine(inertia.ToString());
            Console.Out.WriteLine("standbyTorque");
            Console.Out.WriteLine(standbyTorque.ToString());
            Console.Out.WriteLine("");
            b.addLocalForce(standbyForce, Vector3.ZERO);
            standbyForce = Vector3.ZERO;

            Vector3 pos = new Vector3();
            Quaternion orient = new Quaternion();
            b.getPositionOrientation(out pos, out orient);
            
            b.addTorque(orient * standbyTorque);
			standbyTorque = Vector3.ZERO;

        }

		// thrust is measured in meters / s^2
		public void ThrustRelative(Vector3 vec)
		{
            standbyForce += vec * FORCE * mesh.BoundingRadius;
		}
		public void TorqueRelative(Vector3 vec)
		{
            standbyTorque += vec * TORQUE * mesh.BoundingRadius;
		}
        public void StopRotation(float time)
        {
            Vector3 pos = new Vector3();
            Quaternion orient = new Quaternion();
            body.getPositionOrientation(out pos, out orient);
            Vector3 torque = orient.Inverse() * body.getOmega() * 100.0f / time;
            
            Console.Out.WriteLine("StopRotation");
            Console.Out.WriteLine("torque");
            Console.Out.WriteLine(torque.ToString());
            Console.Out.WriteLine("time");
            Console.Out.WriteLine(time);
            Console.Out.WriteLine("omega");
            Console.Out.WriteLine((orient.Inverse() * body.getOmega()).ToString());
            Console.Out.WriteLine("");

            /*
            if (omega.x > 0.0f)
            {
                standbyTorque += new Vector3(-TORQUE * mesh.BoundingRadius, 0.0f, 0.0f);
            }
            else
            {
                standbyTorque += new Vector3(TORQUE * mesh.BoundingRadius, 0.0f, 0.0f);
            }

            if (omega.y > 0.0f)
            {
                standbyTorque += new Vector3(0.0f, -TORQUE * mesh.BoundingRadius, 0.0f);
            }
            else
            {
                standbyTorque += new Vector3(0.0f, TORQUE * mesh.BoundingRadius, 0.0f);
            }

            if (omega.z > 0.0f)
            {
                standbyTorque += new Vector3(0.0f, 0.0f, -TORQUE * mesh.BoundingRadius);
            }
            else
            {
                standbyTorque += new Vector3(0.0f, 0.0f, TORQUE * mesh.BoundingRadius);
            }
            */      
            
            //x correction

            if(torque.x > TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(-TORQUE * mesh.BoundingRadius, 0.0f, 0.0f);
            }
            else if (torque.x < -TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(TORQUE * mesh.BoundingRadius, 0.0f, 0.0f);
            }
            else
            {
                standbyTorque += new Vector3(-torque.x, 0.0f, 0.0f);
            }
            //y correction
            
            if (torque.y >= TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(0.0f, -TORQUE * mesh.BoundingRadius, 0.0f);
            }
            else if (torque.y <= -TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(0.0f, TORQUE * mesh.BoundingRadius, 0.0f);
            }
            else
            {
                standbyTorque += new Vector3(0.0f, -torque.y, 0.0f);
                Console.Out.WriteLine("y");
            }
            //z correction
            if (torque.z >= TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(0.0f, 0.0f, -TORQUE * mesh.BoundingRadius);
            }
            else if (torque.z <= -TORQUE * mesh.BoundingRadius)
            {
                standbyTorque += new Vector3(0.0f, 0.0f, TORQUE * mesh.BoundingRadius);
            }
            else
            {
                standbyTorque += new Vector3(0.0f, 0.0f, -torque.z);
                Console.Out.WriteLine("z");
            }
        }

        private void PrintBodyPos(Body b, Quaternion orient, Vector3 pos)
        {
            System.Console.WriteLine(pos);
        }

		public Vector3 Position
		{
			get { return node.Position; }
		}
		public SceneNode SceneNode
		{
			get { return node; }
		}
		public Entity Mesh
		{
			get { return mesh; }
		}
        public Vector3 Velocity
        {
            get { return body.getVelocity(); }
            set { body.setVelocity(value); }
        }

		public void Dispose()
		{
			body.Dispose();
			body = null;
			mesh.Dispose();
			mesh = null;
			node.Dispose();
			node = null;
		}
	}
}
