using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {

    /// <summary>
    ///     Class to mesure execution time
    /// </summary>
    public class Timer {

        /******************** Attributes ********************/
        public int      duration     {get; set; }
        public DateTime endTime      {get; set; }
        public DateTime startTime    {get; set; }

        /******************** Constructors ********************/
        public Timer() {
            this.duration = 0;
        }

        /******************** Methods ********************/

        /// <summary>
        ///     Start the timer
        /// </summary>
        public void start() {
            this.startTime = DateTime.Now;
        }

        /// <summary>
        ///     End the time.
        /// </summary>
        public void stop() {
            this.endTime = DateTime.Now;
            this.duration = (this.endTime - this.startTime).Milliseconds;
        }

    }
}