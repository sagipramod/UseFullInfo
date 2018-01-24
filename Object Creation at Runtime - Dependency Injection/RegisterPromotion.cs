using Suncor.OPE.Entities;
using Suncor.OPE.Entities.Enum;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// Register Promotion Business Process
    /// </summary>
    public class RegisterPromotion
    {
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="promotionRequest"></param>
        /// <returns></returns>
        public virtual IRegisterPromotion CreateRegisterPromotion(MemberPromotion promotionRequest)
        {
            IRegisterPromotion promotion = null;

            switch (promotionRequest.PromoType)
            {
                case PromotionType.CSTORE:
                    promotion = new RegisterCStorePromotion();
                    break;

                case PromotionType.ONLINE:
                    promotion = new RegisterOnlinePromotion();
                    break;
            }

            return promotion;
        }
    }
}
