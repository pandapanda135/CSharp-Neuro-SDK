using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuroSDKCsharp;
using NeuroSDKCsharp.Messages.Outgoing;

namespace Sdk_Test;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GameInformation _gameInformation;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.PreferredBackBufferWidth = 1920;

        _graphics.IsFullScreen = true;
    }

    protected override void Initialize()
    {
        _gameInformation = new GameInformation(this);

        SdkSetup.Initialize(this,"MonoGame-Test","ws://localhost:8000");
        
        base.Initialize();
        Context.Send("A new game of rock paper rock paper scissors has started. Your opponent will make their move first",true);
    }
    
    SpriteFont _statusText;
    private Vector2 _statusTextPos;


    private SpriteFont _defaultFont;
    
    private Vector2 _controlsTextPos;
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _statusText = Content.Load<SpriteFont>("statusFont");
        _defaultFont = Content.Load<SpriteFont>("statusFont");

        
        Viewport viewport = _graphics.GraphicsDevice.Viewport;

        _statusTextPos = new Vector2(viewport.Width / 2f, viewport.Height - 1000);
        _controlsTextPos = new Vector2(viewport.Width / 2f,viewport.Height - 800);
    }
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        if (_gameInformation.WinState != GameInformation.PossibleWins.Selecting)
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Space))
            {
                _gameInformation.RestartGame();
            }
        }
        
        if (_gameInformation.PlayerTurn)
        {
            _gameInformation.SelectChoice(Keyboard.GetState());
        }
        
        _gameInformation.CheckWin();
        base.Update(gameTime);
    }

    public static string ControlsText = "The controls are: \n A for paper, S for Rock and D for scissors";
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        
        _spriteBatch.DrawString(_defaultFont,ControlsText,_controlsTextPos,Color.White);
        
        _spriteBatch.DrawString(_statusText,_gameInformation.CurrentGameString,_statusTextPos,Color.White);
        
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}