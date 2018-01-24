using Suncor.Framework.Lib;
using Suncor.OPE.Entities;
using Suncor.OPE.Entities.Enum;
using System;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// Business Process to Register Promotion
    /// </summary>
    public class ProcessRegisterPromotion 
    {
        IRegisterPromotion iRegisterPromotion = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerPromotionRequest"></param>
        /// <returns></returns>
        public MemberPromotion RegisterPromotion(MemberPromotion registerPromotionRequest)
        {
            try
            {
                RegisterPromotion registerPromoBS = new RegisterPromotion();

                this.iRegisterPromotion = registerPromoBS.CreateRegisterPromotion(registerPromotionRequest);

                var memberPromotion = this.iRegisterPromotion.RegisterPromotion(registerPromotionRequest);

                return memberPromotion;
            }
            catch(Exception ex)
            {
                DBLogging.Error("Register Promotion", ex);
                throw;
            }
        }
    }
}
