using System;
using System.Collections.Generic;
using System.Text;

namespace Ymfas {
    class SafeTimer {
        private long timeStart;
        private long diffTime;
        public SafeTimer() {           
            timeStart = Environment.TickCount;
            diffTime = timeStart;
        }

        /// <summary>
        /// Current elapsed time in millis
        /// </summary>
        public long Time{
            get {
                return Environment.TickCount - timeStart;
            }         
        }
        /// <summary>
        /// The difference between the current time and the last time the diff was retrieved
        /// </summary>
        public long Diff {
            get {
                long nowTime = Environment.TickCount;
                long retval = nowTime - diffTime;
                diffTime = nowTime;
                return retval;
            }
        }
    }
}
