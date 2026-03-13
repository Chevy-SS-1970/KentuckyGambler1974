using System;

abstract class CommandBase
{
    private string _command;
    public string Command
    {
        get { return _command; }
        set { _command = value; }
    }
    public virtual void Execute(string command)
    {
        Console.WriteLine(command);
    }
}

abstract class ConditionBase
{
    private string _condition;
    public string Condition
    {
        get { return _condition; }
        set { _condition = value; }
    }
    public virtual void AddCondition(string condition)
    {
        Console.WriteLine(condition);
    }
}

abstract class EffectBase
{
    private string _effect;
    public string Effect
    {
        get { return _effect; }
        set { _effect = value; }
    }
    public virtual void Apply(string effect)
    {
        Console.WriteLine(effect);
    }
}

abstract class GameEventBase
{
    private string _trigger;
    private string[] _effects_list;
    public string Trigger
    {
        get { return _trigger; }
        set { _trigger = value; }
    }
    public string[] Effects_list
    {
        get { return _effects_list; }
        set { _effects_list = value; }
    }
    public virtual void Check(string actual_trigger)
    {
        if (actual_trigger == _trigger)
        {
            Console.WriteLine(_effects_list);
        }
    }
}