using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Haumea_Core.Collections;
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

        private DateTime _date;
        // The smallest unit of time is a day, so there really isn't a point in messing around
        // with hours and such in the DateTime class.
        private double _dayFrac;
        private SpriteFont _dateFont;

        // TODO: Switch to priorityqueue?
        private IList<DateEvent> _listeners;

        public bool Frozen { get; set; }

        public WorldDate(ContentManager content, DateTime startDate)
        {
            _dateFont = content.Load<SpriteFont>("test/LabelFont");
            _date = startDate;
            _dayFrac = 0;
            _listeners = new SortedList<DateEvent>();
            Frozen = false;
        }

        public void Update(GameTime gameTime, int gameSpeed)
        {
            if (Frozen) return;

            double passedDays = 0.005 * gameSpeed * gameTime.ElapsedGameTime.TotalMilliseconds;
            int   fullDays    = (int)passedDays;

            _dayFrac += passedDays - fullDays;

            if (_dayFrac > 1)
            {
                fullDays++;
                _dayFrac--;
            }

            _date = _date.AddDays(fullDays);

            while (_listeners.Count > 0 &&_listeners[0].Trigger <= _date)
            {
                _listeners[0].Handler();
                _listeners.RemoveAt(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            spriteBatch.DrawString(_dateFont, ToString(), Pos, Color.Black);
        }

        public override String ToString()
        {
            return (_date.Day + " - ").PadLeft(5, '0') + (_date.Month + " - ").PadLeft(5, '0') + _date.Year;
        }

        public void AddEvent(DateTime trigger, Action handler)
        {
            if (trigger > _date)
            {
                _listeners.Add(new DateEvent(trigger, handler));    
            }
        }

        public void AddEvent(int years, int days, Action handler)
        {
            // TODO: This doesn't handle leap years?
            TimeSpan offset = new TimeSpan(365 * years + days, 0, 0, 0);
            _listeners.Add(new DateEvent(_date.Add(offset), handler));
        }

        public void AddEvent(int days, Action handler)
        {
            AddEvent(0, days, handler);
        }

        private class DateEvent : IComparable<DateEvent>
        {
            public DateTime Trigger { get; }
            public Action Handler { get; }

            public DateEvent(DateTime trigger, Action handler)
            {
                Trigger = trigger;
                Handler = handler;
            }

            public int CompareTo(DateEvent other)
            {
                return Trigger.CompareTo(other.Trigger);
            }
        }
    }
}


