using System;
using System.Collections.Generic;

//Основные классы
class Game { }
class GameState { }

// Интерфейсы
interface ICommand
{
    string Execute(string args, Game game);
}

interface IInteractable
{
    string Interact(GameState state);
}

interface ICondition
{
    bool Check(GameState state);
}

interface IEffect
{
    void Apply(GameState state);
}

// Абстрактные классы 
abstract class CommandBase : ICommand
{
    private string _commandName;
    public string CommandName
    {
        get { return _commandName; }
        set { _commandName = value; }
    }

    public abstract string Execute(string args, Game game);
}

abstract class ConditionBase: ICondition
{
    public abstract bool Check(GameState state);
}

abstract class EffectBase: IEffect
{
    public abstract void Apply(GameState state);
}

abstract class GameEventBase
{
    private ICondition _condition;
    private List<IEffect> _effects;

    public ICondition Condition
    {
        get { return _condition; }
        set { _condition = value; }
    }

    public List<IEffect> Effects
    {
        get { return _effects; }
        set { _effects = value; }
    }

    public void CheckAndApply(GameState state)
    {
        if (_condition != null && _condition.Check(state))
        {
            foreach (IEffect effect in _effects)
            {
                effect.Apply(state);
            }
        }
    }
}