using System;
using System.Collections.Generic;
using System.Text;

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
}
