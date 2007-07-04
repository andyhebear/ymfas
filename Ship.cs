using System;
using Mogre;
using MogreNewt;

namespace MogreSimple
{
	class Ship : IDisposable
	{
		SceneNode node;
		Entity mesh;
		Body body;
		string id;

        Vector3 standbyForce;

		public Ship(World _w, SceneManager _mgr, SceneNode _parent, string _id)
		{
			SceneNode parent = (_parent == null) ? _mgr.RootSceneNode : _parent;

			id = _id;
			mesh = _mgr.CreateEntity(id, "razor.mesh");
			node = parent.CreateChildSceneNode();
            node.SetScale(new Vector3(2.0f));
			node.AttachObject(mesh);
            node.Position = new Vector3(0);

            body = new Body(_w, new MogreNewt.CollisionPrimitives.Box(_w, mesh.BoundingBox.Size));
			body.attachToNode(node);
            //body.unFreeze();
			body.setMassMatrix(1.0f, MogreNewt.MomentOfInertia.CalcBoxSolid(1.0f, 
                Mesh.BoundingBox.Size));
            body.setPositionOrientation(new Vector3(0.0f, 0, 0), Quaternion.IDENTITY);
            body.IsGravityEnabled = false;
            body.ForceCallback += new ForceCallbackHandler(ForceCallback);
            //body.Autoactivated += new AutoactivateEventHandler(ActivationCallback);
            body.setAutoFreeze(0);
            body.setLinearDamping(0.0f);
            standbyForce = new Vector3();
        }

        void ForceCallback(Body b)
        {
            System.Console.WriteLine("in force callback...");
            System.Console.WriteLine(standbyForce.ToString());
            b.setForce(standbyForce);
            //b.setForce(new Vector3(0, 0, 6));

            standbyForce = Vector3.ZERO;
        }

		// thrust is measured in meters / s^2
		public void ThrustRelative(Vector3 vec)
		{
            System.Console.Write("Thrusting...");
            standbyForce += vec;
            System.Console.WriteLine(standbyForce.ToString());
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
