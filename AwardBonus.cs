using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Suncor.OPE.BusinessService.LoyaltyAction;
using Suncor.OPE.Entities;
using Suncor.OPE.Lib;
using Suncor.Framework.Lib;
using System.Web;

namespace Suncor.OPE.BusinessService.LoyaltyPoints
{
    /// <summary>
    /// A class that award bonus points based on an action (link / reload card) member did 
    /// </summary>
    public class AwardBonus
    {

        LogMessage _logMsg;
        IFormatXmlUtility _objIFormatXmlUtility;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public AwardBonus(LoyaltyAction.LoyaltyAction action)
        {
            this.Action = action;


            _objIFormatXmlUtility = new FormatXmlUtility();

            //This class is also called from Batch, where HttpContext is not available, hence putting this if condition.
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["sessionKey"] == null)
                    HttpContext.Current.Session["sessionKey"] = RetalixSessionManagement.CreateSessionKey();

                _logMsg = new LogMessage
                {
                    SessionKey = (string)HttpContext.Current.Session["sessionKey"],
                    Module = StaticData.ManageHouseholdModule,
                    ClassName = GetType().Name
                };
            }
            else
            {
                _logMsg = new LogMessage
                {
                    SessionKey = "Batch",
                    Module = StaticData.ManageHouseholdModule,
                    ClassName = GetType().Name
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public LoyaltyAction.LoyaltyAction Action { get; set; }

        
        /// <summary>
        /// 
        /// </summary>
        public AwardBonus()
        {
            _objIFormatXmlUtility = new FormatXmlUtility();

            //This class is also called from Batch, where HttpContext is not available, hence putting this if condition.
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session["sessionKey"] == null)
                    HttpContext.Current.Session["sessionKey"] = RetalixSessionManagement.CreateSessionKey();

                _logMsg = new LogMessage
                {
                    SessionKey = (string)HttpContext.Current.Session["sessionKey"],
                    Module = StaticData.ManageHouseholdModule,
                    ClassName = GetType().Name
                };
            }
            else
            {
                _logMsg = new LogMessage
                {
                    SessionKey = "Batch",
                    Module = StaticData.ManageHouseholdModule,
                    ClassName = GetType().Name
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            if (Action == null)
            {
                throw new ArgumentException("No loyalty action found");
            }

            DBLogging.Info(_logMsg, "1 to 1 Bonus - Input", "LoyaltyCardNumber :" + Action.LoyaltyCardNumber + "LinkedCard : " + Action.LinkedCardNumber + "Product Id: " + Action.ProductId.ToString() + "BonusType : " + Action.BonusType + "CardType: " + Action.CardType, "");

            bool qualify = Action.QualifyForBonus();
            DBLogging.Info(_logMsg, "1 to 1 Bonus - QualifyForBonus", "LoyaltyCardNumber :" + Action.LoyaltyCardNumber + "LinkedCard : " + Action.LinkedCardNumber + "CardType: " + Action.CardType, qualify);

            bool result = false;
            if (qualify)
            {
                // call Retalix to trigger the bonus
                var redeemMgr = new ProcessRedemptionOrReversalManager();
                string productId = Action.ProductId.ToString();
                ServiceResponse<string> serviceResp = redeemMgr.Redeem(Action.LoyaltyCardNumber, productId, "1", null, Action.BonusType);
                DBLogging.Info(_logMsg, "1 to 1 Bonus - Redeem", "LoyaltyCardNumber :" + Action.LoyaltyCardNumber + "LinkedCard : " + Action.LinkedCardNumber + "Product Id: " + productId + "CardType: " + Action.CardType, serviceResp);

                // Add Note
                if (serviceResp.IsSuccess)
                {
                    result = true;
                    //Update BonusGivenDate 
                    bool updateBonusResult = Action.UpdateBonusGivenDate();
                    DBLogging.Info(_logMsg, "1 to 1 Bonus - UpdateBonusGivenDate", "LoyaltyCardNumber :" + Action.LoyaltyCardNumber + "LinkedCard : " + Action.LinkedCardNumber + "CardType: " + Action.CardType, updateBonusResult);

                    var noteResult = AddNotes();
                }
            }

            DBLogging.Info(_logMsg, "1 to 1 Bonus - Result", "LoyaltyCardNumber :" + Action.LoyaltyCardNumber + "LinkedCard : " + Action.LinkedCardNumber + "Product Id: " + Action.ProductId.ToString() + "BonusType : " + Action.BonusType + "CardType: " + Action.CardType, result);

            return result;
        }


        private bool AddNotes()
        {

            AddNoteToHouseholdManager addNoteToHouseholdManager = new AddNoteToHouseholdManager();
            ProcessRedemptionOrReversalManager redeemMgr = new ProcessRedemptionOrReversalManager();
            DynamicNote dynamicNote = new DynamicNote();
            string subFunction = string.Empty;

            ProductDetails productData;

            //To get the Redeemed subcategory in the notes instead of the bonus subcategory #5446 fix
            productData = redeemMgr.GetProduct(Action.ProductId);
            if (Action.BonusType == "ReloadBonus")
            {
                //productData = redeemMgr.GetProductByBonus(Action.ProductId); This method is returning the Bonus product details
                subFunction = AppConstants.ManageHouseholdNoteCodes.ReloadBonus;
            }
            else
            {
                //productData = redeemMgr.GetProduct(Action.ProductId);
                subFunction = AppConstants.ManageHouseholdNoteCodes.LinkBonus;
            }

            dynamicNote.CardNumber = Action.LinkedCardNumber;
            dynamicNote.LoyaltyCardNumber = Action.LoyaltyCardNumber;
            dynamicNote.CardType = Action.CardType;
            PromotionBonusAward promotionBonusAward = new PromotionBonusAward();
            if (HttpContext.Current.Session["PromotionBonusAward"] != null)
            {
                promotionBonusAward = (PromotionBonusAward)HttpContext.Current.Session["PromotionBonusAward"];
                dynamicNote.TotalPoints = Convert.ToInt32(promotionBonusAward.EarnValue);
            }
            else
                dynamicNote.TotalPoints = Convert.ToInt32(productData.UnitsPointsRedemptionCost);

            dynamicNote.ProductSubCategory = productData?.ProductSubCategoryDescription;
            dynamicNote.ProductDescription = productData?.EnglishDescriptionBackOffice;
            var noteResult = addNoteToHouseholdManager.AddDynamicNoteToHousehold(StaticData.OPEFunction, subFunction, dynamicNote);
            DBLogging.Info(_logMsg, "1 to 1 Bonus - AddDynamicNoteToHousehold", "Function :" + StaticData.OPEFunction + "SubFunction : " + subFunction + _objIFormatXmlUtility.FormatXMLRequest(dynamicNote), noteResult);

            return true;
        }
    }
}
