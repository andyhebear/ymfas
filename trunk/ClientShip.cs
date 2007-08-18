using System;
using Mogre;
using MogreNewt;

namespace Ymfas
{
    class ClientShip : Ship
    {
        SceneNode node;
        Entity mesh;
        public ClientShip(World _w, SceneManager _mgr, SceneNode _parent, string _id, Vector3 _position, Quaternion _orientation)
		{
			SceneNode parent = _mgr.RootSceneNode;
            if(_parent != null){
                parent =  _parent;
            }

			id = _id;
			mesh = _mgr.CreateEntity(id, "razor.mesh");
			node = parent.CreateChildSceneNode();
			node.AttachObject(mesh);


            MogreNewt.CollisionPrimitives.Box bodyBox = 
                new MogreNewt.CollisionPrimitives.Box(_w, mesh.BoundingBox.Size);
            body = new Body(_w, bodyBox);

            float mass;
            Vector3 inertia;
            body.getMassMatrix(out mass, out inertia);

			body.attachToNode(node);

            Vector3 v = MogreNewt.MomentOfInertia.CalcBoxSolid(1.0f, mesh.BoundingBox.Size);
			body.setMassMatrix(1.0f, v);

            body.setPositionOrientation(_position, _orientation);
            body.IsGravityEnabled = false;
            body.ForceCallback += new ForceCallbackHandler(ForceTorqueCallback);
            body.setAutoFreeze(0);
            body.setLinearDamping(0.0f);
			body.setAngularDamping(new Vector3(0.0f));
        }
    }
}
