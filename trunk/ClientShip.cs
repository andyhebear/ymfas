using System;
using Mogre;
using MogreNewt;

namespace Ymfas
{
    public class ClientShip : Ship
    {
        SceneNode node;
        Entity mesh;
        public ClientShip(World _w, SceneManager _mgr, SceneNode _parent, int _id, Vector3 _position, Quaternion _orientation) : base (_w, _id, _position, _orientation)
		{
			SceneNode parent = _mgr.RootSceneNode;
            if(_parent != null){
                parent =  _parent;
            }

			mesh = _mgr.CreateEntity(id.ToString(), "razor.mesh");
			node = parent.CreateChildSceneNode();
			node.AttachObject(mesh);
			body.attachToNode(node);
        }
        
        public SceneNode SceneNode
        {
            get { return node; }
        }
        public Entity Mesh
        {
            get { return mesh; }
        }
        public override void Dispose()
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
