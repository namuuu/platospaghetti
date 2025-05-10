using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebuggerScript : MonoBehaviour
{
    bool showConsole;

    string input;

    public List<DebugCommandBase> commands = new();

    // This is a gross hardcoded dictionnary. Oh, well

    private void Awake()
    {
        commands.Add(new DebugCommand("respawn", () => {
            Debug.Log("Force respawning player");
            PlayerScript.Instance.Respawn();
        }));

        commands.Add(new DebugCommand<int, string>("set_ability", (slot, abilityName) => {
            AbilityManager.Instance.PutAbilityToSlot(abilityName, slot);
        }));

        commands.Add(new DebugCommand("quit", () => {
            LevelManager.Instance.LoadScene(Scene.MenuScene, Transition.CrossFade);
        }));
    }

    public void OnDebugKey(InputValue value)
    {
        if(showConsole)
        {
            Debug.Log("Showing console");
            HandleInput();
            input = "";
        } else
            showConsole = true;   
    }

    public void OnEscape(InputValue value)
    {
        showConsole = false;
    }

    private void OnGUI()
    {
        if(!showConsole) return;

        float y = 0f;
        
        // Draw the console
        GUI.Box(new Rect(0f, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

        
    }

    private void HandleInput()
    {

        string[] args = input.Split(' ');
        string inputCommand = args[0].ToLower();

        foreach (DebugCommandBase command in commands)
        {
            if(command.CommandId.ToLower() == inputCommand)
            {
                if (command is DebugCommand debugCommand)
                {
                    debugCommand.Invoke();
                }
                else if (command is DebugCommand<int, string> debugCommandWithParams)
                {
                    int slot = int.Parse(args[1]);
                    string abilityName = args[2];
                    debugCommandWithParams.Invoke(slot, abilityName);
                }
                return;
            }
        }

        Debug.LogError("Command " + inputCommand + " not found. List is: " + string.Join(", ", commands));
    }
}

public class DebugCommandBase
{
    private string _commandId;

    public string CommandId { get { return _commandId; } }

    public DebugCommandBase(string commandId)
    {
        _commandId = commandId;
    }

    public override string ToString()
    {
        return _commandId;
    }
}

public class DebugCommand: DebugCommandBase
{

    private Action _action;

    public DebugCommand(string id, Action action): base(id)
    {
        _action = action;
    }

    public void Invoke()
    {
        _action.Invoke();
    }
}

public class DebugCommand<T>: DebugCommandBase
{
    private Action<T> _action;

    public DebugCommand(string id, Action<T> action): base(id)
    {
        _action = action;
    }

    public void Invoke(T value)
    {
        _action.Invoke(value);
    }
}

public class DebugCommand<T, U>: DebugCommandBase
{
    private Action<T, U> _action;

    public DebugCommand(string id, Action<T, U> action): base(id)
    {
        _action = action;
    }

    public void Invoke(T value1, U value2)
    {
        _action.Invoke(value1, value2);
    }
}
