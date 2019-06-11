using BullfrogAPI.SharedViewModels.PartsOrderFulfilment;
using BullfrogAPI.SharedViewModels.PartsOrderQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BullfrogAPI.SharedViewModels.ConcurrencyManagement;
using BullfrogAPI.SharedViewModels.InventoryManagement;

namespace BullfrogAPI.Data.Interfaces
{
    public interface IPartsOrderFulfilmentReository
    {

        //PartsOrderFulfilmentQueue PickOrder();
        PartsOrderFulfilmentQueue PickOrder(string skipOrders);

        bool LockOrder(ConcurrencyManagement param);
        List<PartsOrderFulfilmentQueue> GetPartsOrderFulfilmentQueue(PartsOrderFulfilmentQueue searchParam, bool PopulateOnlyMasterData);

        //List<PartsOrderFulfilmentQueue> GetPartsOrderFulfilmentMasterQueue(string searchParam, string orderSet);
        MasterQueueViewModel GetPartsOrderFulfilmentMasterQueue(string searchParam, string orderSet);
        List<ManufacturingQueueItem> GetManufacturingQueue();

        List<string> GetAllShipmentMethods();

        List<string> GetPaymentOptions();

        ShipPartsOrder GetShipPartsOrder(string orderNumber);
        string UpdateCartLineItem(PartsOrderFulfilmentLineItems cartLineItem, List<PartsOrderFulfilmentLineItems> lineItemsFromLocations);

        bool ValidateOrder(int orderID);

        bool UpdateMasterCartStatus(int orderNumber, bool hasHoldTime);

        bool SaveBoxes(int invoice, string id, string boxNumber);

        List<PartsOrderFulfilmentLineItems> GetBoxList(int invoice,  string lintItems);
        List<PartBarcodeData> GetLineItemBarCodes(string recordNumbers);

        SubmitPartsOrderValidationResult GetOrderValidation(int invoice);

        PartsOrderSubmit GetFinalizeCartItems(int invoice);

        //void RemoveFromCart(int invoice, string userName);
        bool Reassign(int invoice, string userName);

        List<PartsOrderFulfilmentLineItems> GetMasterLineItems(int invoice);
        List<PartsOrderFulfilmentLineItems> GetMasterAllLineItems(int invoice);

        bool IfExists(int invoice);

        void LockCartOrder(int invoice);
        void SaveInternalNotes(int invoice, string internalNotes);

        string GetDSR(string dealerId);

        string GetSessionState(int invoice, string currentScreen, string decission);

        void LockOrderFromMasterQueue(string invoices);

        ShipPartsOrder GetWorldShipInformation(int orderNumber);

        List<PartsOrderFulfilmentQueue> GetPartsOrderFulfilmentMasterQueueByPartNumber(string partNumber);

        List<PartsOrderFulfilmentLineItems> GetMasterAllLineItemsByOrders(string invoices);

        double GetOrderTotalAmount(int invoice);
        int GetLineItemsShipmentInDay();

        ShippingList GetLatestShippingList(string invoice);

        void AddTotalNumberOfBoxes(int totalBoxes, int invoice);
        int GetTotalNoOfBoxes(int invoice, string lineItems);
        int GetBoxCountForLineItems(int invoice, string lineItems);

        List<PartsSupportingDocuments> GetPartsSupportingDocuments();
        
        void UpdatePartsDocumentStatus(PartsSupportingDocuments p);

        void AddPartsDocument(PartsSupportingDocuments p);

        List<PartsOrderFulfillmentReporting> GetWeeklyReport(string startdate, string enddate);
        List<PartsOrderFulfillmentReporting> GetDailyReport(string startdate, string enddate);
        List<PartsOrderFulfillmentReporting> GetReportByUser(string startdate, string enddate);

        List<PartsOrderFulfillmentReportingXDate> GetDatesXes(string startdate, string enddate);
        List<PartsOrderFulfillmentReporting> GetReportByUserTotal(string startdate, string enddate);

        void UpdateShipPartsOrderChanges(string invoice, string terms, string shipBy, string orderNotes, double? termsDiscount, Guid cardId, bool? isPartialShipment = null);
        void UpdateLineItemLocation(int orderNumber, int lineItemId, int locationId);
        //List<LocationInventory> GetPartLocations(string partNumebr, int lineItemId);
        CartsPartLocationLookup GetPartLocations(string partNumebr, int lineItemId);
        int GetPartStock(string partNumber);

        /*
         * Below function is only for testing purpose
         *
         */
        PartsOrderFulfilmentLineItems ScanTest(string partNumber);
        
    }
}
