using Suncor.OPE.Entities;
using Suncor.OPE.Data;
using System.Collections.Generic;
using Suncor.Framework.Data;
using Suncor.OPE.Lib;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// Business Process for Register Online Promotion
    /// </summary>
    public class RegisterOnlinePromotion : ValidateRegisterPromotion, IRegisterPromotion
    {

        ICallRetalixServices iRetalixServices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerPromotionRequest"></param>
        /// <returns></returns>
        public MemberPromotion RegisterPromotion(MemberPromotion registerPromotionRequest)
        {
            List<int> segmentList = null;

            string promotionID = registerPromotionRequest.PromotionID;
            string loyaltyCardNumber = registerPromotionRequest.LoyaltyCardNumber;

            iRetalixServices = new CallRetalixServices();

            // Get the Segment list for the given Promotion            
            segmentList = GetSegmentsForPromotion(promotionID);

            // Continue only if there is a segment found for the given promotion
            if (segmentList?.Count > 0)
            {
                // Verify the segment is found at member or household segment level
                bool isMemberAuthorized = VerifyMemberIsAuthorized(loyaltyCardNumber, segmentList);

                // if found, then register the promotion in LMS
                if (isMemberAuthorized)
                {
                    var registerResult = iRetalixServices.RegisterPromotion(loyaltyCardNumber, promotionID);

                    // if failed to register promotion, check if promotion is already registered or Activated by calling GetMemberPromotion
                    if (!registerResult.IsSuccess)
                    {
                        int promoCheck = CheckPromotionActivated(loyaltyCardNumber, promotionID);

                        // TODO : Assign the Actual Error codes instead of 0,1,2 for the below conditions
                        if (promoCheck == -1)
                        {
                            // Failed
                            registerPromotionRequest.IsSuccess = false;
                            registerPromotionRequest.RegistrationResult = "Promotion Registration Failed";
                        }
                        else if (promoCheck == 0)
                        {
                            // Registered Successfully , Promotion Not Activated or Not Used
                            registerPromotionRequest.IsSuccess = true;
                            registerPromotionRequest.RegistrationResult = "Registered Successfully , Promotion Not Activated or Not Used";
                        }
                        else if (promoCheck > 0)
                        {
                            // Already Registered Successfully and Promotion Activate / Used
                            registerPromotionRequest.IsSuccess = true;
                            registerPromotionRequest.RegistrationResult = "Already Registered Successfully and Promotion Activate";
                        }
                    }
                    else
                    {
                        // Registered Successfully                        
                        registerPromotionRequest.IsSuccess = true;
                        registerPromotionRequest.RegistrationResult = "Promotion Registeration Successfull";
                    }
                }
                else
                {
                    registerPromotionRequest.IsSuccess = false;
                    registerPromotionRequest.RegistrationResult = AppConstants.DisplayMessages.MemberNotAuthorizedForPromotion;
                }
            }
            else
            {
                registerPromotionRequest.IsSuccess = false;
                registerPromotionRequest.RegistrationResult = AppConstants.DisplayMessages.SegmentNotFound;
            }
            return registerPromotionRequest;
        }

        /// <summary>
        /// Method to check Member activated the given promotion in LMS
        /// </summary>
        /// <param name="loyaltyCardNumber"></param>
        /// <param name="promotionID"></param>
        /// <returns></returns>
        private int CheckPromotionActivated(string loyaltyCardNumber, string promotionID)
        {
            int promoCount = -1;
            var memberPromotion = iRetalixServices.GetMemberPromotions(loyaltyCardNumber);

            if (memberPromotion.IsSuccess)
            {
                string registeredPromotions = memberPromotion.Result;

                promoCount = GetMemberPromotionsCount(promotionID, registeredPromotions);
            }

            return promoCount;
        }
        
    }
}
