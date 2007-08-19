using System;
using Mogre;
using MogreNewt;

namespace Ymfas
{
	public class Ship : IDisposable
	{
		protected Body body;
		protected string id;

        Vector3 standbyForce;
		Vector3 standbyTorque;

        const float FORCE = 10.0f;
        const float BOUNDING_RADIUS = 50.0f;
        const float MAX_X_TORQUE = 5.0f;
        const float MAX_Y_TORQUE = 5.0f;
        const float MAX_Z_TORQUE = 5.0f;

        //this is VERY temporary
        const float time = 1 / 60.0f;

		public Ship(World _w, string _id, Vector3 _position, Quaternion _orientation)
		{

			id = _id;

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
            //debugging
            float mass;
            Vector3 inertia;
            body.getMassMatrix(out mass, out inertia);
            b.addLocalForce(standbyForce, Vector3.ZERO);
            //standbyForce = Vector3.ZERO;

            Vector3 pos = new Vector3();
            Quaternion orient = new Quaternion();
            b.getPositionOrientation(out pos, out orient);
            
            b.addTorque(orient * standbyTorque);
			//standbyTorque = Vector3.ZERO;
        }

		public void InertiaCheck()
		{
			Vector3 pos; Quaternion orient;
			body.getPositionOrientation(out pos, out orient);
			float mass; Vector3 massMatrix;
			body.getMassMatrix(out mass, out massMatrix);
		}

		// thrust is measured in meters / s^2
		public void ThrustRelative(Vector3 vec)
		{
            standbyForce = vec * FORCE * BOUNDING_RADIUS;
		}
		public void TorqueRelative(Vector3 vec)
		{
            standbyTorque = new Vector3(vec.x * MAX_X_TORQUE, vec.y * MAX_Y_TORQUE, vec.z * MAX_Z_TORQUE) * BOUNDING_RADIUS;
		}


        public Vector3 GetCorrectiveTorque()
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


            Vector3 correctiveTorque = new Vector3();

            //x correction

            if(torque.x > MAX_X_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.x = -1;
            }
            else if (torque.x < -MAX_X_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.x = 1;
            }
            else
            {
                correctiveTorque.x = -torque.x / MAX_X_TORQUE / BOUNDING_RADIUS;
            }
            //y correction

            if (torque.y > MAX_Y_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.y = -1;
            }
            else if (torque.y < -MAX_Y_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.y = 1;
            }
            else
            {
                correctiveTorque.y = -torque.y / MAX_Y_TORQUE / BOUNDING_RADIUS;
            }
            //z correction
            if (torque.z > MAX_Z_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.z = -1;
            }
            else if (torque.z < -MAX_Z_TORQUE * BOUNDING_RADIUS)
            {
                correctiveTorque.z = 1;
            }
            else
            {
                correctiveTorque.z = -torque.z / MAX_Z_TORQUE / BOUNDING_RADIUS;
            }

            return correctiveTorque;
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
				Vector3 pos;
				Quaternion orient;
				body.getPositionOrientation(out s.Position, out s.Orientation);
				s.Velocity = body.getVelocity();
				s.RotationalVelocity = body.getOmega();
				return s;
			}
			set
			{
				body.setPositionOrientation(value.Position, value.Orientation);
				body.setVelocity(value.Velocity);
				body.setOmega(value.RotationalVelocity);
			}
		}

        public string ID
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
