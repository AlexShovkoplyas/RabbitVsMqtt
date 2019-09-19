using System;
using ChatWithMqtt.MenuScreens;

namespace ChatWithMqtt
{
    public class MenuItem
    {
        public MenuItem(string number, string title, Action<string> command, MenuScreen screen)
        {
            (Number, Title, Command, NextMenuScreen) = (number, title, command, screen);
        }

        public string Number { get; set; }

        public string Title { get; set; }

        public Action<string> Command { get; set; }

        public MenuScreen NextMenuScreen { get; set; }
    }
}