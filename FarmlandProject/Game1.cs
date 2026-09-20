using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FarmlandProject;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private SpriteFont _font = null!;
    private Texture2D _pixel = null!;
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SoundEffect> _sounds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Song> _songs = new(StringComparer.OrdinalIgnoreCase);
    private SoundEffect? _nightmareEffect;
    private SoundEffect? _screamEffect;
    private Player _player = null!;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private Screen _screen = Screen.Farm;
    private bool _nightmareMode;
    private bool _killConfirmationPending;
    private bool _deathSequence;
    private double _deathSequenceTimer;
    private int _lastJumpscare;
    private double _nightmareEffectTimer;
    private double _escapeTimer;
    private float _runDistance;
    private float _devilDistance;
    private bool _escapeFinished;
    private bool _escapeWon;
    private bool _devilCapture;
    private double _captureTimer;
    private NightmareStage _nightmareStage;
    private string _message = "Welcome to My Farm!";
    private double _messageTimer;

    private enum Screen { Farm, Inventory, Shop, Market }
    private enum NightmareStage { None, RunChoice, EscapePath }
    private static readonly Color Background = new(26, 31, 45);
    private static readonly Color Panel = new(48, 57, 79);
    private static readonly Color Border = new(111, 128, 165);
    private static readonly Color Gold = new(247, 196, 79);
    private static readonly Color Green = new(105, 189, 98);
    private static readonly Color Red = new(224, 94, 94);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1152,
            PreferredBackBufferHeight = 720,
            IsFullScreen = true,
            HardwareModeSwitch = false
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "My Farm - Pixel Farm UI";
    }

    protected override void Initialize()
    {
        _player = new Player();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        LoadExternalAssets();
        _nightmareEffect = CreateNightmareEffect();
        _screamEffect = CreateScreamEffect();
        StartFarmMusic();
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        if (_nightmareStage == NightmareStage.RunChoice)
        {
            Rectangle leftRunButton = GetRunButton(true);
            Rectangle rightRunButton = GetRunButton(false);
            if (Clicked(mouse, leftRunButton) || Clicked(mouse, rightRunButton))
            {
                _nightmareStage = NightmareStage.EscapePath;
                _escapeTimer = 0;
                _runDistance = 0;
                _devilDistance = 42;
                _escapeFinished = false;
                _escapeWon = false;
                _devilCapture = false;
                _captureTimer = 0;
                PlayNightmareEffect();
            }

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        if (_nightmareStage == NightmareStage.EscapePath)
        {
            if (!_escapeFinished)
            {
                double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
                _escapeTimer += elapsed;
                _nightmareEffectTimer -= elapsed;

                bool running = keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up);
                if (running)
                {
                    _runDistance += (float)(elapsed * 24);
                    _devilDistance += (float)(elapsed * 1.5);
                }
                else
                {
                    _devilDistance -= (float)(elapsed * 32);
                }

                if (_nightmareEffectTimer <= 0)
                {
                    PlayNightmareEffect();
                    _nightmareEffectTimer = running ? 2.2 : 1.2;
                }

                if (_runDistance >= 100)
                {
                    _escapeFinished = true;
                    _escapeWon = true;
                }
                else if (_devilDistance <= 0 || _escapeTimer >= 30)
                {
                    _escapeFinished = true;
                    _escapeWon = false;
                    _devilCapture = true;
                    _captureTimer = 0;
                    PlayScream();
                }
            }

            if (_devilCapture)
            {
                _captureTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (_captureTimer >= 5)
                {
                    Exit();
                }
            }

            if (Pressed(keyboard, Keys.R) && _escapeFinished && !_devilCapture)
            {
                _escapeTimer = 0;
                _runDistance = 0;
                _devilDistance = 42;
                _escapeFinished = false;
                _escapeWon = false;
                _devilCapture = false;
                _captureTimer = 0;
            }

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        if (_deathSequence)
        {
            _deathSequenceTimer += gameTime.ElapsedGameTime.TotalSeconds;
            int jumpscare = (int)(_deathSequenceTimer / 2);
            double jumpscarePhase = _deathSequenceTimer % 2;
            if (jumpscarePhase >= 0.85 && _lastJumpscare != jumpscare)
            {
                _lastJumpscare = jumpscare;
                PlayNightmareEffect();
            }

            if (_deathSequenceTimer >= 10)
            {
                Exit();
            }

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        if (_killConfirmationPending)
        {
            if (Clicked(mouse, new Rectangle(380, 365, 190, 64)))
            {
                _killConfirmationPending = false;
                ActivateNightmareMode();
            }
            else if (Clicked(mouse, new Rectangle(590, 365, 190, 64)))
            {
                _killConfirmationPending = false;
                Notify("The animals are safe.");
            }

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            _messageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(gameTime);
            return;
        }

        if (Pressed(keyboard, Keys.Escape)) Exit();
        if (Pressed(keyboard, Keys.Space)) { _player.time.Tick(); Notify($"Day {_player.time.Day} started."); }
        if (Pressed(keyboard, Keys.H)) { _player.farm.HarvestAll(); PlaySound("Harvest"); Notify("Harvested all grown crops."); }
        if (Pressed(keyboard, Keys.C)) { _player.farm.Plant("Carrot"); PlaySound("Plant"); Notify("Planted carrot."); }
        if (Pressed(keyboard, Keys.P)) { _player.farm.Plant("Potato"); PlaySound("Plant"); Notify("Planted potato."); }
        if (Pressed(keyboard, Keys.T)) { _player.farm.Plant("Tomato"); PlaySound("Plant"); Notify("Planted tomato."); }
        if (Pressed(keyboard, Keys.K)) RequestKillConfirmation();

        if (Clicked(mouse, new Rectangle(32, 22, 145, 48))) _screen = Screen.Farm;
        if (Clicked(mouse, new Rectangle(187, 22, 170, 48))) _screen = Screen.Inventory;
        if (Clicked(mouse, new Rectangle(367, 22, 145, 48))) _screen = Screen.Shop;
        if (Clicked(mouse, new Rectangle(522, 22, 145, 48))) _screen = Screen.Market;
        if (_screen == Screen.Farm && Clicked(mouse, new Rectangle(850, 505, 250, 52))) RequestKillConfirmation();

        if (_screen == Screen.Shop)
        {
            if (Clicked(mouse, new Rectangle(70, 180, 230, 70))) { _player.farm.Plant("Carrot"); PlaySound("Plant"); Notify("Planted carrot."); }
            if (Clicked(mouse, new Rectangle(70, 270, 230, 70))) { _player.farm.Plant("Potato"); PlaySound("Plant"); Notify("Planted potato."); }
            if (Clicked(mouse, new Rectangle(70, 360, 230, 70))) { _player.farm.Plant("Tomato"); PlaySound("Plant"); Notify("Planted tomato."); }
            if (Clicked(mouse, new Rectangle(430, 180, 230, 70))) { _player.farm.BuyAnimal("Chicken"); PlaySound("Buy"); PlaySound("Chicken"); Notify("Chicken purchase attempted."); }
            if (Clicked(mouse, new Rectangle(430, 270, 230, 70))) { _player.farm.BuyAnimal("Cow"); PlaySound("Buy"); PlaySound("Cow"); Notify("Cow purchase attempted."); }
            if (Clicked(mouse, new Rectangle(430, 360, 230, 70))) { _player.farm.BuyAnimal("Pig"); PlaySound("Buy"); PlaySound("Pig"); Notify("Pig purchase attempted."); }
        }
        else if (_screen == Screen.Market)
        {
            if (Clicked(mouse, new Rectangle(70, 180, 230, 70))) { _player.farm.SellCrop("Carrot"); PlaySound("Sell"); Notify("Carrot sale attempted."); }
            if (Clicked(mouse, new Rectangle(70, 270, 230, 70))) { _player.farm.SellCrop("Potato"); PlaySound("Sell"); Notify("Potato sale attempted."); }
            if (Clicked(mouse, new Rectangle(70, 360, 230, 70))) { _player.farm.SellCrop("Tomato"); PlaySound("Sell"); Notify("Tomato sale attempted."); }
            if (Clicked(mouse, new Rectangle(430, 180, 230, 70))) { _player.farm.SellAnimal("Chicken"); PlaySound("Sell"); Notify("Chicken sale attempted."); }
            if (Clicked(mouse, new Rectangle(430, 270, 230, 70))) { _player.farm.SellAnimal("Cow"); PlaySound("Sell"); Notify("Cow sale attempted."); }
            if (Clicked(mouse, new Rectangle(430, 360, 230, 70))) { _player.farm.SellAnimal("Pig"); PlaySound("Sell"); Notify("Pig sale attempted."); }
        }

        _previousMouse = mouse;
        _previousKeyboard = keyboard;
        _messageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if (_nightmareMode)
        {
            UpdateNightmareAudio(gameTime.ElapsedGameTime.TotalSeconds);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_nightmareMode ? new Color(7, 3, 12) : Background);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (_deathSequence)
        {
            DrawDeathSequence();
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        if (_nightmareStage == NightmareStage.RunChoice)
        {
            DrawRunChoice();
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        if (_nightmareStage == NightmareStage.EscapePath)
        {
            DrawEscapePath();
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        DrawHeader();
        switch (_screen)
        {
            case Screen.Farm: DrawFarm(); break;
            case Screen.Inventory: DrawInventory(); break;
            case Screen.Shop: DrawShop(); break;
            case Screen.Market: DrawMarket(); break;
        }
        DrawFooter();
        if (_killConfirmationPending)
        {
            DrawKillConfirmation();
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawHeader()
    {
        DrawRect(new Rectangle(20, 16, 1112, 68), _nightmareMode ? new Color(28, 8, 30) : Panel, _nightmareMode ? Red : Border);
        Button("FARM", new Rectangle(32, 22, 145, 48), _screen == Screen.Farm);
        Button("INVENTORY", new Rectangle(187, 22, 170, 48), _screen == Screen.Inventory);
        Button("SHOP", new Rectangle(367, 22, 145, 48), _screen == Screen.Shop);
        Button("MARKET", new Rectangle(522, 22, 145, 48), _screen == Screen.Market);
        Text($"DAY {_player.time.Day}", 760, 38, Color.White);
        Text($"GOLD {_player.money.TotalGold}", 955, 38, Gold);
    }

    private void DrawFarm()
    {
        Text("FARM", 44, 112, Green, 1.4f);
        DrawRect(new Rectangle(32, 154, 690, 410), _nightmareMode ? new Color(35, 10, 35) : new Color(68, 111, 65), _nightmareMode ? Red : Border);
        for (var x = 55; x < 690; x += 75) DrawRect(new Rectangle(x, 210, 55, 210), _nightmareMode ? new Color(50, 12, 45) : new Color(88, 139, 73));
        Text("YOUR FIELD", 55, 172, Color.White);
        var y = 235;
        foreach (var crop in _player.crops)
        {
            DrawCrop(crop.Type, 75 + (y % 5) * 110, y);
            Text($"{crop.Type} {(crop.IsGrown(_player.time.Day) ? "READY" : "GROWING")}", 75, y + 62, Color.White, .75f);
            y += 92;
            if (y > 480) break;
        }
        Text("CHICKENS & ANIMALS", 780, 125, Gold, 1.1f);
        var animalY = 180;
        foreach (var animal in _player.animals)
        {
            DrawAnimal(animal.Type, 810, animalY);
            Text(animal.Type, 870, animalY + 14, Color.White);
            animalY += 82;
            if (animalY > 510) break;
        }
        if (_player.animals.Count == 0) Text("Buy animals in the SHOP", 780, 180, Color.LightGray);
        Button(_nightmareMode ? "NIGHTMARE ACTIVE" : "KILL ALL ANIMALS", new Rectangle(850, 505, 250, 52), _nightmareMode);
    }

    private void DrawInventory()
    {
        Text("INVENTORY", 44, 112, Gold, 1.4f);
        DrawRect(new Rectangle(32, 154, 520, 400), Panel, Border);
        DrawRect(new Rectangle(580, 154, 540, 400), Panel, Border);
        Text("HARVESTED CROPS", 55, 175, Green, 1.1f);
        var y = 225;
        foreach (var crop in _player.harvestedCrops) { DrawCrop(crop.Type, 70, y); Text($"{crop.Type}  {crop.Price} GOLD", 130, y + 16, Color.White); y += 58; }
        if (_player.harvestedCrops.Count == 0) Text("Harvested crops appear here.", 55, 225, Color.LightGray);
        Text("OWNED ANIMALS", 605, 175, Gold, 1.1f);
        y = 225;
        foreach (var animal in _player.animals) { DrawAnimal(animal.Type, 620, y); Text($"{animal.Type}  {animal.Sound}", 680, y + 16, Color.White); y += 62; }
        if (_player.animals.Count == 0) Text("Animals appear here.", 605, 225, Color.LightGray);
    }

    private void DrawShop()
    {
        Text("SHOP", 44, 112, Gold, 1.4f); Text("PLANT CROPS", 70, 145, Green, 1.1f); Text("BUY ANIMALS", 430, 145, Gold, 1.1f);
        Button("CARROT   20G", new Rectangle(70, 180, 230, 70), false); Button("POTATO   35G", new Rectangle(70, 270, 230, 70), false); Button("TOMATO   50G", new Rectangle(70, 360, 230, 70), false);
        Button("CHICKEN  100G", new Rectangle(430, 180, 230, 70), false); Button("COW      150G", new Rectangle(430, 270, 230, 70), false); Button("PIG      200G", new Rectangle(430, 360, 230, 70), false);
        Text("Click an item to buy or plant it.", 70, 490, Color.LightGray);
    }

    private void DrawMarket()
    {
        Text("MARKET", 44, 112, Gold, 1.4f); Text("SELL CROPS", 70, 145, Green, 1.1f); Text("SELL ANIMALS", 430, 145, Gold, 1.1f);
        Button("CARROT   +20G", new Rectangle(70, 180, 230, 70), false); Button("POTATO   +35G", new Rectangle(70, 270, 230, 70), false); Button("TOMATO   +50G", new Rectangle(70, 360, 230, 70), false);
        Button("CHICKEN  +100G", new Rectangle(430, 180, 230, 70), false); Button("COW      +150G", new Rectangle(430, 270, 230, 70), false); Button("PIG      +200G", new Rectangle(430, 360, 230, 70), false);
        Text("Only harvested crops and owned animals can be sold.", 70, 490, Color.LightGray);
    }

    private void DrawFooter()
    {
        DrawRect(new Rectangle(20, 600, 1112, 94), _nightmareMode ? new Color(28, 8, 30) : Panel, _nightmareMode ? Red : Border);
        Text("SPACE: sleep / next day     H: harvest all     ESC: quit", 42, 620, Color.White, .85f);
        Text("Use the tabs above to manage your farm.", 42, 655, Color.LightGray, .8f);
        if (_messageTimer > 0) Text(_message, 590, 655, Gold, .75f);
    }

    private void DrawAnimal(string type, int x, int y)
    {
        if (_textures.TryGetValue(type, out Texture2D? texture))
        {
            _spriteBatch.Draw(texture, new Rectangle(x, y, 64, 64), Color.White);
            return;
        }

        var dark = new Color(35, 31, 45); var body = type == "Chicken" ? new Color(244,239,215) : type == "Cow" ? Color.White : new Color(226,115,124);
        DrawRect(new Rectangle(x + 8, y + 8, 42, 30), dark); DrawRect(new Rectangle(x + 12, y + 10, 34, 24), body);
        if (type == "Chicken") { DrawRect(new Rectangle(x + 2, y + 17, 12, 9), body); DrawRect(new Rectangle(x + 45, y + 18, 9, 6), Gold); DrawRect(new Rectangle(x + 20, y + 2, 10, 8), Red); }
        else if (type == "Cow") { DrawRect(new Rectangle(x + 20, y + 13, 9, 8), new Color(139,84,48)); DrawRect(new Rectangle(x + 37, y + 22, 10, 8), new Color(139,84,48)); }
        else { DrawRect(new Rectangle(x + 44, y + 18, 12, 12), body); DrawRect(new Rectangle(x + 48, y + 22, 4, 4), Red); }
        DrawRect(new Rectangle(x + 17, y + 34, 7, 10), dark); DrawRect(new Rectangle(x + 35, y + 34, 7, 10), dark);
    }

    private void DrawCrop(string type, int x, int y)
    {
        if (_textures.TryGetValue(type, out Texture2D? texture))
        {
            _spriteBatch.Draw(texture, new Rectangle(x, y, 48, 48), Color.White);
            return;
        }

        var color = type == "Carrot" ? new Color(240,150,45) : type == "Potato" ? new Color(150,95,50) : Red;
        DrawRect(new Rectangle(x, y + 20, 30, 30), color, new Color(35,31,45));
        DrawRect(new Rectangle(x + 9, y + 5, 12, 18), Green);
    }

    private void Button(string label, Rectangle rectangle, bool selected)
    {
        DrawRect(rectangle, selected ? new Color(77, 112, 89) : new Color(61, 70, 96), selected ? Gold : Border);
        Text(label, rectangle.X + 14, rectangle.Y + 22, Color.White, .8f);
    }

    private void DrawRect(Rectangle rectangle, Color fill, Color border)
    {
        _spriteBatch.Draw(_pixel, rectangle, fill);
        _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 3), border);
        _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Bottom - 3, rectangle.Width, 3), border);
        _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, 3, rectangle.Height), border);
        _spriteBatch.Draw(_pixel, new Rectangle(rectangle.Right - 3, rectangle.Y, 3, rectangle.Height), border);
    }

    private void DrawRect(Rectangle rectangle, Color fill)
    {
        _spriteBatch.Draw(_pixel, rectangle, fill);
    }

    private void Text(string value, int x, int y, Color color, float scale = 1f) => _spriteBatch.DrawString(_font, value, new Vector2(x, y), color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
    private bool Pressed(KeyboardState state, Keys key) => state.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    private bool Clicked(MouseState state, Rectangle rectangle) => state.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released && rectangle.Contains(state.Position);
    private void Notify(string message) { _message = message; _messageTimer = 3; }

    private void ActivateNightmareMode()
    {
        if (_nightmareMode || _deathSequence)
        {
            Notify("Nightmare mode is already active.");
            return;
        }

        int killedCount = _player.farm.KillAllAnimals();
        _nightmareMode = true;
        PlayNightmareEffect();
        _nightmareEffectTimer = 6;
        StartNightmareMusic();
        _deathSequence = false;
        _deathSequenceTimer = 0;
        _lastJumpscare = -1;
        _nightmareStage = NightmareStage.RunChoice;
        _message = $"NIGHTMARE MODE: {killedCount} animal(s) were killed.";
    }

    private void RequestKillConfirmation()
    {
        if (_nightmareMode || _deathSequence)
        {
            return;
        }

        _killConfirmationPending = true;
        _messageTimer = 0;
    }

    private void DrawKillConfirmation()
    {
        DrawRect(
            new Rectangle(250, 220, 660, 260),
            new Color(18, 8, 24, 245),
            Red);
        Text("ARE YOU SURE?", 430, 255, Red, 1.4f);
        Text("This cannot be undone.", 418, 305, Color.White, .9f);
        Text("Click a second button to continue.", 375, 330, Color.LightGray, .75f);
        Button("CONFIRM DEATH", new Rectangle(380, 365, 190, 64), false);
        Button("CANCEL", new Rectangle(590, 365, 190, 64), false);
    }

    private void DrawRunChoice()
    {
        _spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            new Color(3, 1, 8));

        float pulse = (MathF.Sin((float)_escapeTimer * 5) + 1) / 2;
        int width = GraphicsDevice.Viewport.Width;
        int center = width / 2;
        Text("RUN?", center - 70, 170, new Color(205, 20, 35) * (0.65f + pulse * 0.35f), 2.4f);
        Text("THE PATH IS WAITING.", center - 150, 260, Color.LightGray, 1.05f);
        Text("There is no safe choice.", center - 140, 305, new Color(170, 150, 170), .85f);
        Button("YES", GetRunButton(true), false);
        Button("YES", GetRunButton(false), false);
    }

    private Rectangle GetRunButton(bool left)
    {
        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        int buttonWidth = Math.Min(260, Math.Max(180, width / 5));
        int buttonHeight = 84;
        int gap = Math.Max(24, width / 40);
        int totalWidth = buttonWidth * 2 + gap;
        int x = (width - totalWidth) / 2 + (left ? 0 : buttonWidth + gap);
        int y = Math.Min(height - buttonHeight - 100, Math.Max(360, height / 2 + 30));
        return new Rectangle(x, y, buttonWidth, buttonHeight);
    }

    private void DrawEscapePath()
    {
        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;

        if (_devilCapture)
        {
            DrawRect(new Rectangle(0, 0, width, height), Color.Black);
            Text("THE DEVIL CAUGHT YOU", width / 2 - 205, height / 2 - 35, Red, 1.35f);
            return;
        }

        int horizon = 170 + (int)(MathF.Sin((float)_escapeTimer * 4) * 5);

        DrawRect(new Rectangle(0, horizon, width, height - horizon), new Color(12, 8, 20));

        for (int slice = 0; slice < 14; slice++)
        {
            float near = slice / 14f;
            float far = (slice + 1) / 14f;
            int y = horizon + (int)(near * near * (height - horizon));
            int nextY = horizon + (int)(far * far * (height - horizon));
            int roadWidth = 80 + (int)(near * 900);
            int nextWidth = 80 + (int)(far * 900);
            Color roadColor = slice % 2 == 0 ? new Color(25, 22, 35) : new Color(31, 27, 42);
            DrawRect(new Rectangle((width - roadWidth) / 2, y, roadWidth, Math.Max(2, nextY - y)), roadColor);
            DrawRect(new Rectangle((width - nextWidth) / 2 - 8, y, 8, Math.Max(2, nextY - y)), new Color(80, 12, 30));
            DrawRect(new Rectangle((width + nextWidth) / 2, y, 8, Math.Max(2, nextY - y)), new Color(80, 12, 30));
        }

        for (int tree = 0; tree < 9; tree++)
        {
            float depth = (tree + 1) / 10f;
            int y = horizon + (int)(depth * depth * 390);
            int size = 20 + (int)(depth * 90);
            int side = tree % 2 == 0 ? 1 : -1;
            int x = width / 2 + side * (190 + (int)(depth * 250));
            DrawRect(new Rectangle(x - size / 2, y, size, size * 2), new Color(9, 20, 16));
            DrawRect(new Rectangle(x - size / 3, y + size * 2 - 8, size / 2, size * 2), new Color(18, 9, 18));
        }

        if (_devilDistance > 0)
        {
            float danger = MathHelper.Clamp(1f - _devilDistance / 42f, 0f, 1f);
            int devilY = horizon + 25 + (int)(danger * 250);
            DrawEscapeDevil(width / 2, devilY, 5 + (int)(danger * 24));
        }

        if (_escapeFinished)
        {
            string result = _escapeWon ? "YOU ESCAPED" : "THE DEVIL CAUGHT YOU";
            Color resultColor = _escapeWon ? Color.LimeGreen : Red;
            Text(result, width / 2 - (result.Length * 11), height / 2 - 25, resultColor, 1.5f);
            Text("Press R to run again", width / 2 - 115, height / 2 + 35, Color.White, .85f);
        }
        else
        {
            Text("HOLD W OR UP ARROW TO RUN", width / 2 - 220, height - 80, new Color(240, 210, 120), 1f);
            Text($"DISTANCE {Math.Min(100, (int)_runDistance)}%", 40, 40, Color.White, .9f);
            Text($"DEVIL {Math.Max(0, (int)_devilDistance)}m", width - 180, 40, Red, .9f);
        }
    }

    private void DrawEscapeDevil(int centerX, int y, int scale)
    {
        Color body = new(8, 2, 10);
        DrawRect(new Rectangle(centerX - scale * 4, y, scale * 8, scale * 18), body);
        DrawRect(new Rectangle(centerX - scale * 8, y + scale * 4, scale * 16, scale * 12), body);
        DrawRect(new Rectangle(centerX - scale * 10, y - scale * 4, scale * 5, scale * 10), new Color(100, 5, 20));
        DrawRect(new Rectangle(centerX + scale * 5, y - scale * 4, scale * 5, scale * 10), new Color(100, 5, 20));
        DrawRect(new Rectangle(centerX - scale * 5, y + scale * 5, scale * 3, scale * 3), Red);
        DrawRect(new Rectangle(centerX + scale * 2, y + scale * 5, scale * 3, scale * 3), Red);
    }

    private void DrawDeathSequence()
    {
        float time = (float)_deathSequenceTimer;
        float pulse = (MathF.Sin(time * 12f) + 1f) / 2f;
        float textAlpha = MathHelper.Clamp(0.2f + pulse * 0.8f, 0f, 1f);
        double phase = time % 2;
        bool jumpscare = phase >= 0.85 && phase < 1.65;
        bool redFlash = jumpscare || ((int)(time * 18) % 11) == 0;

        _spriteBatch.Draw(
            _pixel,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            redFlash ? new Color(75, 0, 0) : Color.Black);

        if (jumpscare)
        {
            DrawJumpscareFace();
            return;
        }

        string message = ((int)(time / 2) % 2 == 0)
            ? "YOU WILL DIE."
            : "IN A TERRIFYING DEATH.";

        Vector2 size = _font.MeasureString(message) * 1.35f;
        Vector2 position = new(
            (GraphicsDevice.Viewport.Width - size.X) / 2f,
            (GraphicsDevice.Viewport.Height - size.Y) / 2f);

        _spriteBatch.DrawString(
            _font,
            message,
            position,
            new Color(220, 20, 30) * textAlpha,
            0,
            Vector2.Zero,
            1.35f,
            SpriteEffects.None,
            1);
    }

    private void DrawJumpscareFace()
    {
        int shake = (int)(MathF.Sin((float)_deathSequenceTimer * 100) * 30);
        int centerX = GraphicsDevice.Viewport.Width / 2 + shake;
        int centerY = GraphicsDevice.Viewport.Height / 2;
        Color skin = new(235, 225, 185);
        Color black = new(3, 0, 5);

        DrawRect(new Rectangle(centerX - 260, centerY - 240, 520, 460), skin, Red);
        DrawRect(new Rectangle(centerX - 175, centerY - 120, 105, 145), black);
        DrawRect(new Rectangle(centerX + 70, centerY - 120, 105, 145), black);
        DrawRect(new Rectangle(centerX - 135, centerY - 80, 30, 60), Red);
        DrawRect(new Rectangle(centerX + 105, centerY - 80, 30, 60), Red);
        DrawRect(new Rectangle(centerX - 170, centerY + 70, 340, 95), black);

        for (int tooth = -145; tooth <= 125; tooth += 45)
        {
            DrawRect(new Rectangle(centerX + tooth, centerY + 70, 24, 45), skin);
        }

        string message = "DON'T LOOK AWAY";
        Vector2 size = _font.MeasureString(message) * 1.65f;
        _spriteBatch.DrawString(_font, message,
            new Vector2((GraphicsDevice.Viewport.Width - size.X) / 2, 58),
            Color.White, 0, Vector2.Zero, 1.65f, SpriteEffects.None, 1);
    }

    private void UpdateNightmareAudio(double elapsedSeconds)
    {
        _nightmareEffectTimer -= elapsedSeconds;
        if (_nightmareEffectTimer <= 0)
        {
            PlayNightmareEffect();
            _nightmareEffectTimer = 6;
        }
    }

    private void StartNightmareMusic()
    {
        if (_songs.TryGetValue("Nightmare", out Song? music) ||
            _songs.TryGetValue("glitch_001", out music))
        {
            MediaPlayer.Volume = 1f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
        }
    }

    private void StartFarmMusic()
    {
        if (_songs.TryGetValue("Farm", out Song? music) ||
            _songs.TryGetValue("jingles_NES00", out music))
        {
            MediaPlayer.Volume = 0.55f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
        }
    }

    private void PlayNightmareEffect()
    {
        SoundEffect? scream = FindSound("scream", "horror", "monster", "grunt", "glitch");
        (scream ?? _nightmareEffect)?.Play(1f, 0.05f, 0f);
    }

    private void PlayScream()
    {
        _screamEffect?.Play(1f, 0f, 0f);
        _nightmareEffect?.Play(0.85f, -0.35f, 0f);
    }

    private SoundEffect? FindSound(params string[] names)
    {
        foreach (string name in names)
        {
            var match = _sounds.FirstOrDefault(pair => pair.Key.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (match.Value != null)
            {
                return match.Value;
            }
        }

        return null;
    }

    private static SoundEffect CreateNightmareEffect()
    {
        const int sampleRate = 44100;
        const double duration = 0.7;
        int sampleCount = (int)(sampleRate * duration);
        byte[] samples = new byte[sampleCount * 2];
        uint noise = 0x12345678;

        for (int i = 0; i < sampleCount; i++)
        {
            noise ^= noise << 13;
            noise ^= noise >> 17;
            noise ^= noise << 5;
            double time = i / (double)sampleRate;
            double envelope = Math.Min(1, Math.Min(time * 12, (duration - time) * 8));
            double rumble = Math.Sin(2 * Math.PI * (48 + time * 18) * time) * 0.65;
            double distortion = ((noise & 0xffff) / 32768.0 - 1) * 0.35;
            short value = (short)(Math.Clamp((rumble + distortion) * envelope * 28000, short.MinValue, short.MaxValue));
            samples[i * 2] = (byte)(value & 0xff);
            samples[i * 2 + 1] = (byte)(value >> 8);
        }

        return new SoundEffect(samples, sampleRate, AudioChannels.Mono);
    }

    private static SoundEffect CreateScreamEffect()
    {
        const int sampleRate = 44100;
        const double duration = 2.4;
        int sampleCount = (int)(sampleRate * duration);
        byte[] samples = new byte[sampleCount * 2];
        uint noise = 0xA53C91E7;

        for (int i = 0; i < sampleCount; i++)
        {
            double time = i / (double)sampleRate;
            double progress = time / duration;
            double frequency = 420 + progress * 1250;
            double voice = Math.Sin(2 * Math.PI * frequency * time);
            voice += 0.55 * Math.Sin(2 * Math.PI * frequency * 2.01 * time);
            voice += 0.25 * Math.Sin(2 * Math.PI * frequency * 3.03 * time);

            noise ^= noise << 13;
            noise ^= noise >> 17;
            noise ^= noise << 5;
            double grit = ((noise & 0xffff) / 32768.0 - 1) * 0.18;
            double attack = Math.Min(1, time * 30);
            double release = Math.Min(1, (duration - time) * 3);
            double tremolo = 0.78 + 0.22 * Math.Sin(2 * Math.PI * 7 * time);
            short value = (short)Math.Clamp((voice * 0.62 + grit) * attack * release * tremolo * 30000, short.MinValue, short.MaxValue);
            samples[i * 2] = (byte)(value & 0xff);
            samples[i * 2 + 1] = (byte)(value >> 8);
        }

        return new SoundEffect(samples, sampleRate, AudioChannels.Mono);
    }

    private void LoadExternalAssets()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "Content", "ExternalAssets");
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (string file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            try
            {
                string key = Path.GetFileNameWithoutExtension(file);
                if (file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    using FileStream stream = File.OpenRead(file);
                    _textures[key] = Texture2D.FromStream(GraphicsDevice, stream);
                }
                else if (file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    using FileStream stream = File.OpenRead(file);
                    _sounds[key] = SoundEffect.FromStream(stream);
                }
                else if (file.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                         file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    _songs[key] = Song.FromUri(key, new Uri(file));
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private void PlaySound(string name)
    {
        if (_sounds.TryGetValue(name, out SoundEffect? sound))
        {
            sound.Play();
        }
    }
}
