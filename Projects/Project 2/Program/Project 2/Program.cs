using System;
using System.Collections.Generic;

namespace SimpleTextRPG
{
    // ==================================== ИНТЕРФЕЙСЫ ====================
    
    public interface ICommand
    {
        string Name { get; }
        string Execute(string[] args, Game game);
    }

    public interface IInteractable
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string Interact(GameState state, Game game);
    }

    public interface ICondition
    {
        bool Check(GameState state);
    }

    public interface IEffect
    {
        void Apply(GameState state, Game game);
    }

    // ============================ АБСТРАКТНЫЕ КЛАССЫ ====================
    
    public abstract class CommandBase : ICommand
    {
        private string _name;
        
        public string Name
        {
            get { return _name; }
        }
        
        public CommandBase(string name)
        {
            _name = name;
        }
        
        public abstract string Execute(string[] args, Game game);
    }

    public abstract class ConditionBase : ICondition
    {
        public abstract bool Check(GameState state);
    }

    public abstract class EffectBase : IEffect
    {
        public abstract void Apply(GameState state, Game game);
    }

    public abstract class GameEventBase
    {
        private string _name;
        private ICondition _condition;
        private List<IEffect> _effects;
        private bool _oneTime;
        private bool _hasTriggered;
        
        public string Name
        {
            get { return _name; }
        }
        
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
        
        public bool OneTime
        {
            get { return _oneTime; }
            set { _oneTime = value; }
        }
        
        public GameEventBase(string name)
        {
            _name = name;
            _effects = new List<IEffect>();
            _oneTime = false;
            _hasTriggered = false;
        }
        
        public bool ShouldTrigger(GameState state)
        {
            if (_oneTime && _hasTriggered)
            {
                return false;
            }
            
            if (_condition == null)
            {
                return true;
            }
            
            return _condition.Check(state);
        }
        
        public void Trigger(GameState state, Game game)
        {
            if (!ShouldTrigger(state))
            {
                return;
            }
            
            _hasTriggered = true;
            
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(state, game);
            }
            
            ThisIsBroken(state, game);
        }
        
        public void ThisIsBroken(GameState state, Game game)
        {
            // Пустой метод для отладки через наследника и тестирования в обход триггеров (читы)
        }
    }

    // ==================== СОСТОЯИЕ КВЕСТА ==========================
    
    public enum QuestState //Пронумерованные константы
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    // ==================== КЛАСС КВЕССТА ==================================
    
    public class Quest
    {
        private string _id;
        private string _name;
        private string _description;
        private QuestState _state;
        private ICondition _startCondition;
        private ICondition _completeCondition;
        private ICondition _failCondition;
        private List<IEffect> _onStartEffects;
        private List<IEffect> _onCompleteEffects;
        private List<IEffect> _onFailEffects;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public QuestState State
        {
            get { return _state; }
        }
        
        public ICondition StartCondition
        {
            get { return _startCondition; }
            set { _startCondition = value; }
        }
        
        public ICondition CompleteCondition
        {
            get { return _completeCondition; }
            set { _completeCondition = value; }
        }
        
        public ICondition FailCondition
        {
            get { return _failCondition; }
            set { _failCondition = value; }
        }
        
        public List<IEffect> OnStartEffects
        {
            get { return _onStartEffects; }
            set { _onStartEffects = value; }
        }
        
        public List<IEffect> OnCompleteEffects
        {
            get { return _onCompleteEffects; }
            set { _onCompleteEffects = value; }
        }
        
        public List<IEffect> OnFailEffects
        {
            get { return _onFailEffects; }
            set { _onFailEffects = value; }
        }
        
        public Quest(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _state = QuestState.NotStarted;
            _onStartEffects = new List<IEffect>();
            _onCompleteEffects = new List<IEffect>();
            _onFailEffects = new List<IEffect>();
        }
        
        public void Check(GameState state, Game game)
        {
            if (_state == QuestState.NotStarted)
            {
                bool canStart = true;
                if (_startCondition != null)
                {
                    canStart = _startCondition.Check(state);
                }
                
                if (canStart)
                {
                    _state = QuestState.InProgress;
                    for (int i = 0; i < _onStartEffects.Count; i++)
                    {
                        _onStartEffects[i].Apply(state, game);
                    }
                    state.AddLog("Квест начат: " + _name);
                }
            }
            else if (_state == QuestState.InProgress)
            {
                bool isFailed = false;
                if (_failCondition != null)
                {
                    isFailed = _failCondition.Check(state);
                }
                
                if (isFailed)
                {
                    _state = QuestState.Failed;
                    for (int i = 0; i < _onFailEffects.Count; i++)
                    {
                        _onFailEffects[i].Apply(state, game);
                    }
                    state.AddLog("Квест провален: " + _name);
                }
                else
                {
                    bool isCompleted = true;
                    if (_completeCondition != null)
                    {
                        isCompleted = _completeCondition.Check(state);
                    }
                    
                    if (isCompleted)
                    {
                        _state = QuestState.Completed;
                        for (int i = 0; i < _onCompleteEffects.Count; i++)
                        {
                            _onCompleteEffects[i].Apply(state, game);
                        }
                        state.AddLog("Квест завершён: " + _name);
                    }
                }
            }
        }
        
        public override string ToString()
        {
            string status = "";
            if (_state == QuestState.NotStarted) status = "[Не начат]";
            else if (_state == QuestState.InProgress) status = "[В процессе]";
            else if (_state == QuestState.Completed) status = "[Завершён]";
            else if (_state == QuestState.Failed) status = "[Провален]";
            
            return status + " " + _name + " - " + _description;
        }
    }

    // ==================== КЛАСС GAMESTATE ====================
    
    public class GameState
    {
        private int _health;
        private int _maxHealth;
        private List<string> _inventory;
        private Dictionary<string, bool> _flags;
        private Dictionary<string, int> _counters;
        private int _turnCount;
        private List<string> _log;
        private List<Quest> _quests;
        private string _pendingLocationChange;
        
        public int Health
        {
            get { return _health; }
        }
        
        public int MaxHealth
        {
            get { return _maxHealth; }
        }
        
        public List<string> Inventory
        {
            get { return _inventory; }
        }
        
        public Dictionary<string, bool> Flags
        {
            get { return _flags; }
        }
        
        public Dictionary<string, int> Counters
        {
            get { return _counters; }
        }
        
        public int TurnCount
        {
            get { return _turnCount; }
        }
        
        public List<string> Log
        {
            get { return _log; }
        }
        
        public List<Quest> Quests
        {
            get { return _quests; }
        }
        
        public string PendingLocationChange
        {
            get { return _pendingLocationChange; }
        }
        
        public bool IsAlive
        {
            get { return _health > 0; }
        }
        
        public GameState()
        {
            _health = 100;
            _maxHealth = 100;
            _inventory = new List<string>();
            _flags = new Dictionary<string, bool>();
            _counters = new Dictionary<string, int>();
            _turnCount = 0;
            _log = new List<string>();
            _quests = new List<Quest>();
            _pendingLocationChange = null;
        }
        
        public void AddItem(string itemId)
        {
            bool hasItem = false;
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] == itemId)
                {
                    hasItem = true;
                    break;
                }
            }
            
            if (!hasItem)
            {
                _inventory.Add(itemId);
            }
        }
        
        public void RemoveItem(string itemId)
        {
            for (int i = _inventory.Count - 1; i >= 0; i--)
            {
                if (_inventory[i] == itemId)
                {
                    _inventory.RemoveAt(i);
                    break;
                }
            }
        }
        
        public bool HasItem(string itemId)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] == itemId)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void SetFlag(string flag, bool value)
        {
            _flags[flag] = value;
        }
        
        public bool GetFlag(string flag, bool defaultValue)
        {
            if (_flags.ContainsKey(flag))
            {
                return _flags[flag];
            }
            return defaultValue;
        }
        
        public bool GetFlag(string flag)
        {
            return GetFlag(flag, false);
        }
        
        public void SetCounter(string counter, int value)
        {
            _counters[counter] = value;
        }
        
        public int GetCounter(string counter, int defaultValue)
        {
            if (_counters.ContainsKey(counter))
            {
                return _counters[counter];
            }
            return defaultValue;
        }
        
        public int GetCounter(string counter)
        {
            return GetCounter(counter, 0);
        }
        
        public void IncrementCounter(string counter)
        {
            int current = GetCounter(counter);
            _counters[counter] = current + 1;
        }
        
        public void TakeDamage(int damage)
        {
            _health = _health - damage;
            if (_health < 0)
            {
                _health = 0;
            }
            AddLog("Получен урон: " + damage + ". Здоровье: " + _health + "/" + _maxHealth);
            
            if (_health <= 0)
            {
                AddLog("ВЫ ПОГИБЛИ!");
            }
        }
        
        public void Heal(int amount)
        {
            _health = _health + amount;
            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }
            AddLog("Восстановлено здоровья: " + amount + ". Здоровье: " + _health + "/" + _maxHealth);
        }
        
        public void AddLog(string message)
        {
            _log.Add("[Ход " + _turnCount + "] " + message);
        }
        
        public void IncrementTurn()
        {
            _turnCount = _turnCount + 1;
        }
        
        public void AddQuest(Quest quest)
        {
            _quests.Add(quest);
        }
        
        public Quest GetQuest(string questId)
        {
            for (int i = 0; i < _quests.Count; i++)
            {
                if (_quests[i].Id == questId)
                {
                    return _quests[i];
                }
            }
            return null;
        }
        
        public void RequestLocationChange(string locationId)
        {
            _pendingLocationChange = locationId;
        }
        
        public void ClearPendingLocationChange()
        {
            _pendingLocationChange = null;
        }
    }

    // ==================== КЛАСС LOCATION ====================
    
    public class Location
    {
        private string _id;
        private string _name;
        private string _description;
        private List<IInteractable> _interactables;
        private List<GameEventBase> _events;
        private Dictionary<string, Exit> _exits;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public List<IInteractable> Interactables
        {
            get { return _interactables; }
        }
        
        public List<GameEventBase> Events
        {
            get { return _events; }
        }
        
        public Dictionary<string, Exit> Exits
        {
            get { return _exits; }
        }
        
        public Location(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _interactables = new List<IInteractable>();
            _events = new List<GameEventBase>();
            _exits = new Dictionary<string, Exit>();
        }
        
        public void AddExit(string direction, string targetLocationId, ICondition condition)
        {
            Exit exit = new Exit();
            exit.Direction = direction;
            exit.TargetLocationId = targetLocationId;
            exit.Condition = condition;
            
            _exits[direction.ToLower()] = exit;
        }
        
        public void AddExit(string direction, string targetLocationId)
        {
            AddExit(direction, targetLocationId, null);
        }
        
        public void AddInteractable(IInteractable interactable)
        {
            _interactables.Add(interactable);
        }
        
        public void AddEvent(GameEventBase gameEvent)
        {
            _events.Add(gameEvent);
        }
        
        public void RemoveInteractable(IInteractable interactable)
        {
            for (int i = _interactables.Count - 1; i >= 0; i--)
            {
                if (_interactables[i] == interactable)
                {
                    _interactables.RemoveAt(i);
                    break;
                }
            }
        }
        
        public string GetExitDestination(string direction, GameState state)
        {
            string key = direction.ToLower();
            
            if (_exits.ContainsKey(key))
            {
                Exit exit = _exits[key];
                
                bool canPass = true;
                if (exit.Condition != null)
                {
                    canPass = exit.Condition.Check(state);
                }
                
                if (canPass)
                {
                    return exit.TargetLocationId;
                }
            }
            
            return null;
        }
        
        public void TriggerEvents(GameState state, Game game)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Trigger(state, game);
            }
        }
        
        public string GetDescription()
        {
            string desc = "\n=== " + _name + " ===\n" + _description + "\n";
            
            if (_interactables.Count > 0)
            {
                desc += "\nВы видите:";
                for (int i = 0; i < _interactables.Count; i++)
                {
                    // Показываем имя с ID в скобках для удобства игрока
                    desc += "\n  - " + _interactables[i].Name + " (" + _interactables[i].Id + ")";
                    desc += "\n    " + _interactables[i].Description;
                }
            }
            
            if (_exits.Count > 0)
            {
                desc += "\n\nВыходы:";
                foreach (KeyValuePair<string, Exit> kvp in _exits)
                {
                    desc += "\n  - " + kvp.Value.Direction;
                }
            }
            
            return desc;
        }
        
        public IInteractable FindInteractable(string id)
        {
            string lowerId = id.ToLower();
            
            for (int i = 0; i < _interactables.Count; i++)
            {
                if (_interactables[i].Id.ToLower() == lowerId || 
                    _interactables[i].Name.ToLower().Contains(lowerId))
                {
                    return _interactables[i];
                }
            }
            
            return null;
        }
        
        public class Exit
        {
            public string Direction { get; set; }
            public string TargetLocationId { get; set; }
            public ICondition Condition { get; set; }
        }
    }

    // ==================== КЛАСС GAME ====================
    
    public class Game
    {
        private Dictionary<string, ICommand> _commands;
        private Dictionary<string, Location> _locations;
        private List<GameEventBase> _globalEvents;
        private GameState _state;
        private Location _currentLocation;
        private bool _isRunning;
        
        public GameState State
        {
            get { return _state; }
        }
        
        public Location CurrentLocation
        {
            get { return _currentLocation; }
        }
        
        public bool IsRunning
        {
            get { return _isRunning; }
        }
        
        public Game()
        {
            _commands = new Dictionary<string, ICommand>();
            _locations = new Dictionary<string, Location>();
            _globalEvents = new List<GameEventBase>();
            _state = new GameState();
            _isRunning = true;
        }
        
        public void RegisterCommand(ICommand command)
        {
            _commands[command.Name.ToLower()] = command;
        }
        
        public void RegisterLocation(Location location)
        {
            _locations[location.Id] = location;
        }
        
        public void RegisterGlobalEvent(GameEventBase gameEvent)
        {
            _globalEvents.Add(gameEvent);
        }
        
        public bool ChangeLocation(string locationId)
        {
            if (_locations.ContainsKey(locationId))
            {
                _currentLocation = _locations[locationId];
                _state.AddLog("Переход в локацию: " + _currentLocation.Name);
                _currentLocation.TriggerEvents(_state, this);
                return true;
            }
            return false;
        }
        
        public IInteractable FindInteractable(string id)
        {
            if (_currentLocation == null)
            {
                return null;
            }
            return _currentLocation.FindInteractable(id);
        }
        
        private void ProcessPendingLocationChange() // Отложенная смена локации
        {
            if (_state.PendingLocationChange != null)
            {
                ChangeLocation(_state.PendingLocationChange);
                _state.ClearPendingLocationChange();
            }
        }
        
        private void UpdateQuests()
        {
            for (int i = 0; i < _state.Quests.Count; i++)
            {
                _state.Quests[i].Check(_state, this);
            }
        }
        
        private void CheckGameOver()
        {
            if (!_state.IsAlive)
            {
                Console.WriteLine("\n=== ИГРА ОКОНЧЕНА ===");
                Console.WriteLine("Вы погибли...");
                _isRunning = false;
            }
        }
        
        public void Run()
        {
            Console.WriteLine("=== ТЕКСТОВАЯ RPG ===");
            Console.WriteLine("Введите 'help' для списка команд\n");
            
            while (_isRunning)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                
                string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string commandName = parts[0].ToLower();
                
                string[] args = new string[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                {
                    args[i - 1] = parts[i];
                }
                
                if (commandName == "quit")
                {
                    Console.WriteLine("До свидания!");
                    break;
                }
                
                if (_commands.ContainsKey(commandName))
                {
                    ICommand command = _commands[commandName];
                    string result = command.Execute(args, this);
                    
                    if (!string.IsNullOrEmpty(result))
                    {
                        Console.WriteLine(result);
                    }
                    
                    _state.IncrementTurn();
                    
                    for (int i = 0; i < _globalEvents.Count; i++)
                    {
                        _globalEvents[i].Trigger(_state, this);
                    }
                    
                    if (_currentLocation != null)
                    {
                        _currentLocation.TriggerEvents(_state, this);
                    }
                    
                    ProcessPendingLocationChange();
                    UpdateQuests();
                    CheckGameOver();
                }
                else
                {
                    Console.WriteLine("Неизвестная команда. Введите 'help' для списка команд.");
                }
            }
        }
    }

    // ==================== КОМАНДЫ ====================
    
    public class LookCommand : CommandBase
    {
        public LookCommand() : base("look") { }
        
        public override string Execute(string[] args, Game game)
        {
            if (game.CurrentLocation == null)
            {
                return "Локация не задана.";
            }
            return game.CurrentLocation.GetDescription();
        }
    }
    
    public class GoCommand : CommandBase
    {
        public GoCommand() : base("go") { }
        
        public override string Execute(string[] args, Game game)
        {
            if (args.Length == 0)
            {
                return "Укажите направление. Пример: go north";
            }
            
            string direction = args[0].ToLower();
            
            if (game.CurrentLocation == null)
            {
                return "Локация не задана.";
            }
            
            string destinationId = game.CurrentLocation.GetExitDestination(direction, game.State);
            
            if (destinationId == null)
            {
                return "Вы не можете пойти в этом направлении.";
            }
            
            bool success = game.ChangeLocation(destinationId);
            if (success)
            {
                return "";
            }
            return "Ошибка перехода.";
        }
    }
    
    public class InteractCommand : CommandBase
    {
        public InteractCommand() : base("interact") { }
        
        public override string Execute(string[] args, Game game)
        {
            if (args.Length == 0)
            {
                return "Укажите объект для взаимодействия. Пример: interact chest";
            }
            
            string targetId = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) targetId += " ";
                targetId += args[i];
            }
            
            IInteractable interactable = game.FindInteractable(targetId);
            
            if (interactable == null)
            {
                return "Здесь нет такого объекта.";
            }
            
            return interactable.Interact(game.State, game);
        }
    }
    
    public class InventoryCommand : CommandBase
    {
        public InventoryCommand() : base("inventory") { }
        
        public override string Execute(string[] args, Game game)
        {
            if (game.State.Inventory.Count == 0)
            {
                return "Ваш инвентарь пуст.";
            }
            
            string result = "Инвентарь:";
            for (int i = 0; i < game.State.Inventory.Count; i++)
            {
                result += "\n  - " + game.State.Inventory[i];
            }
            return result;
        }
    }
    
    public class StatusCommand : CommandBase
    {
        public StatusCommand() : base("status") { }
        
        public override string Execute(string[] args, Game game)
        {
            string status = "Здоровье: " + game.State.Health + "/" + game.State.MaxHealth + "\n";
            status += "Ход: " + game.State.TurnCount + "\n";
            
            if (game.CurrentLocation != null)
            {
                status += "Локация: " + game.CurrentLocation.Name + "\n";
            }
            
            status += "Предметов в инвентаре: " + game.State.Inventory.Count;
            return status;
        }
    }
    
    public class HealthCommand : CommandBase
    {
        public HealthCommand() : base("health") { }
        
        public override string Execute(string[] args, Game game)
        {
            int health = game.State.Health;
            int maxHealth = game.State.MaxHealth;
            int percent = (int)((float)health / maxHealth * 20);
            
            string bar = "[";
            for (int i = 0; i < 20; i++)
            {
                if (i < percent)
                {
                    bar += "#";
                }
                else
                {
                    bar += ".";
                }
            }
            bar += "]";
            
            return "Здоровье: " + bar + " " + health + "/" + maxHealth;
        }
    }
    
    public class QuestsCommand : CommandBase
    {
        public QuestsCommand() : base("quests") { }
        
        public override string Execute(string[] args, Game game)
        {
            List<Quest> activeQuests = new List<Quest>();
            
            for (int i = 0; i < game.State.Quests.Count; i++)
            {
                if (game.State.Quests[i].State == QuestState.InProgress)
                {
                    activeQuests.Add(game.State.Quests[i]);
                }
            }
            
            if (activeQuests.Count == 0)
            {
                return "Нет активных квестов.";
            }
            
            string result = "Активные квесты:";
            for (int i = 0; i < activeQuests.Count; i++)
            {
                result += "\n  " + activeQuests[i].ToString();
            }
            
            return result;
        }
    }
    
    public class LogCommand : CommandBase
    {
        public LogCommand() : base("log") { }
        
        public override string Execute(string[] args, Game game)
        {
            int count = 10;
            
            if (args.Length > 0)
            {
                int.TryParse(args[0], out count);
            }
            
            int startIndex = game.State.Log.Count - count;
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            
            if (game.State.Log.Count == 0)
            {
                return "Журнал пуст.";
            }
            
            string result = "";
            for (int i = startIndex; i < game.State.Log.Count; i++)
            {
                if (i > startIndex)
                {
                    result += "\n";
                }
                result += game.State.Log[i];
            }
            
            return result;
        }
    }
    
    public class HelpCommand : CommandBase
    {
        public HelpCommand() : base("help") { }
        
        public override string Execute(string[] args, Game game)
        {
            return "Доступные команды:\n" +
                   "  look - осмотреться\n" +
                   "  go [направление] - идти\n" +
                   "  interact [объект] - взаимодействовать\n" +
                   "  inventory - инвентарь\n" +
                   "  status - состояние\n" +
                   "  health - здоровье\n" +
                   "  quests - квесты\n" +
                   "  log [количество] - журнал событий\n" +
                   "  help - справка\n" +
                   "  quit - выход";
        }
    }

    // ==================== УСЛОВИЯ ====================
    
    public enum ComparisonType
    {
        Greater,
        Less,
        Equal,
        GreaterOrEqual,
        LessOrEqual
    }
    
    public class HasItemCondition : ConditionBase
    {
        private string _itemId;
        
        public HasItemCondition(string itemId)
        {
            _itemId = itemId;
        }
        
        public override bool Check(GameState state)
        {
            return state.HasItem(_itemId);
        }
    }
    
    public class FlagCondition : ConditionBase
    {
        private string _flag;
        private bool _expectedValue;
        
        public FlagCondition(string flag, bool expectedValue)
        {
            _flag = flag;
            _expectedValue = expectedValue;
        }
        
        public FlagCondition(string flag) : this(flag, true)
        {
        }
        
        public override bool Check(GameState state)
        {
            return state.GetFlag(_flag) == _expectedValue;
        }
    }
    
    public class HealthCondition : ConditionBase
    {
        private int _threshold;
        private ComparisonType _comparison;
        
        public HealthCondition(int threshold, ComparisonType comparison)
        {
            _threshold = threshold;
            _comparison = comparison;
        }
        
        public override bool Check(GameState state)
        {
            if (_comparison == ComparisonType.Greater)
            {
                return state.Health > _threshold;
            }
            else if (_comparison == ComparisonType.Less)
            {
                return state.Health < _threshold;
            }
            else if (_comparison == ComparisonType.Equal)
            {
                return state.Health == _threshold;
            }
            else if (_comparison == ComparisonType.GreaterOrEqual)
            {
                return state.Health >= _threshold;
            }
            else if (_comparison == ComparisonType.LessOrEqual)
            {
                return state.Health <= _threshold;
            }
            
            return false;
        }
    }
    
    public class AndCondition : ConditionBase
    {
        private List<ICondition> _conditions;
        
        public AndCondition(params ICondition[] conditions)
        {
            _conditions = new List<ICondition>();
            for (int i = 0; i < conditions.Length; i++)
            {
                _conditions.Add(conditions[i]);
            }
        }
        
        public override bool Check(GameState state)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (!_conditions[i].Check(state))
                {
                    return false;
                }
            }
            return true;
        }
    }
    
    public class OrCondition : ConditionBase
    {
        private List<ICondition> _conditions;
        
        public OrCondition(params ICondition[] conditions)
        {
            _conditions = new List<ICondition>();
            for (int i = 0; i < conditions.Length; i++)
            {
                _conditions.Add(conditions[i]);
            }
        }
        
        public override bool Check(GameState state)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].Check(state))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    public class NotCondition : ConditionBase
    {
        private ICondition _condition;
        
        public NotCondition(ICondition condition)
        {
            _condition = condition;
        }
        
        public override bool Check(GameState state)
        {
            return !_condition.Check(state);
        }
    }

    // ==================== ЭФФЕКТЫ ====================
    
    public class AddItemEffect : EffectBase
    {
        private string _itemId;
        private string _message;
        
        public AddItemEffect(string itemId, string message)
        {
            _itemId = itemId;
            _message = message;
        }
        
        public AddItemEffect(string itemId) : this(itemId, "Получен предмет: " + itemId)
        {
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.AddItem(_itemId);
            state.AddLog(_message);
        }
    }
    
    public class RemoveItemEffect : EffectBase
    {
        private string _itemId;
        
        public RemoveItemEffect(string itemId)
        {
            _itemId = itemId;
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.RemoveItem(_itemId);
            state.AddLog("Потерян предмет: " + _itemId);
        }
    }
    
    public class SetFlagEffect : EffectBase
    {
        private string _flag;
        private bool _value;
        
        public SetFlagEffect(string flag, bool value)
        {
            _flag = flag;
            _value = value;
        }
        
        public SetFlagEffect(string flag) : this(flag, true)
        {
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.SetFlag(_flag, _value);
        }
    }
    
    public class DamageEffect : EffectBase
    {
        private int _damage;
        
        public DamageEffect(int damage)
        {
            _damage = damage;
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.TakeDamage(_damage);
        }
    }
    
    public class HealEffect : EffectBase
    {
        private int _amount;
        
        public HealEffect(int amount)
        {
            _amount = amount;
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.Heal(_amount);
        }
    }
    
    public class LogEffect : EffectBase
    {
        private string _message;
        
        public LogEffect(string message)
        {
            _message = message;
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.AddLog(_message);
        }
    }
    
    public class AddExitEffect : EffectBase
    {
        private string _direction;
        private string _targetLocationId;
        private ICondition _condition;
        
        public AddExitEffect(string direction, string targetLocationId, ICondition condition)
        {
            _direction = direction;
            _targetLocationId = targetLocationId;
            _condition = condition;
        }
        
        public AddExitEffect(string direction, string targetLocationId) 
            : this(direction, targetLocationId, null)
        {
        }
        
        public override void Apply(GameState state, Game game)
        {
            if (game.CurrentLocation != null)
            {
                game.CurrentLocation.AddExit(_direction, _targetLocationId, _condition);
            }
        }
    }
    
    public class ChangeLocationEffect : EffectBase
    {
        private string _locationId;
        
        public ChangeLocationEffect(string locationId)
        {
            _locationId = locationId;
        }
        
        public override void Apply(GameState state, Game game)
        {
            state.RequestLocationChange(_locationId);
        }
    }

    // ==================== ОБЪЕКТЫ ВЗАИМОДЕЙСТВИЯ ====================
    
    public class Chest : IInteractable
    {
        private string _id;
        private string _name;
        private string _description;
        private ICondition _lockCondition;
        private List<IEffect> _effects;
        private bool _oneTime;
        private bool _opened;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public ICondition LockCondition
        {
            get { return _lockCondition; }
            set { _lockCondition = value; }
        }
        
        public List<IEffect> Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }
        
        public bool OneTime
        {
            get { return _oneTime; }
            set { _oneTime = value; }
        }
        
        public Chest(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _effects = new List<IEffect>();
            _oneTime = true;
            _opened = false;
        }
        
        public string Interact(GameState state, Game game)
        {
            if (_oneTime && _opened)
            {
                return "Сундук уже открыт.";
            }
            
            if (_lockCondition != null && !_lockCondition.Check(state))
            {
                return "Сундук заперт.";
            }
            
            _opened = true;
            
            // ПРИМЕНЯЕМ ЭФФЕКТЫ СУНДУКА
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(state, game);
            }
            
            return "Вы открыли сундук!";
        }
    }
    
    public class Door : IInteractable
    {
        private string _id;
        private string _name;
        private string _description;
        private ICondition _unlockCondition;
        private List<IEffect> _effects;
        private bool _unlocked;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public ICondition UnlockCondition
        {
            get { return _unlockCondition; }
            set { _unlockCondition = value; }
        }
        
        public List<IEffect> Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }
        
        public Door(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _effects = new List<IEffect>();
            _unlocked = false;
        }
        
        public string Interact(GameState state, Game game)
        {
            if (_unlocked)
            {
                return "Дверь уже открыта.";
            }
            
            if (_unlockCondition != null && !_unlockCondition.Check(state))
            {
                return "Дверь заперта. Нужен ключ.";
            }
            
            _unlocked = true;
            
            // ПРИМЕНЯЕМ ЭФФЕКТЫ ДВЕРИ
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(state, game);
            }
            
            return "Вы открыли дверь!";
        }
    }
    
    public class NPC : IInteractable
    {
        private string _id;
        private string _name;
        private string _description;
        private string _dialogue;
        private ICondition _dialogueCondition;
        private List<IEffect> _effects;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public string Dialogue
        {
            get { return _dialogue; }
            set { _dialogue = value; }
        }
        
        public ICondition DialogueCondition
        {
            get { return _dialogueCondition; }
            set { _dialogueCondition = value; }
        }
        
        public List<IEffect> Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }
        
        public NPC(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _effects = new List<IEffect>();
        }
        
        public string Interact(GameState state, Game game)
        {
            if (_dialogueCondition != null && !_dialogueCondition.Check(state))
            {
                return _name + " не хочет с вами говорить.";
            }
            
            // ПРИМЕНЯЕМ ЭФФЕКТЫ NPC
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(state, game);
            }
            
            if (string.IsNullOrEmpty(_dialogue))
            {
                return _name + " молчит.";
            }
            
            return _name + ": \"" + _dialogue + "\"";
        }
    }
    
    // Плита-ловушка (без слова trap в ID)
    public class PressurePlate : IInteractable
    {
        private string _id;
        private string _name;
        private string _description;
        private List<IEffect> _effects;
        private bool _triggered;
        
        public string Id
        {
            get { return _id; }
        }
        
        public string Name
        {
            get { return _name; }
        }
        
        public string Description
        {
            get { return _description; }
        }
        
        public List<IEffect> Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }
        
        public PressurePlate(string id, string name, string description)
        {
            _id = id;
            _name = name;
            _description = description;
            _effects = new List<IEffect>();
            _triggered = false;
        }
        
        public string Interact(GameState state, Game game)
        {
            if (_triggered)
            {
                return "Механизм уже сработал.";
            }
            
            _triggered = true;
            
            // ПРИМЕНЯЕМ ЭФФЕКТЫ ЛОВУШКИ
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(state, game);
            }
            
            // Удаляем плиту из локации после срабатывания
            if (game.CurrentLocation != null)
            {
                game.CurrentLocation.RemoveInteractable(this);
            }
            
            return "Вы наступили на плиту...";
        }
    }

    // ==================== СОБЫТИЯ ====================
    
    public class OnEnterLocationEvent : GameEventBase
    {
        public OnEnterLocationEvent() : base("OnEnterLocation")
        {
        }
    }
    
    public class OnTurnEvent : GameEventBase
    {
        public OnTurnEvent() : base("OnTurn")
        {
        }
    }

    // ==================== ТОЧКА ВХОДА ====================
    
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            
            // Регистрация команд
            game.RegisterCommand(new LookCommand());
            game.RegisterCommand(new GoCommand());
            game.RegisterCommand(new InteractCommand());
            game.RegisterCommand(new InventoryCommand());
            game.RegisterCommand(new StatusCommand());
            game.RegisterCommand(new HealthCommand());
            game.RegisterCommand(new QuestsCommand());
            game.RegisterCommand(new LogCommand());
            game.RegisterCommand(new HelpCommand());
            
            // Создание локаций
            Location startRoom = new Location("start", "Тёмная комната", 
                "Вы находитесь в тёмной сырой комнате. Слабый свет пробивается из-под двери на севере.");
            Location hallway = new Location("hallway", "Коридор", 
                "Длинный коридор с факелами на стенах. На востоке виднеется массивная дверь.");
            Location treasureRoom = new Location("treasure", "Сокровищница", 
                "Великолепная комната, полная золота и драгоценностей! Вы нашли легендарные сокровища!");
            
            // Добавление выходов
            startRoom.AddExit("north", "hallway");
            hallway.AddExit("south", "start");
            hallway.AddExit("east", "treasure", new HasItemCondition("gold_key"));
            
            // Создание сундука с золотым ключом
            Chest chest = new Chest("chest", "Старый сундук", "Деревянный сундук с металлическими уголками");
            chest.Effects.Add(new AddItemEffect("gold_key", "В сундуке лежит золотой ключ!"));
            
            // Создание ящика с ржавым ключом
            Chest rustyChest = new Chest("box", "Маленький ящик", "Небольшой деревянный ящик в углу");
            rustyChest.Effects.Add(new AddItemEffect("rusty_key", "В ящике лежит ржавый ключ!"));
            
            // Создание двери
            Door door = new Door("door", "Тяжёлая дверь", "Массивная дубовая дверь с ржавым замком");
            door.UnlockCondition = new HasItemCondition("rusty_key");
            door.Effects.Add(new AddExitEffect("east", "treasure"));
            door.Effects.Add(new RemoveItemEffect("rusty_key"));
            door.Effects.Add(new LogEffect("Замок поддаётся! Дверь со скрипом открывается..."));
            
            // Создание плиты-ловушки (без слова trap)
            PressurePlate plate = new PressurePlate("plate", "Каменная плита", "Одна из плит пола слегка выступает над остальными");
            plate.Effects.Add(new DamageEffect(20));
            plate.Effects.Add(new LogEffect("Плита уходит вниз! Из стены вылетают стрелы!"));
            
            // Создание нпс
            NPC guard = new NPC("guard", "Стражник", "Усталый стражник в помятых доспехах");
            guard.Dialogue = "Добро пожаловать, путник. Будь осторожен - здесь повсюду ловушки. Ищи ключи в сундуках.";
            guard.DialogueCondition = new NotCondition(new FlagCondition("met_guard"));
            guard.Effects.Add(new SetFlagEffect("met_guard", true));
            
            // Добавление интерактаблов
            startRoom.AddInteractable(chest);
            startRoom.AddInteractable(rustyChest);
            hallway.AddInteractable(door);
            hallway.AddInteractable(plate);
            hallway.AddInteractable(guard);
            
            // Создание квеста
            Quest quest = new Quest("find_treasure", "В поисках сокровищ", 
                "Найдите путь в сокровищницу и заберите богатства!");
            quest.StartCondition = new NotCondition(new FlagCondition("game_started"));
            quest.CompleteCondition = new FlagCondition("found_treasure");
            quest.OnCompleteEffects.Add(new LogEffect("Поздравляем! Вы нашли сокровища!"));
            
            // Событие при входе в сокровищницу
            OnEnterLocationEvent treasureEvent = new OnEnterLocationEvent();
            treasureEvent.Condition = new NotCondition(new FlagCondition("found_treasure"));
            treasureEvent.Effects.Add(new SetFlagEffect("found_treasure", true));
            treasureEvent.Effects.Add(new LogEffect("Невероятно! Вы нашли легендарные сокровища!"));
            treasureRoom.AddEvent(treasureEvent);
            
            game.State.AddQuest(quest);
            game.State.SetFlag("game_started", true);
            
            // Регистрация локаций
            game.RegisterLocation(startRoom);
            game.RegisterLocation(hallway);
            game.RegisterLocation(treasureRoom);
            
            // Установка начальной локации
            game.ChangeLocation("start");
            
            // Запуск игры
            game.Run();
        }
    }
}