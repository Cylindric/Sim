using Engine.Utilities;
using SDL2;
using System;
using System.Collections.Generic;
using static SDL2.SDL;

namespace Engine.Renderer.SDLRenderer
{
    internal static class SDLEvent
    {
        static SDLEvent()
        {
            _keyEvents = new List<SDL.SDL_Event>();
            _mouseEvents = new List<SDL.SDL_Event>();
            _events = new List<SDL.SDL_Event>();
            _downKeys = new List<SDL_Keycode>();
            _mouseButtonStates = new Dictionary<uint, bool>();
            MousePosition = new Vector2<int>();
            Quit = false;
        }

        /* #################################################################### */
        /* #                         CONSTANT FIELDS                          # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                              FIELDS                              # */
        /* #################################################################### */
        private static List<SDL.SDL_Event> _keyEvents;
        private static List<SDL.SDL_Event> _mouseEvents;
        private static List<SDL.SDL_Event> _events;
        private static List<SDL_Keycode> _downKeys;
        private static Dictionary<uint, bool> _mouseButtonStates;

        /* #################################################################### */
        /* #                           CONSTRUCTORS                           # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                             DELEGATES                            # */
        /* #################################################################### */

        /* #################################################################### */
        /* #                            PROPERTIES                            # */
        /* #################################################################### */

        /// <summary>
        /// Has SDL seen a Quit event this tick?
        /// </summary>
        public static bool Quit { get; private set; }

        /// <summary>
        /// The last seen position of the mouse.
        /// </summary>
        public static Vector2<int> MousePosition { get; private set; }

        /* #################################################################### */
        /* #                              METHODS                             # */
        /* #################################################################### */
        public static void Update()
        {
            _keyEvents.Clear();
            _events.Clear();
            _mouseEvents.Clear();
            Quit = false;

            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    Quit = true;
                }

                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    if (_downKeys.Contains(e.key.keysym.sym) == false)
                    {
                        _downKeys.Add(e.key.keysym.sym);
                    }
                    _keyEvents.Add(e);
                }
                else if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    if (_downKeys.Contains(e.key.keysym.sym))
                    {
                        _downKeys.Remove(e.key.keysym.sym);
                    }
                    _keyEvents.Add(e);
                }
                else if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                {
                    //Get mouse position
                    SDL_GetMouseState(out int x, out int y);
                    MousePosition = new Vector2<int>(x, y);
                    _mouseEvents.Add(e);
                }
                else if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    _mouseButtonStates[e.button.button] = true;
                    _mouseEvents.Add(e);
                }
                else if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
                {
                    _mouseButtonStates[e.button.button] = false;
                    _mouseEvents.Add(e);
                }
                else
                {
                    _events.Add(e);
                }
            }
        }

        public static bool KeyState(SDL_Keycode key)
        {
            return _downKeys.Contains(key);
        }

        public static bool MouseButtonWentDown(uint button)
        {
            foreach (var e in _mouseEvents)
            {
                if (e.button.button == button && e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool MouseButtonWentUp(uint button)
        {
            foreach (var e in _mouseEvents)
            {
                if (e.button.button == button && e.type == SDL_EventType.SDL_MOUSEBUTTONUP)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool MouseButtonIsDown(uint button)
        {
            if (_mouseButtonStates.ContainsKey(button))
            {
                return _mouseButtonStates[button];
            }
            else
            {
                return false;
            }
        }

        public static bool KeyUp(SDL_Keycode key)
        {
            foreach(var k in _keyEvents)
            {
                if (k.key.keysym.sym == key)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
