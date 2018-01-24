using Suncor.Framework.Data;
using Suncor.OPE.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suncor.OPE.BusinessService.LoyaltyAction
{
    public class LinkPartnerCard:LoyaltyAction
    {
        public LinkPartnerCard(string loyaltyCardNumber, int productId, string linkedCardNumber, string bonusType, string cardType) :base(loyaltyCardNumber,productId, linkedCardNumber, bonusType, cardType)
        {
            this.LoyaltyCardNumber = loyaltyCardNumber;
            this.ProductId = productId;
            this.LinkedCardNumber = linkedCardNumber;
            this.CardType = cardType;
        }
        ///// <summary>
        ///// Check if the card is qualify for bonus (ie have the card received the bonus or not.
        ///// </summary>
        ///// <returns><c>true</c> if qualitifed, <c>false</c> otherwise.</returns>
        //public override bool QualifyForBonus()
        //{
        //    using (DBConnection conn = new DBConnection())
        //    {
        //        var cardsManagerData = new ManageCardsData(conn);
        //        var cardDetails = cardsManagerData.CheckIfPartnerCardExists(LinkedCardNumber);

               
        //        if (cardDetails == null || cardDetails.BonusGivenDate != null) return false;
        //    }
        //    return true;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <returns><c>true</c> if qualitifed, <c>false</c> otherwise.</returns>
        public override bool UpdateBonusGivenDate()
        {
            using (DBConnection conn = new DBConnection())
            {
                var cardsManagerData = new ManageCardsData(conn);
                int updateBonusDate = cardsManagerData.UpdatePartnerBonusdate(LinkedCardNumber);

                if (updateBonusDate < 1) return false;
            }
            return true;
        }
    }
}
