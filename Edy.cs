using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FeliCa2Money
{
    class Edy : Card
    {
        public Edy()
        {
            ident = "Edy";
            cardName = "Edy";
        }

        public override List<Transaction> ReadCard()
        {
            SfcPeep s = new SfcPeep();
            List<string> lines = s.Execute("-e");

            if (lines[0].Substring(0, 4) != "EDY:")
	        {
                return null;
            }

            CardId = lines[0].Substring(4);

            lines.RemoveAt(0);
            lines.Reverse();

            // Parse lines
            List<Transaction> transactions = new List<Transaction>();
            foreach (string line in lines)
            {
                Transaction t = new Transaction();

                string[] items = ParseLine(line);
                if (SetTransaction(t, items)) {
                    transactions.Add(t);
                }
            }
            return transactions;
        }

        private bool SetTransaction(Transaction t, string[] items)
        {
      	    // 0:処理,1:日付時刻,2:今回取引額,3:チャージ残高, 4:取引連番
            // ET00:ﾁｬｰｼﾞ	2007年03月14日23時08分16秒	24000	49428	59

            t.id = int.Parse(items[4]);

            string d = items[1];
            int yy = int.Parse(d.Substring(0, 4));
            int mm = int.Parse(d.Substring(5, 2));
            int dd = int.Parse(d.Substring(8, 2));
            int h = int.Parse(d.Substring(11, 2));
            int m = int.Parse(d.Substring(14, 2));
            int s = int.Parse(d.Substring(17, 2));

            t.date = new DateTime(yy, mm, dd, h, m, s);

            t.desc = items[0].Substring(5);
            if (t.desc == "----") {
                return false;   // empty
            }
            t.memo = t.desc;

            if (t.desc == "支払") {
                t.GuessTransType(false);
                t.value = - int.Parse(items[2]);
            }
            else
            {
                t.GuessTransType(true);
		        t.value = int.Parse(items[2]);
            }
            t.balance = int.Parse(items[3]);

            return true;
        }
    }
}
