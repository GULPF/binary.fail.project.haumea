using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Haumea.Components
{
    /// <summary>
    /// Fills two purposes (SRP? split it when/if the events gets more sophisticated):
    /// - Keeps track of the current date
    /// - Triggers events based on date
    /// WorldDate is conceptually a model,
    /// but it can't implement IModel since WorldDate is the the input to IModel's update.
    /// </summary>
    public class WorldDate
    {
        // The smallest unit of time is a day, so there really isn't a point in messing around
        // with hours and such in the DateTime class.
        private double _dayRest;
        private static readonly IList<string> _monthNames = new List<string>{
            "January", "February", "March", "April",
            "May", "June", "July", "August", "September",
            "October", "November", "December"
        };

        /// <summary>
        /// Indicates if the game is paused.
        /// Can be updated to pause/unpause.
        /// </summary>
        public bool Frozen { get; set; }


        /// <summary>
        /// Speed modifier. Valid values are 1-5. Invalid values will be truncated.
        /// </summary>
        public int Speed
        {
            get
            {
                return _speed;
            }

            set
            {
                _speed = Math.Max(1, Math.Min(5, value));
            }
        }

        private int _speed = 1;


        /// <summary>
        /// Some things needs to be checked every day-tick instead of every game-tick.
        /// By checking this prop in the <code>Update</code> method, it can be achieved.
        /// </summary>
        public bool IsNewDay { get; private set; }

        /// <summary>
        /// Number of days that have passed since the game started.
        /// Useful when entities implement their own event-system.
        /// </summary>
        public long DaysPassed { get; private set; }

        /// <summary>
        /// The current date.
        /// </summary>
        public DateTime Date { get; private set; }

        private readonly float[] _speedLevels = { 1, 1.4f, 1.8f, 2.2f, 2.6f };

        public WorldDate(DateTime startDate)
        {
            _dayRest = 0;

            Date = startDate;
            Frozen = false;
            IsNewDay = false;
            DaysPassed = 0;
        }

        // The way this is implemented means that no more than one day can pass each tick.
        // If to much time has passed (which probably means something is wrong),
        // the missed days are added to dayRest. It might be better to just discard them.
        public void Update(GameTime gameTime)
        {
            if (Frozen) return;

            float speedLevel = _speedLevels[Speed - 1];
            _dayRest = _dayRest + 0.005 * speedLevel * gameTime.ElapsedGameTime.TotalMilliseconds;
            IsNewDay = _dayRest > 1;

            if (IsNewDay)
            {
                _dayRest--;
                Date = Date.AddDays(1);
                DaysPassed++;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}, {2}", _monthNames[Date.Month - 1], Date.Day, Date.Year);
        }
    }
}


