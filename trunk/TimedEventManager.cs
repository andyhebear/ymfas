using System;
using System.Collections.Generic;
using System.Text;
using Mogre;

namespace Ymfas
{
	class TimedEventManager
	{
		public delegate void TimedEventFunction(uint time, Object args);

		public enum TimedEventType
		{
			Repeating,
			Once
		};

		public struct TimedEvent
		{
			public event TimedEventFunction tef;
			public uint targetTime;
			public uint elapsedTime;
			public TimedEventType type;
			public Object args;

			public TimedEvent(TimedEventFunction _tef, TimedEventType _type, 
				uint _targetTime, Object _args)
			{
				tef = _tef; type = _type; 
				elapsedTime = 0;
				targetTime = _targetTime;
				args = _args;
			}

			public void Update(uint time)
			{
				elapsedTime += time;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns>returns true if the event should be
			/// removed</returns>
			public bool FireEvent()
			{
				if (elapsedTime > targetTime)
				{
					tef(elapsedTime, args);
					while (elapsedTime > targetTime)
						elapsedTime -= targetTime;

					if (type == TimedEventType.Once)
						return true;
				}

				return false;
			}
		}

		#region PrivateFields
		private Dictionary<int, TimedEvent> eventMap;
		private int idCounter = 0; 
		private Timer timer;
		#endregion


		public TimedEventManager()
		{
			eventMap = new Dictionary<int, TimedEvent>();
			timer = new Timer();
		}
		
		/// <summary>
		/// Add an event to the map
		/// </summary>
		/// <param name="ev"></param>
		/// <returns>unique id of the added event, negative if the adding the event failed</returns>
		public int AddEvent(TimedEvent ev)
		{
			// assign and return the unique id
			int eventId = ++idCounter;
			eventMap.Add(eventId, ev);

			return eventId;
		}

		public void RemoveEvent(int id)
		{
			eventMap.Remove(id);
		}

		public void Update()
		{
			// update the internal timer
			uint timeDelta = timer.Milliseconds;
			timer.Reset();

			// process all events
			List<int> removeList = new List<int>();
			foreach (KeyValuePair<int, TimedEvent> kvp in eventMap)
			{
				kvp.Value.Update(timeDelta);
				if (kvp.Value.FireEvent())
					removeList.Add(kvp.Key);
			}

			foreach (int rIdx in removeList)
				RemoveEvent(rIdx);
		}
	}
}
