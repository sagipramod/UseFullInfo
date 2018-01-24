using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Suncor.OPE.Data;
using Suncor.Framework.Data;
using Suncor.OPE.Entities;

namespace Suncor.OPE.BusinessService.LoyaltyAction
{
    public abstract class LoyaltyAction
    {
        public string LoyaltyCardNumber { get; set; }
        public int ProductId { get; set; }
        public string LinkedCardNumber { get; set; }
        public string BonusType { get; set; }
        public string CardType { get; set; }


        public LoyaltyAction(string loyaltyCardNumber, int productId,string linkedCardNumber, string bonusType, string cardType)
        {
            this.LoyaltyCardNumber = loyaltyCardNumber;
            this.ProductId = productId;
            this.LinkedCardNumber = linkedCardNumber;
            this.BonusType = bonusType;
            this.CardType = cardType;
        }
        
        public LoyaltyAction(string loyaltyCardNumber, int productId, string bonusType, string linkedCardNumber)
        {
            LoyaltyCardNumber = loyaltyCardNumber;
            ProductId = productId;
            BonusType = bonusType;
            LinkedCardNumber = linkedCardNumber;
        }
        public virtual bool QualifyForBonus()
        {
            return true;
        }
        public virtual bool UpdateBonusGivenDate()
        {
            return true;
        }
    }
}
