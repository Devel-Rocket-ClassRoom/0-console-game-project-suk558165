using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Framework.Engine
{
    public static class Input
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static readonly HashSet<ConsoleKey> s_currentKeys = new();
        private static readonly HashSet<ConsoleKey> s_previousKeys = new();

        private static readonly ConsoleKey[] s_trackedKeys =
        {
            // 방향키
            ConsoleKey.UpArrow, ConsoleKey.DownArrow,
            ConsoleKey.LeftArrow, ConsoleKey.RightArrow,

            // 숫자키 (일반)
            ConsoleKey.D0, ConsoleKey.D1, ConsoleKey.D2, ConsoleKey.D3, ConsoleKey.D4,
            ConsoleKey.D5, ConsoleKey.D6, ConsoleKey.D7, ConsoleKey.D8, ConsoleKey.D9,

            // 숫자키 (넘패드)
            ConsoleKey.NumPad0, ConsoleKey.NumPad1, ConsoleKey.NumPad2, ConsoleKey.NumPad3,
            ConsoleKey.NumPad4, ConsoleKey.NumPad5, ConsoleKey.NumPad6, ConsoleKey.NumPad7,
            ConsoleKey.NumPad8, ConsoleKey.NumPad9,

            // 특수키
            ConsoleKey.Enter, ConsoleKey.Escape, ConsoleKey.Spacebar,
            ConsoleKey.Tab, ConsoleKey.Backspace,

            // 영문자
            ConsoleKey.H, ConsoleKey.S, ConsoleKey.Y, ConsoleKey.N,
            ConsoleKey.W, ConsoleKey.A, ConsoleKey.D,
        };

        public static bool HasInput => s_currentKeys.Count > 0;

        public static void Poll()
        {
            s_previousKeys.Clear();
            foreach (var key in s_currentKeys)
            {
                s_previousKeys.Add(key);
            }

            s_currentKeys.Clear();

            foreach (var key in s_trackedKeys)
            {
                short state = GetAsyncKeyState((int)key);
                if ((state & 0x8000) != 0)
                {
                    s_currentKeys.Add(key);
                }
            }

            // Console 입력 버퍼 drain (잔여 키 방지)
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// 이번 프레임에 눌려있는지 (held)
        /// </summary>
        public static bool IsKey(ConsoleKey key)
        {
            return s_currentKeys.Contains(key);
        }

        /// <summary>
        /// 이전 프레임에 안 눌렸다가 이번 프레임에 눌린 순간 (edge-triggered)
        /// </summary>
        public static bool IsKeyDown(ConsoleKey key)
        {
            return s_currentKeys.Contains(key) && !s_previousKeys.Contains(key);
        }

        /// <summary>
        /// 이전 프레임에 눌렸다가 이번 프레임에 뗀 순간
        /// </summary>
        public static bool IsKeyUp(ConsoleKey key)
        {
            return !s_currentKeys.Contains(key) && s_previousKeys.Contains(key);
        }
    }
}
