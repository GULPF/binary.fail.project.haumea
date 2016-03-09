using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    /// <summary>
    /// Fills two purposes (SRP? split it when/if the events gets more sophisticated):
    /// - Keeps track of the current date
    /// - Triggers events based on date
    /// </summary>
    public class WorldDate : IView
    {
        private static Vector2 Pos = new Vector2(10, 10);

        // The smallest unit of time is a day, so there really isn't a point in messing around
        // with hours and such in the DateTime class.
        private readonly double _dayFrac;
        private SpriteFont _dateFont;
        private static readonly IList<string> MonthNames = new List<string>{
            "January", "February", "March", "April",
            "May", "June", "July", "August", "September",
            "October", "November", "December"
        };

        /// <summary>
        /// Indicates if the game is paused.
        /// </summary>
        public bool Frozen { get; }

        /// <summary>
        /// Some things needs to be checked every day-tick instead of every game-tick.
        /// By checking this prop in the <code>Update</code> method, it can be achieved.
        /// </summary>
        public bool IsNewDay { get; }

        /// <summary>
        /// Number of days that have passed since the game started.
        /// Useful when entities implement their own event-system.
        /// </summary>
        public long DaysPassed { get; }

        /// <summary>
        /// The current date.
        /// </summary>
        public DateTime Date { get; }

        public WorldDate(DateTime startDate)
        {
            _dayFrac = 0;

            Date = startDate;
            Frozen = false;
            IsNewDay = false;
            DaysPassed = 0;
        }

        private WorldDate(DateTime date, long daysPassed, bool isNewDay, bool frozen,
            double dayFrac, SpriteFont dateFont)
        {
            _dayFrac = dayFrac;
            _dateFont = dateFont;

            Date = date;
            DaysPassed = daysPassed;
            IsNewDay = isNewDay;
            Frozen = frozen;
        }

        public void Update(InputState input) {}

        public WorldDate Update(GameTime gameTime, int gameSpeed, InputState input)
        {
            if (Frozen) return this;

            bool freeze = input.WentActive(Keys.Space) ? !Frozen : Frozen;

            // RESEARCH: Some code is written with the assumption that `fulldays` is allways lower than 2.
            // ......... I think that's a reasonable assumption,
            // ......... so perhaps this method should also mbe written as such?
            double passedDays = 0.005 * gameSpeed * gameTime.ElapsedGameTime.TotalMilliseconds;
            int    fullDays    = (int)passedDays;

            double dayFrac = _dayFrac + passedDays - fullDays;

            if (dayFrac > 1)
            {
                fullDays++;
                dayFrac--;
            }

            bool isNewDay = fullDays > 0;

            DateTime date = Date;
            long daysPassed = DaysPassed;

            if (isNewDay)
            {
                date = date.AddDays(fullDays);
                daysPassed++;
            }

            return new WorldDate(date, daysPassed, isNewDay, freeze, dayFrac, _dateFont);
        }

        public void LoadContent(ContentManager content)
        {
            _dateFont = content.Load<SpriteFont>("test/LabelFont");
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            spriteBatch.DrawString(_dateFont, ToString(), Pos, Color.Black);
        }

        public override String ToString()
        {
            return (MonthNames[Date.Month - 1] + " ") + (Date.Day + " ") +
                ", " + Date.Year;
        }
    }
}


