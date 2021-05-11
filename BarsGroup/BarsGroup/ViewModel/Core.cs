using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarsGroup.Model;

namespace BarsGroup.ViewModel
{
    public static class Core
    {
        //Рабочий вариант
        public static MainEntities db { get; set; }  = new MainEntities();

        /*
        private static MainEntities privateDb;
        public static MainEntities db 
        {
            get 
            {
                privateDb = new MainEntities();
                return privateDb;
            }
            set { privateDb = new MainEntities(); } 
        }
        */

        public static Users CurrentUser { get; set; } = new Users();
    }
}
