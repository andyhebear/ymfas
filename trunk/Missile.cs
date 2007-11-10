using System;
using Mogre;
using MogreNewt;


namespace Ymfas
{
    class Missile : IDisposable
    {
        protected Body body;
		protected int id;

        Vector3 standbyForce;
		Vector3 standbyTorque;

        Ship target;

        const float FORCE = 10.0f;
        const float BOUNDING_RADIUS = 50.0f;
        const float MAX_X_TORQUE = 5.0f;
        const float MAX_Y_TORQUE = 5.0f;
        const float MAX_Z_TORQUE = 5.0f;

		public Missile(World _w, int _id, Vector3 _position, Quaternion _orientation, Ship _target)
		{

			id = _id;
            target = _target;

            MogreNewt.CollisionPrimitives.Box bodyBox =
                new MogreNewt.CollisionPrimitives.Box(_w, new Vector3(1.0f, 1.0f, 1.0f));
            body = new Body(_w, bodyBox);

            float mass;
            Vector3 inertia;
            body.getMassMatrix(out mass, out inertia);

           	body.setMassMatrix(1.0f, new Vector3(1.0f, 1.0f, 1.0f));

            body.setPositionOrientation(_position, _orientation);
            body.IsGravityEnabled = false;
            body.ForceCallback += new ForceCallbackHandler(ForceTorqueCallback);
            body.setAutoFreeze(0);
            body.setLinearDamping(0.0f);
			body.setAngularDamping(new Vector3(0.0f));
        }

        protected void ForceTorqueCallback(Body b)
        {
            b.addLocalForce(CalculateThrust(), Vector3.ZERO);
        }

		public void InertiaCheck()
		{
			Vector3 pos; Quaternion orient;
			body.getPositionOrientation(out pos, out orient);
			float mass; Vector3 massMatrix;
			body.getMassMatrix(out mass, out massMatrix);
		}

		// thrust is measured in meters / s^2
        private Vector3 CalculateThrust()
        {
            Vector3 v = target.Position - this.Position;
            return v / v.Length * FORCE;
        }


        private void PrintBodyPos(Body b, Quaternion orient, Vector3 pos)
        {
            System.Console.WriteLine(pos);
        }

		public Vector3 Position
		{
            get
            {
                Vector3 pos;
                Quaternion or;
                body.getPositionOrientation(out pos, out or);
                return pos; 
            }
		}
		public Vector3 Velocity
        {
            get { return body.getVelocity(); }
            set { body.setVelocity(value); }
        }
		public ShipState ShipState
		{
			get
			{
				ShipState s = new ShipState();
				body.getPositionOrientation(out s.Position, out s.Orientation);
				s.Velocity = body.getVelocity();
				s.RotationalVelocity = body.getOmega();
				s.id = id;
				return s;
			}
			set
			{
				body.setPositionOrientation(value.Position, value.Orientation);
				body.setVelocity(value.Velocity);
				body.setOmega(value.RotationalVelocity);
			}
		}

        public int ID
        {
            get { return id; }
        }

		public virtual void Dispose()
		{
			body.Dispose();
			body = null;
		}
	}
}

