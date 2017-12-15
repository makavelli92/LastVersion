using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelStrategy.Model
{
    public class Order
    {
        public string classCode;

        public string security;

        public double price;

        public string operation;

        public int quantity;

        public long transactionId = -1;

        public long StopProfitId = -1;

        public double deleteLevel;

        public long numberOrder = -1;

        public long numberStopProfitOrder = -1;
        
        public Order(string classCode, string security, double price, string operation, int quantity, int transId)
        {
            this.classCode = classCode;
            this.security = security;
            this.price = price;
            this.operation = operation;
            this.quantity = quantity;
            transactionId = transId;
        }
        public Order(string classCode, string security, double price, string operation, int quantity, double deleteLevel)
        {
            this.classCode = classCode;
            this.security = security;
            this.price = price;
            this.operation = operation;
            this.quantity = quantity;
            this.deleteLevel = deleteLevel;
        }

        public bool OnlyOrder
        {
            get
            {
                return transactionId > 0 && StopProfitId == -1;
            }
        }
    }
}
