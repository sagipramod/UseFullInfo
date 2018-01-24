using System;
using Suncor.OPE.Entities;
using System.Collections.Generic;
using Suncor.Framework.Lib;
using System.Web;
using Suncor.OPE.Lib;
using System.Reflection;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// Business Process for Register CStore Promotions
    /// </summary>
    public class RegisterCStorePromotion : ValidateRegisterPromotion, IRegisterPromotion
    {
        ICallRetalixServices iRetalixServices;
        LogMessage _logMsg;

        /// <summary>
        /// Constructor
        /// </summary>
        public RegisterCStorePromotion()
        {
            //This class is also called from Batch, where HttpContext is not available, hence putting this if condition.
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["sessionKey"] == null)
                    HttpContext.Current.Session["sessionKey"] = RetalixSessionManagement.CreateSessionKey();

                _logMsg = new LogMessage
                {
                    SessionKey = (string)HttpContext.Current.Session["sessionKey"],
                    Module = "Register to Promotion",
                    ClassName = GetType().Name,
                };
            }
            else
            {
                _logMsg = new LogMessage
                {
                    SessionKey = "Batch",
                    Module = "Register to Promotion",
                    ClassName = GetType().Name,
                };
            }
        }

        /// <summary>
        ///  Method to Register Promotion
        /// </summary>
        /// <param name="registerPromotionRequest"></param>
        /// <returns></returns>
        public MemberPromotion RegisterPromotion(MemberPromotion registerPromotionRequest)
        {
            _logMsg.MethodName = MethodBase.GetCurrentMethod().Name;
            ProcessEmailCampaignManager processEmailCampaignManager = new ProcessEmailCampaignManager();

            // Set the registration flag to default as failed
            int registrationStatusCode = 0;

            List<int> segmentList = null;
            string promotionID = registerPromotionRequest.PromotionID;
            string loyaltyCardNumber = registerPromotionRequest.LoyaltyCardNumber;
            string houseHoldId = loyaltyCardNumber.Substring(4, 9);
            string individualId = loyaltyCardNumber.Substring(13, 2);

            iRetalixServices = new CallRetalixServices();

            //Check if promotion is found in OPE
            bool checkpromotionInOPE = CheckPromotionInOPE(promotionID);
            if (checkpromotionInOPE)
            {
                //Get Segments configured for the promotion
                segmentList = GetSegmentsForPromotion(promotionID);
                if (segmentList?.Count > 0)
                {
                    // Verify the segment is found at member or household segment level
                    bool isMemberAuthorized = VerifyMemberIsAuthorized(loyaltyCardNumber, segmentList);

                    if (isMemberAuthorized)
                    {
                        //Register promotion in LMS
                        var registerResult = iRetalixServices.RegisterPromotion(loyaltyCardNumber, promotionID);
                        if (!registerResult.IsSuccess)
                        {
                            DBLogging.Warn(_logMsg, "Register Promotion Failed", "Loyalty Card Number :" + loyaltyCardNumber + ", Promotion Id :" + promotionID, registerResult.ResDescription);
                            processEmailCampaignManager.LogLoyaltyTransaction("Promotion Registration in Retalix Failed for the given Member", individualId, loyaltyCardNumber, houseHoldId);
                            registerPromotionRequest.IsSuccess = false;
                            registerPromotionRequest.RegistrationResult = registerResult.ResDescription;
                        }
                        else
                        {
                            var synchHouseholdBonus= processEmailCampaignManager.SyncHouseholdBonus(AppConstants.ConstantValues.ActionCodeA, individualId, houseHoldId, loyaltyCardNumber, promotionID);
                            if (synchHouseholdBonus < 1)
                            {
                                DBLogging.Warn(_logMsg, "Sync Household Bonuses Failed", "Action Code : R" + ", Household Id :" + houseHoldId + ", Individual Id :" + individualId + ", Loyalty Card Number :" + loyaltyCardNumber + ", MOD Promotion Id :" + promotionID, AppConstants.DisplayMessages.SyncToMobillaFailed);
                                registerPromotionRequest.IsSuccess = false;
                                registerPromotionRequest.RegistrationResult = AppConstants.DisplayMessages.SyncToMobillaFailed;
                            }
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
            }
            else
            {
                registerPromotionRequest.IsSuccess = false;
                registerPromotionRequest.RegistrationResult = "Promotion Not Found in OPE";
            }
            registerPromotionRequest.RegistrationResult = registrationStatusCode.ToString();
            return registerPromotionRequest;
        }
    }
}
