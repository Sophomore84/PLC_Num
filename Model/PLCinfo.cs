using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
   public class PLCinfo
    {
        private DateTime date;
        private string num;

        public DateTime Date { get => date; set => date = value; }
        public string Num { get => num; set => num = value; }
    }
}
