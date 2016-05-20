﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Extended.InputListeners
{
    public class KeyboardListenerComponent : InputListenerComponent
    {
        public KeyboardListenerComponent(Game game, int initialDelayMilliseconds = 800, int repeatDelayMilliseconds = 50)
            : base(game)
        {
            InitialDelay = initialDelayMilliseconds;
            RepeatDelay = repeatDelayMilliseconds;
        }
        
        private Keys _previousKey;
        private TimeSpan _lastPressTime;
        private bool _isInitial;
        private KeyboardState _previousState;

        public event EventHandler<KeyboardEventArgs> KeyTyped;
        public event EventHandler<KeyboardEventArgs> KeyPressed;
        public event EventHandler<KeyboardEventArgs> KeyReleased;

        public int InitialDelay { get; }
        public int RepeatDelay { get; }

        public override void Update(GameTime gameTime)
        {
            var currentState = Keyboard.GetState();
            
            RaisePressedEvents(gameTime, currentState);
            RaiseReleasedEvents(currentState);
            RaiseRepeatEvents(gameTime, currentState);

            _previousState = currentState;
        }

        private void RaisePressedEvents(GameTime gameTime, KeyboardState currentState)
        {
            if (!currentState.IsKeyDown(Keys.LeftAlt) && !currentState.IsKeyDown(Keys.RightAlt))
            {
                var pressedKeys = Enum.GetValues(typeof (Keys))
                    .Cast<Keys>()
                    .Where(key => currentState.IsKeyDown(key) && _previousState.IsKeyUp(key));

                foreach (var key in pressedKeys)
                {
                    var args = new KeyboardEventArgs(key, currentState);

                    KeyPressed.Raise(this, args);

                    if (args.Character.HasValue)
                        KeyTyped.Raise(this, args);

                    _previousKey = key;
                    _lastPressTime = gameTime.TotalGameTime;
                    _isInitial = true;
                }
            }
        }

        private void RaiseReleasedEvents(KeyboardState currentState)
        {
            var releasedKeys = Enum.GetValues(typeof (Keys))
                .Cast<Keys>()
                .Where(key => currentState.IsKeyUp(key) && _previousState.IsKeyDown(key));

            foreach (var key in releasedKeys)
                KeyReleased.Raise(this, new KeyboardEventArgs(key, currentState));
        }

        private void RaiseRepeatEvents(GameTime gameTime, KeyboardState currentState)
        {
            var elapsedTime = (gameTime.TotalGameTime - _lastPressTime).TotalMilliseconds;

            if (currentState.IsKeyDown(_previousKey) && ((_isInitial && elapsedTime > InitialDelay) || (!_isInitial && elapsedTime > RepeatDelay)))
            {
                var args = new KeyboardEventArgs(_previousKey, currentState);

                if (args.Character.HasValue)
                    KeyTyped.Raise(this, args);

                _lastPressTime = gameTime.TotalGameTime;
                _isInitial = false;
            }
        }
    }
}