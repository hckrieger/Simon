using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;

namespace Simon
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        enum GameState
        {
            Start,
            Watch,
            Repeat,
            GameOver
        }

        GameState state;

        SpriteFont title;
        SpriteFont message;
        SpriteFont scoreFont;

        int score;
        int highscore;

        MouseStateExtended mouse;

        Random rand = new Random();
        Square[,] grid;

        Dictionary<int, Square> squareSet;

        List<int> sequence = new List<int>();
        int sequenceIndex = 0;
        float sequenceTimer, sequenceTimerStart = .75f;
        float transitionTimer, transitionTimerStart = 1.2f;

        Color backgroundColor = Color.Black;

        string messageString;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
         
        }

        protected override void Initialize()
        {

            _graphics.PreferredBackBufferWidth = 365;
            _graphics.PreferredBackBufferHeight = 550;
            _graphics.ApplyChanges();

            
            squareSet = new Dictionary<int, Square>();
            state = GameState.Start;

            Vector2 offset = new Vector2(25, 160);
            Size2 squareSize = new Size2(150, 175);

            int iterator = 0;

            score = 0;

            Color[] activeColors = { Color.Green, Color.Yellow, Color.Red, Color.Blue };
            Color[] blinkingColors = { Color.LightGreen, Color.LightYellow, Color.PaleVioletRed, Color.CornflowerBlue };

            const int ROWS = 2;
            const int COLUMNS = 2;

            sequenceTimer = sequenceTimerStart;

            grid = new Square[ROWS, COLUMNS]; //Initiate the multi-dimensional array where the squares lie

            for (int x = 0; x < ROWS; x++) // Imbed a loop in a loop to present the grid of squares
            {
                for (int y = 0; y < COLUMNS; y++)
                {
                    grid[x, y] = new Square( // Pass parameters into the square object that set each squares position, size, color, blinking color and tone
                        Content,    
                        new RectangleF(x * 165 + offset.X, y * 190 + offset.Y, squareSize.Width, squareSize.Height), //Set position of rectangles
                        activeColors[iterator],
                        blinkingColors[iterator],
                        iterator
                    );
                    squareSet.Add(iterator, grid[x, y]);  //Add each square to a dictionary so it can be called on by an integer from here on out. 
                    iterator++;
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            title = Content.Load<SpriteFont>("Title");
            message = Content.Load<SpriteFont>("Message");
            scoreFont = Content.Load<SpriteFont>("Message");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            mouse = MouseExtended.GetState();

            for (int i = 0; i < squareSet.Count; i++)
            {
                squareSet[i].Update(gameTime);
            }

            //Switch between gamestates
            switch (state)
            {
                case GameState.Start:
                    Start();
                    break;

                case GameState.Watch:
                    WatchStateLogic(gameTime);
                    break;

                case GameState.Repeat:                    
                    RepeatStateLogic();
                    break;

                case GameState.GameOver:
                    GameOver();
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            _spriteBatch.Begin();

            //Draw the 4 rectangles
            for (int i = 0; i < squareSet.Count; i++)
            {
                _spriteBatch.FillRectangle(squareSet[i].BoundingBox, squareSet[i].Color);
            }

            //Method for centering the origin of the text
            Vector2 CenterText(SpriteFont font, string text)
            {
                return font.MeasureString(text) / 2;
            }


            var titleString = "Simon";  //Draw the title 
            _spriteBatch.DrawString(title, titleString, new Vector2(_graphics.PreferredBackBufferWidth/2, 40), Color.White, 0f, CenterText(title, titleString), 1f, SpriteEffects.None, 0f);


            //Draw the score and message
            var scoreString = $"Score: {score}         Record: {highscore}";
            _spriteBatch.DrawString(scoreFont, scoreString, new Vector2(_graphics.PreferredBackBufferWidth / 2, 95), Color.White, 0f, CenterText(message, scoreString), 1f, SpriteEffects.None, 0f);
            _spriteBatch.DrawString(message, messageString, new Vector2(_graphics.PreferredBackBufferWidth / 2, 135), Color.White, 0f, CenterText(message, messageString), 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void Start() //The state that starts at the beginning
        {
            
            messageString = "Click to Play";
            if (mouse.WasButtonJustDown(MouseButton.Left))
            {
                sequence.Add(rand.Next(0, 4));
                state = GameState.Watch;
            }
        }

        private void GameOver() //State that plays after you lose and want to restart
        {
            messageString = "Wrong! Click to play again";

            if (score > highscore)  //Set high score if score is bigger than the last high score
                highscore = score;

            //If player clicks than reset the score, sequence, background color and go to the Watch state
            if (mouse.WasButtonJustDown(MouseButton.Left))
            {
                backgroundColor = Color.Black;
                score = 0;
                sequence.Add(rand.Next(0, 4));        
                state = GameState.Watch;
            }
        }

        //This state plays when it plays a pattern that the player has to observe before repeating
        private void WatchStateLogic(GameTime gameTime) 
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            messageString = "Watch";
            transitionTimer -= dt;
            //If the transition timer runs out (this is for after the state switches from Repeat to Watch to give the player more time between states)
            if (transitionTimer <= 0)
            {
                sequenceTimer -= dt;
                //when the sequence timer runs out then play the next tone in the sequence
                if (sequenceTimer <= 0)
                {

                    squareSet[sequence[sequenceIndex]].PlayTone();
                    sequenceTimer = sequenceTimerStart;
                    sequenceIndex++;
                }
                //When the sequence has finished than go the the Repeat state
                else if (sequenceIndex == sequence.Count && sequence.Count > 0)
                {
                    sequenceIndex = 0;
                    transitionTimer = transitionTimerStart;
                    state = GameState.Repeat;
                }
            }
        }

        //The state for when the player has to repeat the pattern 
        private void RepeatStateLogic()
        {
            
            messageString = "Repeat";
            for (int i = 0; i < squareSet.Count; i++)
            {
                //If the player clicks on a square when it's not activated
                //then activate the square that's been clicked on to activate it's tone and color
                if (squareSet[i].BoundingBox.Contains(mouse.Position) &&
                    !squareSet[i].ColorSwitched &&
                    mouse.WasButtonJustDown(MouseButton.Left))
                {
                    squareSet[i].PlayTone(); //See the Square class to see what this method does

                    //if the player clicks on the correct square in the sequence then move to the next in the sequence
                    if (i == sequence[sequenceIndex])
                    { 
                        sequenceIndex++;                                
                    } 
                    //If the player clicks on the wrong square in the sequence....
                    else                                                
                    {
                        //Then turn the background color to Dark red, clear the sequence and go the GameOver state
                        backgroundColor = Color.DarkRed;
                        sequenceIndex = 0;
                        sequence.Clear();
                        state = GameState.GameOver;   
                                                       
                                              
                    }
                }
            }

            //If the player repeats the correct sequence
            if (sequenceIndex == sequence.Count && sequence.Count > 0)                     
            {
                //Then add one to the score, add a random value to the end of the pattern so it repeats one more, and go to the Watch state
                score++;
                sequenceIndex = 0;                                       
                sequence.Add(rand.Next(0, 4));                           
                transitionTimer = transitionTimerStart;
                state = GameState.Watch;                                 
            }

            
        }
    }
}
