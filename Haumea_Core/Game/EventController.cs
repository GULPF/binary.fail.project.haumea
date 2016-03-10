using System;
using System.Collections.Generic;

using Haumea_Core.Collections;

namespace Haumea_Core
{
    public class EventController : IEntity
    {

        // RESEARCH: Alright, Singletons are considered harmful and I shold know better etc.
        // What can be done to improve the situation? Just passing the EventController
        // in the constructor (dependency injection style) to the entities that needs is tempting,
        // but that can quickly spiral out of control. It is very likely that more dependencies like
        // this (dependencies that might be required by pretty much anything) will show up further down the road,
        // which will lead to some really nasty constructors.
        //
        // http://blog.ploeh.dk/2010/02/02/RefactoringtoAggregateServices/
        // This page has some good ideas that we might be able to apply later on.
        //
        // For now, it might be OK to just pass it in the constructor as is.
        // The only entity that uses it is Units, which currently has a single constructor argument,
        // but I know for a fact that pretty much all entities will need it sooner or later.
        //
        // Another soulution might be to introduce a IPublisher which collects the events,
        // and let the callee of Update() pass the events on. See the bottom of this class for an example.
        // That would mean that immutability goes out the window,
        // but on the other hand, immutable entities seems to be regarded as a bad idea in game development anyway.
        // That would essentielly be an implementation of the mediator pattern.
        // I don't like this soloution, because it introduces a lot of shit in the entities that I'd rather be without.
        public static EventController Instance { get; } = new EventController();

        private IList<DateEvent> _listeners;
        private WorldDate _currentDate;

        private EventController()
        {
            _listeners = new SortedList<DateEvent>();
        }

        public void AddEvent(DateTime trigger, Action handler)
        {
            if (trigger > _currentDate.Date)
            {
                _listeners.Add(new DateEvent(trigger, handler));    
            }
            else
            {
                // RESEARCH: I'm not certain this is a good idea. 
                // Without this, events will allways be called before __all__
                // other update methods, but if this happen,
                // the event will be called before some update methods and after some others.
                // It might be better to just discard events that has already occured.
                handler();
            }
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

        private interface IPublisher
        {
            IEnumerable<DateEvent> CollectEvents();
        }
    }
}

