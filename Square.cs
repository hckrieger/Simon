using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;

namespace Simon
{
    class Square 
    {
        Vector2 size = new Vector2(200, 225);
        public bool ColorSwitched { get; set; } 

        public RectangleF BoundingBox { get; set; }
        public Color Color { get; set; }

        Color blinkingColor;
        Color activeColor;

        float timer = .25f;

        SoundEffect tone;

        public Square(ContentManager Content, RectangleF BoundingBox, Color activeColor, Color blinkingColor, int toneIndex) 
        {
            tone = Content.Load<SoundEffect>($"tones/tone{toneIndex}");

            this.BoundingBox = BoundingBox;
            this.activeColor = activeColor;
            this.blinkingColor = blinkingColor;
            Color = this.activeColor;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.GetElapsedSeconds();

            //The duration after the player clicks on the square make the color blink to a different color 
            if (ColorSwitched)
            {
                Color = blinkingColor;
                timer -= dt;
                if (timer <= 0)
                {
                    timer = .25f;
                    ColorSwitched = false;
                } 
            } else
            {
                Color = activeColor;
            }
        }

        //Play the tone and execute the conditional above.  This method is accessed in the Game1 class to activate it
        public void PlayTone()
        {
            ColorSwitched = true;
            tone.Play();
        }
    }
}
