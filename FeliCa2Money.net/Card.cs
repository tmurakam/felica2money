using System;
using System.Collections.Generic;
using System.Text;
using FelicaLib;

namespace FeliCa2Money
{
    abstract class Card
    {
        protected string ident;
        protected string cardName;
        protected string cardId;

        public abstract List<Transaction> ReadCard();

        public string Ident
        {
            get { return this.ident; }
        }

        public string CardName
        {
            get { return this.cardName; }
        }
        
        public string CardId
        {
            set { this.cardId = value; }
            get { return this.cardId; }
        }

        protected string[] ParseLine(string line)
        {
            return line.Split('\t');
        }
    }

    abstract class CardWithFelicaLib : Card
    {
	protected int systemCode;   // システムコード
	protected int serviceCode;  // サービスコード
	protected bool needReverse; // レコード順序を逆転するかどうか

	// カード ID 取得
	public abstract void analyzeCardId(Felica f);

	// Transaction 解析
	public abstract void analyzeTransaction(Transaction t, byte[] data);

	public override List<Transaction> ReadCard()
	{
	    List<Transaction> list = new List<Transaction>();

	    using (Felica f = new Felica())
	    {
		f.Polling(systemCode);
		analyzeCardId(f);

		for (int i = 0; ; i++)
		{
		    byte[] data = f.ReadWithoutEncryption(serviceCode, i);
		    if (data == null) break;

		    Transaction t = new Transaction();
		    analyzeTransaction(t, data);
		    list.Add(t);
		}
	    }
	    if (needReverse)
	    {
		list.Reverse();
	    }

	    return list;
	}
    }
}
