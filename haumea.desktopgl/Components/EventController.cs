using System;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class EventController : IModel
    {
        private IList<DateEvent> _listeners;
        private WorldDate _currentDate;

        public EventController()
        {
            _listeners = new SortedList<DateEvent>();
        }

        public void AddEvent(DateTime trigger, Action handler)
        {
            Debug.Assert(trigger > _currentDate.Date, "Can't create event for a date that has already occured");
            _listeners.Add(new DateEvent(trigger, handler));    
        }

        public void Update(WorldDate date)
        {
            _currentDate = date;

            while (_listeners.Count > 0 &&_listeners[0].Trigger <= _currentDate.Date)
            {
                _listeners[0].Handler();
                _listeners.RemoveAt(0);
            }
        }

        public void AddEvent(int years, int days, Action handler)
        {
            AddEvent(_currentDate.Date.AddYears(years).AddDays(days), handler);
        }

        public void AddEvent(int days, Action handler)
        {
            AddEvent(0, days, handler);
        }

        private struct DateEvent : IComparable<DateEvent>
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