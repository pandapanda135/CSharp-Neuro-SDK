using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Messages.Outgoing;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace Sdk_Test;

public class GameInformation(Game gameClass) : GameComponent(gameClass)
{
    private enum Choices
    {
        Rock,
        Paper,
        Scissors,
        Selecting,
    }

    public enum PossibleWins
    {
        Player,
        Server,
        Draw,
        Selecting,
    }

    private Choices _playerOneChoice = Choices.Selecting;
    private Choices _playerTwoChoice = Choices.Selecting;

    public bool PlayerTurn = true;

    public PossibleWins WinState = PossibleWins.Selecting;

    public string CurrentGameString = "It is your turn to select";

    private static bool _hasShownWin;
    
    /// <summary>
    /// Check who won the current game
    /// </summary>
    /// <returns></returns>
    public PossibleWins CheckWin()
    {
        if (_hasShownWin) return WinState;
        
        // I'm such an amazing programmer :)
        // local
        if (_playerOneChoice == Choices.Rock && _playerTwoChoice == Choices.Scissors)
        {
            WinState = PossibleWins.Player;
        }

        if (_playerOneChoice == Choices.Paper && _playerTwoChoice == Choices.Rock)
        {
            WinState = PossibleWins.Player;
        }

        if (_playerOneChoice == Choices.Scissors && _playerTwoChoice == Choices.Paper)
        {
            WinState = PossibleWins.Player;
        }

        // server
        if (_playerTwoChoice == Choices.Rock && _playerOneChoice == Choices.Scissors)
        {
            WinState = PossibleWins.Server;
        }

        if (_playerTwoChoice == Choices.Paper && _playerOneChoice == Choices.Rock)
        {
            WinState = PossibleWins.Server;
        }

        if (_playerTwoChoice == Choices.Scissors && _playerOneChoice == Choices.Paper)
        {
            WinState = PossibleWins.Server;
        }

        if (_playerOneChoice == _playerTwoChoice && _playerOneChoice != Choices.Selecting && _playerTwoChoice != Choices.Selecting)
        {
            WinState = PossibleWins.Draw;
        }
        else if (_playerOneChoice == Choices.Selecting || _playerTwoChoice == Choices.Selecting)
        {
            return PossibleWins.Selecting;
        }

        CurrentGameString = $"The end result of the game was {WinState}\n player played: {_playerOneChoice}. Server played: {_playerTwoChoice}";
        Context.Send(CurrentGameString);
        _hasShownWin = true;
        Game1.ControlsText = "You should use space to restart the game.";
        return WinState;
    }

    private KeyboardState _previousKeyboardState;
    public void SelectChoice(KeyboardState keyState)
    {
        if (_hasShownWin)
        {
            return;
        }
        
        if (keyState.IsKeyDown(Keys.A) && _previousKeyboardState.IsKeyUp(Keys.A))
        {
            _playerOneChoice = Choices.Paper;
        }
        else if (keyState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S))
        {
            _playerOneChoice = Choices.Rock;
        }
        else if (keyState.IsKeyDown(Keys.D) && _previousKeyboardState.IsKeyUp(Keys.D))
        {
            _playerOneChoice = Choices.Scissors;
        }
        else
        {
            _previousKeyboardState = keyState;
            return;
        }
        
        _previousKeyboardState = keyState;
        PlayerTurn = false;
        CurrentGameString = "It is now the server time to pick";
        Context.Send("The opponent has just played.",true);

        ActionWindow window = ActionWindow.Create(Game)
            .SetForce(5, "It is now your turn. Please pick a choice.",
                $"Your opponent just did their turn, now it is time for you to do yours", false, ForcePriority.Critical)
            .AddAction(new ServerChoice(this));
        window.Register();
    }
    
    public void SelectChoiceServer(int choice)
    {
        Console.WriteLine($"Server choice was {choice}");
        
        switch (choice)
        {
            case 0:
                _playerTwoChoice = Choices.Rock;
                break;
            case 1:
                _playerTwoChoice = Choices.Paper;
                break;
            case 2:
                _playerTwoChoice = Choices.Scissors;
                break;
        }
        
        PlayerTurn = true;
    }

    public void RestartGame()
    {
        _playerOneChoice = Choices.Selecting;
        _playerTwoChoice = Choices.Selecting;
        PlayerTurn = true;
        WinState = PossibleWins.Selecting;
        _hasShownWin = false;
        Game1.ControlsText = "The controls are: \n A for paper, S for Rock and D for scissors";
        CurrentGameString = "It is your turn to select";
        
        Context.Send("A new game of rock paper rock paper scissors has started. Your opponent will make their move first", true);
    }
}

public class ServerChoice(GameInformation gameInformation) : NeuroAction<int>
{
    readonly IEnumerable<string> _choices = new List<string>() { "Rock", "Paper", "Scissors" };

    public override string Name => "play";
    protected override string Description => "Play an option to try to win";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
        Required = ["choice"],
        Properties = new Dictionary<string, JsonSchema>()
        {
            ["choice"] = QJS.Enum(_choices)
        }
    };

    protected override ExecutionResult Validate(ActionData actionData, out int choice)
    {
        string desiredChoice = actionData.Data?["choice"]?.Value<string>();

        if (string.IsNullOrEmpty(desiredChoice))
        {
            choice = 4;
            return ExecutionResult.Failure("Missing required parameter: choice");
        }

        if (!_choices.Contains(desiredChoice))
        {
            choice = 4;
            return ExecutionResult.Failure("Required parameter invalid: choice");
        }
        
        // incredible dumb way to handle this but im too lazy to change this
        switch (desiredChoice)
        {
            case "Rock":
                choice = 0;
                break;
            case "Paper":
                choice = 1;
                break;
            case "Scissors":
                choice = 2;
                break;
            default:
                choice = 4;
                return ExecutionResult.ModFailure($"Desired choice was an incorrect value, this is most likely an issue with the integration. {desiredChoice}");
        }
        return ExecutionResult.Success();
    }

    protected override void Execute(int choice)
    {
        gameInformation.SelectChoiceServer(choice);
    }
}
