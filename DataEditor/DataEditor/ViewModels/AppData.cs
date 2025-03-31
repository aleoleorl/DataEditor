using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEditor.ViewModels
{
    public class AppData
    {
        private static AppData _instance;

        private AppData()
        {
            Wnd = null;
        }

        public static AppData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppData();
                }
                return _instance;
            }
        }

        public Window Wnd { get; set; }
    }
}
