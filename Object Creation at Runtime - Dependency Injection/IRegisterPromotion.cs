using Suncor.OPE.Entities;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// Interface object for Register Promotion
    /// </summary>
    public interface IRegisterPromotion
    {
        /// <summary>
        /// Register Promotion Method
        /// </summary>
        /// <param name="registerPromotionRequest"></param>
        /// <returns></returns>
        MemberPromotion RegisterPromotion(MemberPromotion registerPromotionRequest);

    }
}
