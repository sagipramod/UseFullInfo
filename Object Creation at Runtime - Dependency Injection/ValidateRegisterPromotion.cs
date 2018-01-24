using Suncor.Framework.Data;
using Suncor.Framework.Lib;
using Suncor.OPE.Data;
using Suncor.OPE.Entities;
using Suncor.OPE.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;

namespace Suncor.OPE.BusinessService.PromotionServices
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidateRegisterPromotion
    {
        readonly GetMemberDemographicsManager objGetMemberDemographicsManager = new GetMemberDemographicsManager();
        LogMessage _logMsg;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidateRegisterPromotion()
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
        ///  Verify the Member is registered for given segment
        /// </summary>
        /// <returns></returns>
        public bool VerifyMemberIsAuthorized(string loyaltyCardNumber, List<int> segmentsList)
        {
            try
            {
                bool isMemberAuthorized = false;

                HouseHold objHousehold = objGetMemberDemographicsManager.GetDemographicData(loyaltyCardNumber).Result;

                MemberSegments memberSegments = objHousehold?.Members?.MemberMain.FirstOrDefault().MemberSegments;

                var houseHoldSegment = objHousehold?.HouseHoldSegments;
                
                foreach (int segmentId in segmentsList)
                {
                    if ((memberSegments != null && memberSegments.Segment.Any(x => x.Id == Convert.ToInt32(segmentId)))
                        || (houseHoldSegment != null && houseHoldSegment.Segment.Count > 0 && houseHoldSegment.Segment.Any(x => x.Id == Convert.ToInt32(segmentId))))
                    {
                        isMemberAuthorized = true;
                        break;
                    }
                }

                return isMemberAuthorized;

            }catch(Exception ex)
            {
                DBLogging.Error("Validate Register Promotion", ex);

                throw;
            }
        }
        
        /// <summary>
        /// Helper Method to parse the message and get the promotion count
        /// </summary>
        /// <param name="promotionId"></param>
        /// <param name="promotions"></param>
        /// <returns></returns>
        public int GetMemberPromotionsCount(string promotionId, string promotions)
        {
            int redemptionCount = 0;
            XmlDocument _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(promotions);
            var singlePromotion = _xmlDocument.SelectSingleNode("//Promotions/Promotion[@id='" + promotionId + "']");

            if (singlePromotion == null)
                redemptionCount = -1;
            else
            {
                var xmlAttributeCollection = singlePromotion?.Attributes;
                redemptionCount = Convert.ToInt16(xmlAttributeCollection["Redemptions"].Value);
            }
            return redemptionCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="promotionHeaderId"></param>
        /// <returns></returns>
        public bool CheckPromotionInOPE(string promotionHeaderId)
        {
            _logMsg.MethodName = MethodBase.GetCurrentMethod().Name;
            MobilaPromotion mobPromotion = null;
            using (DBConnection conn = new DBConnection())
            {
                BackOfficeAdminData backOfficeAdminData = new BackOfficeAdminData(conn);
                mobPromotion = backOfficeAdminData.ValidatePromotion(promotionHeaderId);
            }
            if (mobPromotion == null)
            {
                DBLogging.Warn(_logMsg, "Validate Promotion Failed", "Promotion Id :" + promotionHeaderId, "Promotion Not Found in OPE");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Get all segment id's which are configured for the promotion
        /// </summary>
        /// <param name="promotionId"></param>
        /// <returns></returns>
        public List<int> GetSegmentsForPromotion(string promotionId)
        {
            _logMsg.MethodName = MethodBase.GetCurrentMethod().Name;
            List<int> segmentList = null;
            using (DBConnection conn = new DBConnection())
            {
                BackOfficeAdminData backOfficeAdminData = new BackOfficeAdminData(conn);
                segmentList = backOfficeAdminData.GetSegmentsForPromotion(promotionId);
            }
            if (segmentList == null)
                DBLogging.Warn(_logMsg, "GetSegmentsForPromotion", "Promotion Id :" + promotionId, segmentList);

            return segmentList;
        }
    }
}
