using System;
using Haumea.Components;

namespace Haumea.GameActions
{
    public enum ActionType
    {
        CreateEntity, ExitGame
    }

    public interface IGameAction
    {
        ActionType Type { get; }
    }

    public class SimpleAction
    {
        public ActionType Type { get; }

        public SimpleAction(ActionType type)
        {
            Type = type;
        }
    }

    public class CreateEntityAction
    {
        public ActionType Type { get; } = ActionType.CreateEntity;

        public IModel Model { get; }
        public IView  View  { get; }
    }
}

