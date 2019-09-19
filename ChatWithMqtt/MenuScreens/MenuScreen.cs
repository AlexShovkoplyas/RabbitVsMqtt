using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChatWithMqtt.UserInterface;

namespace ChatWithMqtt.MenuScreens
{
    public abstract class MenuScreen
    {
        private IChatInterface _chatUi;
        public List<MenuItem> MenuItems { get; set; }

        protected MenuScreen(IChatInterface chatUi)
        {
            _chatUi = chatUi;
        }

        public virtual void View()
        {
            var menuItems = MenuItems;

            _chatUi.PrintMenuItems(menuItems.Select(i => (i.Number, i.Title)));

            while (true)
            {
                var result = _chatUi.SelectMenuOption();

                var regex = new Regex(@"(\d+)\s*([\S]*)");

                var match = regex.Match(result);

                var option = match.Groups[1].Value;
                var argument = match.Groups[2].Value;

                var menuItem = menuItems.Single(i => i.Number == option);

                menuItem.Command(argument);

                if (menuItem.NextMenuScreen != null && menuItem.NextMenuScreen.GetType() != this.GetType())
                {
                    menuItem.NextMenuScreen.View();
                    return;
                }
            }
        }

        public virtual void View(string command)
        {
            var menuItems = MenuItems;

            var regex = new Regex(@"(\d+)\s*([\S]*)");

            var match = regex.Match(command);

            var option = match.Groups[1].Value;
            var argument = match.Groups[2].Value;

            var menuItem = menuItems.Single(i => i.Number == option);

            menuItem.Command(argument);

            if (menuItem.NextMenuScreen != null && menuItem.NextMenuScreen.GetType() != this.GetType())
            {
                menuItem.NextMenuScreen.View();
                return;
            }
        }
    }
}