using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BullfrogAPI.Data.Interfaces;
using BullfrogAPI.SharedViewModels.PartsOrderFulfilment;
using System.Data.SqlClient;
using System.Data;
using BullfrogAPI.DataObjects;
using BullfrogAPI.Extensions;
using BullfrogAPI.SharedViewModels.PartsOrderQueue;
using Bullfrog.PrecisionId;
using BullfrogAPI.Enums;
using BullfrogAPI.SharedViewModels.ConcurrencyManagement;
using BullfrogAPI.SharedViewModels.InventoryManagement;

namespace BullfrogAPI.Data.SQLRepositories
{
    public class SqlPartsOrderFulfilmentRepository : IPartsOrderFulfilmentReository
    {
        //public int PickOrder()
        //{
        //    int orderNumber = 0;
        //    bool isBackOrderAllowed = false;
        //    using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
        //    {
        //        using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentPickOrder", con))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            con.Open();
        //            var reader = cmd.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                orderNumber = Convert.ToInt32(reader["OrderNumber"]);
        //                isBackOrderAllowed = (bool)reader["AllowBackOrderPartialShipment"];
        //            }

        //            reader.Close();
        //            con.Close();
        //        }
        //    }

        //    return orderNumber;
        //}

        //public PartsOrderFulfilmentQueue PickOrder()
        //{

        //    PartsOrderFulfilmentQueue order = new PartsOrderFulfilmentQueue();
        //    using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
        //    {
        //        using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentPickOrder", con))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;



        //            con.Open();
        //            var reader = cmd.ExecuteReader();
        //            while (reader.Read())
        //            {
        //               order.OrderNumber = Convert.ToInt32(reader["OrderNumber"]);
        //             order.AllowPartialShipment = (bool)(reader["AllowBackOrderPartialShipment"]);

        //            }

        //            reader.Close();
        //            con.Close();
        //        }
        //    }

        //    return order;
        //}

        public PartsOrderFulfilmentQueue PickOrder(string skipOrders)
        {

            PartsOrderFulfilmentQueue order = new PartsOrderFulfilmentQueue();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentPickOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderNumbers", SqlDbType.VarChar).Value = skipOrders;

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        order.OrderNumber = Convert.ToInt32(reader["OrderNumber"]);
                        order.AllowPartialShipment = (bool)(reader["AllowBackOrderPartialShipment"]);

                    }

                    reader.Close();
                    con.Close();
                }
            }

            return order;
        }
        public bool LockOrder(ConcurrencyManagement param)
        {
            bool success = false;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilment_ConcurrencyManagment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (param.IsLocked || !param.IsLocked)
                    {
                        cmd.Parameters.Add("@islocked", SqlDbType.Bit).Value = param.IsLocked;
                    }

                    if (!string.IsNullOrEmpty(param.EntityId))
                    {
                        cmd.Parameters.Add("@entityId", SqlDbType.VarChar).Value = param.EntityId;
                    }

                    if (!string.IsNullOrEmpty(param.LastLockedBy))
                    {
                        cmd.Parameters.Add("@lastLockedBy", SqlDbType.VarChar).Value = param.LastLockedBy;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    success = true;
                }
            }

            return success;
        }
        public List<PartsOrderFulfilmentQueue> GetPartsOrderFulfilmentQueue(PartsOrderFulfilmentQueue searchParam,
            bool populateOnlyMasterData)
        {
            var masterData = new List<PartsOrderFulfilmentQueue>();
            var childData = new List<PartsOrderFulfilmentLineItems>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentQueue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (searchParam != null && !populateOnlyMasterData)
                    {
                        if (searchParam.OrderNumber > 0)
                        {
                            cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = searchParam.OrderNumber;
                        }

                        if (!string.IsNullOrEmpty(searchParam.AssignedTo))
                        {
                            cmd.Parameters.Add("@assignedTo", SqlDbType.VarChar).Value = searchParam.AssignedTo;
                        }

                        if (searchParam.Id > 0)
                        {
                            cmd.Parameters.Add("@cardId", SqlDbType.Int).Value = searchParam.Id;
                        }

                        cmd.Parameters.Add("@populateOnlyMasterData", SqlDbType.Bit).Value = populateOnlyMasterData;
                    }
                    else
                    {
                        cmd.Parameters.Add("@populateOnlyMasterData", SqlDbType.Bit).Value = populateOnlyMasterData;
                    }

                    con.Open();

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = new PartsOrderFulfilmentQueue
                        {
                            Id = Convert.ToInt32(reader["CartId"]),
                            OrderNumber = Convert.ToInt32(reader["INVOICE"]),
                            InvoiceDate = Convert.ToDateTime(reader["INVCDTE"]),
                            ShipmentMethod = reader["SHIPBY"].ToString(),
                            ShipmentAddress = reader["ShipAddr1"].ToString(),
                            AssignedTo = reader["AssignedTo"].ToString(),
                            DealerName = reader["Dealer"].ToString(),
                            AssignedOn =
                                Convert.ToDateTime(reader["AssignedOn"] == DBNull.Value ? null : reader["AssignedOn"]),
                            CompletionDate =
                                Convert.ToDateTime(reader["CompletionDate"] == DBNull.Value
                                    ? null
                                    : reader["CompletionDate"]),
                            IsLocked = (bool)reader["IsLocked"],
                            AllowPartialShipment = (bool)reader["AllowBackOrderPartialShipment"],
                            InternalNotes = (string)(reader["InternalNotes"] == DBNull.Value ? null : (string)reader["InternalNotes"]),
                            PO = (string)(reader["PONUM"] == DBNull.Value ? null : (string)reader["PONUM"])
                        };

                        masterData.Add(item);
                    }


                    if (!populateOnlyMasterData)
                    {
                        reader.NextResult();

                        while (reader.Read())
                        {
                            var lineItem = new PartsOrderFulfilmentLineItems
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CartId = int.Parse(reader["CartId"].ToString()),
                                LineItemId = int.Parse(reader["LineItemId"].ToString()),
                                PickedQty = int.Parse(reader["PickedQty"].ToString()),
                                LocationId = int.Parse(reader["LocationId"].ToString()),
                                OrderQty = int.Parse(reader["Quantity"].ToString()),
                                BoxNumber = reader["BoxNumber"].ToString(),
                                Location = reader["Location"].ToString(),
                                PartNumber = reader["partno"].ToString(),
                                Description = reader["descript"].ToString(),
                                Measure = reader["fmeasure"].ToString(),
                                Status = reader["Status2"].ToString(),
                                IsPartialShipment = reader["Status"].ToString(),
                                InStockQty = Convert.ToInt32(reader["InStock"]),
                                InLocationStock = Convert.ToInt32(reader["InLocationStock"]),
                                IsFreeShippingExempt = (bool)(reader["FreeShippingExempt"]),
                                Price = Convert.ToDouble(reader["TruePrice"]),
                                DiscountedPrice = Convert.ToDouble(reader["DiscountedPrice"]),
                                HasSupportingDocs = (bool)(reader["HasSupportingDocs"]),
                                LineItemStatus = (string)(reader["LIStatus"] == DBNull.Value ? string.Empty : reader["LIStatus"]),
                                Source = (string)(reader["LiSource"] == DBNull.Value ? string.Empty : reader["LiSource"]),
                                ItemNotes = (string)(reader["ItemNotes"] == DBNull.Value ? string.Empty : reader["ItemNotes"]),
                            };
                            childData.Add(lineItem);
                        }

                        foreach (var partsOrderFulfilmentQueue in masterData)
                        {
                            partsOrderFulfilmentQueue.LineItems =
                                childData.Where(x => x.CartId == partsOrderFulfilmentQueue.Id).ToList();
                        }
                    }

                    reader.Close();
                    con.Close();
                }
            }

            return masterData;
        }

        public ShipPartsOrder GetShipPartsOrder(string orderNumber)
        {
            ShipPartsOrder order = new ShipPartsOrder();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfilmentGetShipper", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = int.Parse(orderNumber);

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        order = new ShipPartsOrder
                        {
                            Invoice = reader["Invoice"].ToString(),
                            DealerId = reader["DealerId"].ToString(),
                            M2MDealerId =
                                (string)(reader["m2mDealerID"] == DBNull.Value ? string.Empty : reader["m2mDealerID"]),
                            OrderStatus = (string)reader["ORDERSTATUS"],
                            IsGuniteJetpakOrder = (bool)reader["IsGuniteJetpakOrder"],
                            Company = (string)reader["Company"],
                            PONumber = (string)(reader["PONUM"] == DBNull.Value ? string.Empty : reader["PONUM"]),
                            ShipBy = (string)(reader["SHIPBY"] == DBNull.Value ? string.Empty : reader["SHIPBY"]),
                            Terms = (string)(reader["TERMS"] == DBNull.Value ? string.Empty : reader["TERMS"]),
                            ChargeForShipping = (bool)(reader["CHARGEFORSHIPPING"] == DBNull.Value
                                ? false
                                : (reader["CHARGEFORSHIPPING"].ToString()
                                    .Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                                    ? true
                                    : false)),
                            PickListPrintDate = (DateTime?)(reader["PickListPrintDate"] == DBNull.Value
                                ? null
                                : reader["PickListPrintDate"]),
                            OrderNotes =
                                (string)(reader["OrderNotes"] == DBNull.Value ? string.Empty : reader["OrderNotes"]),
                            InvoiceDate = (DateTime)reader["INVCDTE"],
                            TermsDiscount = (double)(reader["TERMSDISCOUNT"] == DBNull.Value
                                ? 0.00d
                                : Convert.ToDouble(reader["TERMSDISCOUNT"])),
                            IsTaxable = (bool)reader["Taxable"],
                            CreditCard = (Card)(reader["Ucc"] == DBNull.Value ? null : new Card { Ucc = reader["Ucc"].ToString() }),
                            ShipAddress = new Address
                            {
                                ContactName =
                                    (string)(reader["ShipName"] == DBNull.Value
                                        ? reader["Company"]
                                        : reader["ShipName"]),
                                Addr = (string)(reader["ShipAddr1"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipAddr1"]),
                                City =
                                    (string)(reader["ShipCity"] == DBNull.Value ? string.Empty : reader["ShipCity"]),
                                State = (string)(reader["ShipState"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipState"]),
                                Zip = (string)(reader["ShipZip"] == DBNull.Value ? string.Empty : reader["ShipZip"]),
                                Country = (string)(reader["ShipCountry"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipCountry"]),
                                WorkPhone = (string)(reader["ShipPhone"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipPhone"])
                            },
                            CreditMemosAmount = Convert.ToDouble(reader["CreditMemosAmount"]),
                            InternalNotes = (string)reader["InternalNotes"],
                            Timestamp = (long)reader["Time_stamp"]

                        };
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            order.LineItems.Add(new ShipPartsOrderLineItem
                            {
                                ID = reader["recno"].ToString(),
                                Invoice = reader["Invoice"].ToString(),
                                PartNumber = (string)reader["PartNo"],
                                Description = (string)reader["descript"],
                                Quantity = (int)reader["quantity"],
                                ShippedQuantity = (int)reader["shippedquantity"],
                                AvailQty = (int)reader["AvailQty"],
                                Price = Convert.ToDouble(reader["Price"]),
                                M2mLineNumber = (string)(reader["m2mlineno"] == DBNull.Value ? string.Empty : reader["m2mlineno"]),
                                WebLineNumber = (string)(reader["weblineno"] == DBNull.Value ? string.Empty : reader["weblineno"].ToString()),
                                JobStartDate = (DateTime?)(reader["jostartdate"] == DBNull.Value ? null : reader["jostartdate"]),
                                ProjectedShipDate = (DateTime?)(reader["ProjShipDate"] == DBNull.Value ? null : reader["ProjShipDate"]),
                                Source = (string)reader["source"],
                                IsFreeShippingExempt = (bool)reader["FreeShippingExempt"],
                                JobStatus = (string)reader["jostatus"],
                                HasSupportingDocuments = (bool)reader["HasSupportingDocs"],
                                UnitOfMeasurement = (string)reader["UnitOfMeasurement"]
                            });
                        }
                    }
                }
            }

            if (order.CreditCard != null)
            {
                order.CreditCard = new SqlPaymentRepository().GetCreditCard(order.CreditCard.Ucc);
            }

            return order;

        }

        public string UpdateCartLineItem(PartsOrderFulfilmentLineItems cartLineItem, List<PartsOrderFulfilmentLineItems> lineItemsFromLocations)
        {
            bool success = false;
            string Stock = "";
            string response;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                con.Open();

                if (lineItemsFromLocations != null)
                {

                    if (lineItemsFromLocations.Count > 0)
                    {
                        var qty = 0;
                        foreach (var i in lineItemsFromLocations)
                        {
                            qty = qty + i.PickedQty;
                        }
                        var locations = lineItemsFromLocations.Select(x => x.LocationId).ToList();
                        var strLocations = string.Join(",", locations);

                        var qtys = lineItemsFromLocations.Select(x => x.PickedQty).ToList();
                        var strQtys = string.Join(",", qtys);


                        using (var cmd = new SqlCommand("BullfrogAPI_UpdatePartsOrderFulfilmentCartLineItem", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            if (cartLineItem.CartId > 0 &&
                                cartLineItem.Id > 0 && cartLineItem.LocationId >= 0
                                                    && cartLineItem.PickedQty >= 0 && cartLineItem.LabelQty >= 0
                                                    && !string.IsNullOrEmpty(cartLineItem.PartNumber) &&
                                                    !string.IsNullOrEmpty(cartLineItem.UserName) &&
                                                    !string.IsNullOrEmpty(cartLineItem.IsPartialShipment))
                            {
                                cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = cartLineItem.CartId;
                                cmd.Parameters.Add("@assignedTo", SqlDbType.VarChar).Value = cartLineItem.UserName;
                                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = cartLineItem.Id;
                                cmd.Parameters.Add("@locationId", SqlDbType.Int).Value = cartLineItem.LocationId;
                                cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = cartLineItem.PartNumber;
                                cmd.Parameters.Add("@pickQty", SqlDbType.Int).Value = qty;
                                cmd.Parameters.Add("@oldPickedQty", SqlDbType.Int).Value = cartLineItem.OldPickedQty;
                                cmd.Parameters.Add("@labelQty", SqlDbType.Int).Value = cartLineItem.LabelQty;
                                cmd.Parameters.Add("@status", SqlDbType.VarChar).Value = cartLineItem.IsPartialShipment;
                                cmd.Parameters.Add("@qtyAtLocation", SqlDbType.VarChar).Value = strQtys;
                                cmd.Parameters.Add("@multipleLocations", SqlDbType.VarChar).Value = strLocations;
                                //cmd.Parameters.Add("@updateInventory", SqlDbType.Bit).Value = cartLineItem.InvFromCartUpdate;

                                var reader1 = cmd.ExecuteReader();
                                while (reader1.Read())
                                {
                                    success = (bool)reader1["msg"];
                                    Stock = reader1["Instock"].ToString();
                                }
                                reader1.Close();
                            }
                        }
                    }
                }
                else
                {

                    using (var cmd = new SqlCommand("BullfrogAPI_UpdatePartsOrderFulfilmentCartLineItem", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (cartLineItem.CartId > 0 &&
                            cartLineItem.Id > 0 && cartLineItem.LocationId >= 0
                                                && cartLineItem.PickedQty >= 0 && cartLineItem.LabelQty >= 0
                                                && !string.IsNullOrEmpty(cartLineItem.PartNumber) &&
                                                !string.IsNullOrEmpty(cartLineItem.UserName) &&
                                                !string.IsNullOrEmpty(cartLineItem.IsPartialShipment))
                        {

                            cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = cartLineItem.CartId;
                            cmd.Parameters.Add("@assignedTo", SqlDbType.VarChar).Value = cartLineItem.UserName;
                            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = cartLineItem.Id;
                            cmd.Parameters.Add("@locationId", SqlDbType.Int).Value = cartLineItem.LocationId;
                            cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = cartLineItem.PartNumber;
                            cmd.Parameters.Add("@pickQty", SqlDbType.Int).Value = cartLineItem.PickedQty;
                            cmd.Parameters.Add("@oldPickedQty", SqlDbType.Int).Value = cartLineItem.OldPickedQty;
                            cmd.Parameters.Add("@labelQty", SqlDbType.Int).Value = cartLineItem.LabelQty;
                            cmd.Parameters.Add("@status", SqlDbType.VarChar).Value = cartLineItem.IsPartialShipment;
                            //cmd.Parameters.Add("@updateInventory", SqlDbType.Bit).Value = cartLineItem.InvFromCartUpdate;


                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                success = (bool)reader["msg"];
                                Stock = reader["Instock"].ToString();

                            }
                        }
                    }
                }

                con.Close();
            }

            if (success)
            {
                response = "true," + Stock;
            }
            else
            {
                response = "false," + Stock;
            }

            return response;
        }

        public bool ValidateOrder(int orderId)
        {
            bool success = false;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentOrderProcess", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (orderId > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = orderId;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        success = (bool)reader["AllowBackOrderPartialShipment"];
                    }

                    reader.Close();
                    con.Close();
                }
            }

            return success;
        }


        public MasterQueueViewModel GetPartsOrderFulfilmentMasterQueue(string searchParam, string orderSet)
        {
            //var queue = new List<PartsOrderFulfilmentQueue>();
            var queue = new MasterQueueViewModel();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentMasterQueue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(searchParam))
                    {
                        cmd.Parameters.Add("@searchParam", SqlDbType.VarChar).Value = searchParam;

                    }

                    if (!string.IsNullOrEmpty(orderSet))
                    {
                        cmd.Parameters.Add("@orderSet", SqlDbType.VarChar).Value = orderSet;
                    }

                    con.Open();

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var order = new PartsOrderFulfilmentQueue
                        {
                            Id = Convert.ToInt32(reader["CartId"]),
                            OrderNumber = (int)reader["INVOICE"],
                            IsExpedite = (bool)(reader["Expedite"] == DBNull.Value ? false : reader["Expedite"]),
                            IsPartialShipment = (bool)(reader["PARTIALSHIPMENT"] == DBNull.Value ? false : reader["PARTIALSHIPMENT"]),
                            AllowPartialShipment = (bool)(reader["AllowBackOrderPartialShipment"] == DBNull.Value ? false : reader["AllowBackOrderPartialShipment"]),
                            AssignedTo = reader["AssignedTo"].ToString(),
                            AssignedOn = Convert.ToDateTime(reader["AssignedOn"] == DBNull.Value ? null : reader["AssignedOn"]),
                            CompletionDate = Convert.ToDateTime(reader["CompletionDate"] == DBNull.Value ? null : reader["CompletionDate"]),
                            ShipmentMethod = reader["SHIPBY"].ToString(),
                            DealerName = reader["Company"].ToString(),
                            InvoiceDate = Convert.ToDateTime(reader["INVCDTE"] == DBNull.Value ? null : reader["INVCDTE"]),
                            OrderStatus = reader["OrderStatus"].ToString(),
                            HoldTime = Convert.ToDateTime(reader["HoldTime"] == DBNull.Value ? null : reader["HoldTime"]),
                            IsLocked = (bool)(reader["IsLocked"] == DBNull.Value ? false : reader["IsLocked"]),
                            InternalNotes = reader["InternalNotes"].ToString(),
                            OrderNotes = reader["OrderNotes"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            PO = (string)(reader["PONUM"] == DBNull.Value ? null : (string)reader["PONUM"])

                        };

                        queue.MasterQueueList.Add(order);
                    }

                    reader.NextResult();

                    while (reader.Read())
                    {
                        var order = new PartsOrderFulfilmentQueue
                        {
                            Id = Convert.ToInt32(reader["CartId"]),
                            OrderNumber = (int)reader["INVOICE"],
                            IsExpedite = (bool)(reader["Expedite"] == DBNull.Value ? false : reader["Expedite"]),
                            IsPartialShipment = (bool)(reader["PARTIALSHIPMENT"] == DBNull.Value ? false : reader["PARTIALSHIPMENT"]),
                            AllowPartialShipment = (bool)(reader["AllowBackOrderPartialShipment"] == DBNull.Value ? false : reader["AllowBackOrderPartialShipment"]),
                            AssignedTo = reader["AssignedTo"].ToString(),
                            AssignedOn = Convert.ToDateTime(reader["AssignedOn"] == DBNull.Value ? null : reader["AssignedOn"]),
                            CompletionDate = Convert.ToDateTime(reader["CompletionDate"] == DBNull.Value ? null : reader["CompletionDate"]),
                            ShipmentMethod = reader["SHIPBY"].ToString(),
                            DealerName = reader["Company"].ToString(),
                            InvoiceDate = Convert.ToDateTime(reader["INVCDTE"] == DBNull.Value ? null : reader["INVCDTE"]),
                            OrderStatus = reader["OrderStatus"].ToString(),
                            HoldTime = Convert.ToDateTime(reader["HoldTime"] == DBNull.Value ? null : reader["HoldTime"]),
                            IsLocked = (bool)(reader["IsLocked"] == DBNull.Value ? false : reader["IsLocked"]),
                            InternalNotes = reader["InternalNotes"].ToString(),
                            OrderNotes = reader["OrderNotes"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            PO = (string)(reader["PONUM"] == DBNull.Value ? null : (string)reader["PONUM"])

                        };

                        queue.MasterQueueList.Add(order);
                    }

                    reader.NextResult();


                    while (reader.Read())
                    {
                        queue.UnLockedLineItems = (int)reader["Unlocked"];
                    }

                    reader.NextResult();

                    while (reader.Read())
                    {
                        queue.LocakedLineItems = (int)reader["locked"];
                    }

                    reader.Close();
                    con.Close();
                }

            }

            return queue;
        }

        public List<ManufacturingQueueItem> GetManufacturingQueue()
        {
            List<ManufacturingQueueItem> list = new List<ManufacturingQueueItem>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_GetPartsManufacturingQueue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (dr.Read())
                    {
                        list.Add(new ManufacturingQueueItem
                        {
                            DealerId = dr["DealerId"].ToString(),
                            Company = (string)dr["Company"],
                            OrderDate = (DateTime?)(dr["invcdte"] == DBNull.Value ? null : dr["invcdte"]),
                            OrderNumber = dr["Invoice"].ToString(),
                            PartNumber = (string)dr["partno"],
                            Description = (string)dr["descript"],
                            OrderQuantity = (int)dr["quantity"],
                            ShippedQuantity = (int)dr["ShippedQuantity"],
                            WebLineNo = (int)dr["weblineno"],
                            JobStartDate = (DateTime?)(dr["jostartdate"] == DBNull.Value ? null : dr["jostartdate"]),
                            Source = (string)dr["source"],
                            JobStatus = (string)dr["jostatus"],
                            ID = dr["recno"].ToString()
                        });
                    }

                    dr.Close();
                }
            }

            return list;
        }

        public bool UpdateMasterCartStatus(int orderNumber, bool isPartialShipment)
        {
            bool success = false;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_UpdatePartsOrderFulfilmentCartStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = orderNumber;
                    cmd.Parameters.Add("@isPartialShipment", SqlDbType.Bit).Value = isPartialShipment;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    success = true;
                }
            }

            return success;
        }

        public List<string> GetAllShipmentMethods()
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_GetAllShippingMethods", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<string> shipMethodOptions = new List<string>();

                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        shipMethodOptions.Add(reader["ShipMethodDesc"].ToString());
                    }

                    return shipMethodOptions;
                }
            }
        }

        public List<string> GetPaymentOptions()
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_GetPaymentOptions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<string> paymentOptions = new List<string>();

                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        paymentOptions.Add(reader["PayMethodDesc"].ToString());
                    }

                    return paymentOptions;
                }
            }
        }

        public bool SaveBoxes(int invoice, string id, string boxNumber)
        {
            bool success = false;

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_UpdatePartsOrderFulfilmentCartLineItem_BoxNumber", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    cmd.Parameters.Add("@Id", SqlDbType.VarChar).Value = id;
                    // cmd.Parameters.Add("@partNumber", SqlDbType.NVarChar).Value = itemId;
                    cmd.Parameters.Add("@boxNumber", SqlDbType.VarChar).Value = boxNumber;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    success = true;
                }
            }

            return success;
        }

        public List<PartBarcodeData> GetLineItemBarCodes(string recordNumbers)
        {
            List<PartBarcodeData> barCodes = new List<PartBarcodeData>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfilment_GetPartsOrderLineItemBarcodes",
                    con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@recordNumbers", SqlDbType.VarChar, 4000).Value = recordNumbers;
                    con.Open();

                    var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (dr.Read())
                    {
                        barCodes.Add(new PartBarcodeData
                        {
                            RecordNumber = dr["RecNo"].ToString(),
                            DealerId = dr["DealerId"].ToString(),
                            LabelQty = Convert.ToInt32((dr["LabelQty"])),
                            PartNumber = (string)(dr["PartNumber"] == DBNull.Value ? null : (string)dr["PartNumber"]),
                            Description = (string)(dr["PartDesc"] == DBNull.Value ? null : (string)dr["PartDesc"]),
                            VnbPartNumber =
                                (string)(dr["VnbPartNumber"] == DBNull.Value ? null : (string)dr["VnbPartNumber"]),
                            VnbDescription =
                                (string)(dr["VnbPartDesc"] == DBNull.Value ? null : (string)dr["VnbPartDesc"]),
                            Barcode = (string)(dr["BarCode"] == DBNull.Value ? null : (string)dr["BarCode"]),
                            FoundInLineItems = true,
                            UM = (string)(dr["fmeasure"] == DBNull.Value ? null : (string)dr["fmeasure"])
                        });
                    }

                    dr.Close();
                }
            }

            List<PartBarcodeData> finals = new List<PartBarcodeData>();
            if (barCodes.Count > 0)
            {
                List<string> recNums = recordNumbers.Split(new[] { ',' }).ToList();
                bool isVnb = barCodes.First().DealerId.Equals("5307");

                foreach (string r in recNums)
                {
                    var bCode = barCodes.FirstOrDefault(b => b.RecordNumber.Equals(r));

                    if (bCode == null)
                    {
                        finals.Add(new PartBarcodeData
                        {
                            RecordNumber = r,
                            FoundInLineItems = false,
                        });
                    }
                    else
                    {
                        if (isVnb)
                        {
                            if (!string.IsNullOrEmpty(bCode.Barcode) && !"-".Equals(bCode.Barcode) &&
                                bCode.VnbPartNumber.Length > 5)
                            {
                                bCode.EanBarcode = PrecisionIdAdapter.EAN13(bCode.Barcode);
                            }
                        }

                        finals.Add(bCode);
                    }
                }

            }

            return finals;
        }


        public SubmitPartsOrderValidationResult GetOrderValidation(int invoice)
        {
            var result = new SubmitPartsOrderValidationResult();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_DoPartsOrderValidation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    con.Open();

                    var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (dr.Read())
                    {
                        result.PartsWithMisingVnbDescription.Add(dr.GetString(0));
                    }

                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            result.HasPaidOrVoidedCreditMemos = dr.GetBoolean(0);
                        }
                    }

                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            result.OrderNotFound = false;
                            result.Terms = dr.GetString(0);
                            result.OrderStatus = dr.GetString(1);
                        }
                        else
                        {
                            result.OrderNotFound = true;
                        }
                    }

                    dr.Close();
                }
            }

            return result;
        }

        public PartsOrderSubmit GetFinalizeCartItems(int invoice)
        {

            var cartList = new PartsOrderSubmit();
            var cartLineItem = new List<PartsOrderLineItemSubmit>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfilmentFinalizeCartItems", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var master = new PartsOrderSubmit
                        {

                            Invoice = reader["invoice"].ToString(),
                            DealerId = reader["dealerid"].ToString(),
                            IsPartialShipment = (bool)reader["IsPartialShip"]
                        };

                        cartList = master;
                    }

                    reader.NextResult();

                    while (reader.Read())
                    {
                        var lineItem = new PartsOrderLineItemSubmit
                        {
                            //CartId = int.Parse(reader["CartId"].ToString()),

                            RecordNumber = reader["LineItemId"].ToString(),
                            QuantityToShip = int.Parse(reader["PickedQty"].ToString()),
                            QuantityToBackOrder = int.Parse(reader["backorderQty"].ToString()),
                            //s = reader["Status"].ToString(),
                            DiscountedPrice = double.Parse(reader["price"].ToString())
                        };

                        cartLineItem.Add(lineItem);
                    }
                }

                cartList.LineItems = cartLineItem;

            }

            return cartList;
        }

        public bool Reassign(int invoice, string userName)
        {
            var success = false;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentRemoveFromCart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    if (!string.IsNullOrEmpty(userName))
                    {
                        cmd.Parameters.Add("@assignedTo", SqlDbType.VarChar).Value = userName;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                    success = true;
                    con.Close();
                }
            }

            return success;

        }

        public List<PartsOrderFulfilmentLineItems> GetMasterLineItems(int invoice)
        {
            var lineItemsList = new List<PartsOrderFulfilmentLineItems>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentMasterQueueLineItems", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var lineItems = new PartsOrderFulfilmentLineItems
                        {
                            Id = Convert.ToInt32(reader["CartId"]),
                            LineItemId = Convert.ToInt32(reader["LineItemId"]),
                            PickedQty = Convert.ToInt32(reader["PickedQty"]),
                            BoxNumber = (string)(reader["BoxNumber"] == DBNull.Value ? string.Empty : reader["BoxNumber"]),
                            LabelQty = Convert.ToInt32(reader["LabelQuantity"]),
                            PartNumber = (string)(reader["partno"] == DBNull.Value ? string.Empty : reader["partno"]),
                            OrderQty = Convert.ToInt32(reader["quantity"]),
                            LineItemStatus = (string)(reader["LIStatus"] == DBNull.Value ? string.Empty : reader["LIStatus"]),
                            Price = Convert.ToDouble(reader["price"]),
                            Measure = (string)(reader["fmeasure"] == DBNull.Value ? string.Empty : reader["fmeasure"]),
                            Description = (string)(reader["descript"] == DBNull.Value ? string.Empty : reader["descript"]),
                            Source = (string)(reader["source"] == DBNull.Value ? string.Empty : reader["source"])

                        };

                        lineItemsList.Add(lineItems);
                    }


                    reader.Close();
                    con.Close();

                    return lineItemsList;
                }
            }
        }
        public List<PartsOrderFulfilmentLineItems> GetMasterAllLineItems(int invoice)
        {
            var lineItemsList = new List<PartsOrderFulfilmentLineItems>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentMasterQueueAllLineItems", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var lineItems = new PartsOrderFulfilmentLineItems
                        {
                            CartId = Convert.ToInt32(reader["CartId"]),
                            LineItemId = Convert.ToInt32(reader["LineItemId"]),
                            PickedQty = Convert.ToInt32(reader["PickedQty"]),
                            BoxNumber = (string)(reader["BoxNumber"] == DBNull.Value ? string.Empty : reader["BoxNumber"]),
                            LabelQty = Convert.ToInt32(reader["LabelQuantity"]),
                            PartNumber = (string)(reader["partno"] == DBNull.Value ? string.Empty : reader["partno"]),
                            OrderQty = Convert.ToInt32(reader["quantity"]),
                            LineItemStatus = (string)(reader["LIStatus"] == DBNull.Value ? string.Empty : reader["LIStatus"]),
                            Price = Convert.ToDouble(reader["price"]),
                            Measure = (string)(reader["fmeasure"] == DBNull.Value ? string.Empty : reader["fmeasure"]),
                            Description = (string)(reader["descript"] == DBNull.Value ? string.Empty : reader["descript"]),
                            Source = (string)(reader["source"] == DBNull.Value ? string.Empty : reader["source"])

                        };

                        lineItemsList.Add(lineItems);
                    }


                    reader.Close();
                    con.Close();

                    return lineItemsList;
                }
            }
        }
        public bool IfExists(int invoice)
        {
            bool success = false;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentIsInCart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        success = (bool)reader["IsInCart"];

                    }
                }
            }
            return success;
        }

        public void LockCartOrder(int invoice)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_UpdatePartsOrderFulfilmentLockOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
        }

        public void SaveInternalNotes(int invoice, string internalNotes)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentSaveNotes", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    }
                    if (!string.IsNullOrEmpty(internalNotes))
                    {
                        cmd.Parameters.Add("@internalNotes", SqlDbType.VarChar).Value = internalNotes;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
        }

        public string GetDSR(string dealerId)
        {
            string dsrName = "";

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFullfilmentGetDSR", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!string.IsNullOrEmpty(dealerId))
                    {
                        cmd.Parameters.Add("@dealerid", SqlDbType.VarChar).Value = dealerId;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dsrName = (string)(reader["DSR"] == DBNull.Value ? string.Empty : reader["DSR"]);

                    }

                    reader.Close();
                    con.Close();
                }
            }
            return dsrName;
        }

        public string GetSessionState(int invoice, string currentScreen, string decission)
        {
            string screen = "";

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFullfilmentStateManagment", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = invoice;
                    }
                    if (!string.IsNullOrEmpty(currentScreen))
                    {
                        cmd.Parameters.Add("@currentState", SqlDbType.VarChar).Value = currentScreen;
                    }
                    if (!string.IsNullOrEmpty(decission))
                    {
                        cmd.Parameters.Add("@decission", SqlDbType.VarChar).Value = decission;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        screen = (string)(reader["PreviousState"] == DBNull.Value ? string.Empty : reader["PreviousState"]);

                    }

                    reader.Close();
                    con.Close();
                }
            }
            return screen;
        }
        public void LockOrderFromMasterQueue(string invoices)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_LockOrderFromMasterQueue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(invoices))
                    {
                        cmd.Parameters.Add("@orderNumbers", SqlDbType.VarChar).Value = invoices;
                    }


                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public ShipPartsOrder GetWorldShipInformation(int orderNumber)
        {
            ShipPartsOrder orderInfo = null;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentGetShipper", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (orderNumber > 0)
                    {
                        cmd.Parameters.Add("@orderNumber", SqlDbType.Int).Value = orderNumber;
                    }


                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        orderInfo = new ShipPartsOrder
                        {
                            //ShipAddress = new Address
                            //{
                            //    Addr = reader["ShipAddr1"].ToString(),
                            //    Country = (string)(reader["ShipCountry"] == DBNull.Value ? string.Empty : reader["ShipCountry"])
                            //},
                            ShipAddress = new Address
                            {
                                ContactName =
                                    (string)(reader["ShipName"] == DBNull.Value
                                        ? reader["Company"]
                                        : reader["ShipName"]),
                                Addr = (string)(reader["ShipAddr1"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipAddr1"]),
                                City =
                                    (string)(reader["ShipCity"] == DBNull.Value ? string.Empty : reader["ShipCity"]),
                                State = (string)(reader["ShipState"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipState"]),
                                Zip = (string)(reader["ShipZip"] == DBNull.Value ? string.Empty : reader["ShipZip"]),
                                Country = (string)(reader["ShipCountry"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipCountry"]),
                                WorkPhone = (string)(reader["ShipPhone"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ShipPhone"])
                            },
                            ShipBy = (string)(reader["SHIPBY"] == DBNull.Value ? string.Empty : reader["SHIPBY"]),
                            Terms = (string)(reader["TERMS"] == DBNull.Value ? string.Empty : reader["TERMS"]),
                            Company = (string)(reader["Company"] == DBNull.Value ? string.Empty : reader["Company"]),
                            DealerId = reader["DEALERID"].ToString()
                        };
                    }

                    con.Close();
                }
            }

            return orderInfo;
        }

        /*
         * Below function is only for the scaning purpose
         */
        public PartsOrderFulfilmentLineItems ScanTest(string partNumber)
        {
            var data = new PartsOrderFulfilmentLineItems();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                var query =
                    "SELECT Top 1 partno, descript, price FROM dbo.CLIENT_00000325_Lineitem  WHERE	partno= '" +
                    partNumber + "'";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@partno", SqlDbType.VarChar).Value = partNumber;

                    con.Open();
                    var reader = cmd.ExecuteReader();



                    while (reader.Read())
                    {

                        data.PartNumber = reader["partno"].ToString();
                        data.Description = reader["descript"].ToString();
                        data.Price = Convert.ToDouble(reader["price"]);

                    }

                    con.Close();
                }
            }

            return data;
        }

        public List<PartsOrderFulfilmentQueue> GetPartsOrderFulfilmentMasterQueueByPartNumber(string partNumber)
        {
            var queue = new List<PartsOrderFulfilmentQueue>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentMasterQueue_SearchByPartNumber", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(partNumber))
                    {
                        cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = partNumber;

                    }

                    con.Open();

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var order = new PartsOrderFulfilmentQueue
                        {
                            Id = Convert.ToInt32(reader["CartId"]),
                            OrderNumber = (int)reader["INVOICE"],
                            IsExpedite = (bool)(reader["Expedite"] == DBNull.Value ? false : reader["Expedite"]),
                            IsPartialShipment = (bool)(reader["PARTIALSHIPMENT"] == DBNull.Value ? false : reader["PARTIALSHIPMENT"]),
                            AllowPartialShipment = (bool)(reader["AllowBackOrderPartialShipment"] == DBNull.Value ? false : reader["AllowBackOrderPartialShipment"]),
                            AssignedTo = reader["AssignedTo"].ToString(),
                            AssignedOn = Convert.ToDateTime(reader["AssignedOn"] == DBNull.Value ? null : reader["AssignedOn"]),
                            CompletionDate = Convert.ToDateTime(reader["CompletionDate"] == DBNull.Value ? null : reader["CompletionDate"]),
                            ShipmentMethod = reader["SHIPBY"].ToString(),
                            DealerName = reader["Company"].ToString(),
                            InvoiceDate = Convert.ToDateTime(reader["INVCDTE"] == DBNull.Value ? null : reader["INVCDTE"]),
                            OrderStatus = reader["OrderStatus"].ToString(),
                            HoldTime = Convert.ToDateTime(reader["HoldTime"] == DBNull.Value ? null : reader["HoldTime"]),
                            IsLocked = (bool)(reader["IsLocked"] == DBNull.Value ? false : reader["IsLocked"]),
                            InternalNotes = reader["InternalNotes"].ToString(),
                            OrderNotes = reader["OrderNotes"].ToString(),
                            Terms = reader["Terms"].ToString(),
                            PO = (string)(reader["PONUM"] == DBNull.Value ? null : (string)reader["PONUM"])

                        };

                        queue.Add(order);
                    }

                    reader.Close();
                    con.Close();
                }

            }

            return queue;
        }

        public List<PartsOrderFulfilmentLineItems> GetMasterAllLineItemsByOrders(string invoices)
        {
            var lineItemsList = new List<PartsOrderFulfilmentLineItems>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilmentLineItemsByOrders", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!string.IsNullOrEmpty(invoices))
                    {
                        cmd.Parameters.Add("@orderNumbers", SqlDbType.VarChar).Value = invoices;
                    }

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var lineItems = new PartsOrderFulfilmentLineItems
                        {
                            Id = Convert.ToInt32(reader["invoice"]),
                            CartId = Convert.ToInt32(reader["CartId"]),
                            LineItemId = Convert.ToInt32(reader["LineItemId"]),
                            PickedQty = Convert.ToInt32(reader["PickedQty"]),
                            BoxNumber = (string)(reader["BoxNumber"] == DBNull.Value ? string.Empty : reader["BoxNumber"]),
                            LabelQty = Convert.ToInt32(reader["LabelQuantity"]),
                            PartNumber = (string)(reader["partno"] == DBNull.Value ? string.Empty : reader["partno"]),
                            OrderQty = Convert.ToInt32(reader["quantity"]),
                            LineItemStatus = (string)(reader["LIStatus"] == DBNull.Value ? string.Empty : reader["LIStatus"]),
                            Price = Convert.ToDouble(reader["price"]),
                            Measure = (string)(reader["fmeasure"] == DBNull.Value ? string.Empty : reader["fmeasure"]),
                            Description = (string)(reader["descript"] == DBNull.Value ? string.Empty : reader["descript"]),
                            Source = (string)(reader["source"] == DBNull.Value ? string.Empty : reader["source"])

                        };

                        lineItemsList.Add(lineItems);
                    }


                    reader.Close();
                    con.Close();

                    return lineItemsList;
                }
            }
        }

        public double GetOrderTotalAmount(int invoice)
        {
            double grandTotal = 0d;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilment_GetOrderAmount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    }
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        grandTotal = Convert.ToDouble(reader["TotalAmount"]);

                    }

                    reader.Close();
                    con.Close();
                }
            }

            return grandTotal;
        }
        public int GetLineItemsShipmentInDay()
        {
            int count = 0;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilment_GetTotalLineItemsShipment_OfDay", con))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        count = Convert.ToInt32(reader["TotalShipmentToday"]);

                    }

                    reader.Close();
                    con.Close();
                }
            }
            return count;
        }
        public ShippingList GetLatestShippingList(string invoice)
        {
            ShippingList shipper = null;
            List<PartsOrderFulfilmentLineItems> list = new List<PartsOrderFulfilmentLineItems>();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_GetLatestPartsOrderShipperInformation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@webOrderNumber", SqlDbType.VarChar, 4000).Value = int.Parse(invoice);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (dr.Read())
                    {
                        shipper = new ShippingList
                        {
                            OrderNumber = dr["Invoice"].ToString(),
                            PoNumber = (string)(dr["PONUM"] == DBNull.Value ? string.Empty : dr["PONUM"]),
                            TrackingNumber = (string)(dr["TrackingNum"] == DBNull.Value ? string.Empty : dr["TrackingNum"]),
                            ShipTo = new Address
                            {
                                ContactName = (string)(dr["Name"] == DBNull.Value ? string.Empty : dr["Name"]),
                                Addr = (string)(dr["Addr1"] == DBNull.Value ? string.Empty : dr["Addr1"]),
                                City = (string)(dr["City"] == DBNull.Value ? string.Empty : dr["City"]),
                                State = (string)(dr["State"] == DBNull.Value ? string.Empty : dr["State"]),
                                Zip = (string)(dr["Zip"] == DBNull.Value ? string.Empty : dr["Zip"]),
                                Country = (string)(dr["Country"] == DBNull.Value ? string.Empty : dr["Country"])
                            }
                        };


                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                shipper.ShipperNumber = (string)dr["ShipperNo"];
                                shipper.InvoiceNumber = (string)dr["fcinvoice"];
                                shipper.ShipDate = (DateTime)(dr["finvdate"]);
                                shipper.ShipVia = (string)dr["fshipvia"];

                                shipper.SoldTo = new Address
                                {
                                    ContactName = (string)dr["fbcompany"],
                                    Addr = (string)dr["fmbstreet"],
                                    City = (string)dr["fbcity"],
                                    State = (string)dr["fbstate"],
                                    Zip = (string)dr["fbzip"],
                                    Country = (string)dr["fbcountry"]
                                };
                            }

                            if (dr.NextResult())
                            {
                                while (dr.Read())
                                {
                                    shipper.Items.Add(new ShippingListItem
                                    {
                                        LineNumber = (string)dr["fitem"],
                                        PartNumber = (string)dr["fpartno"],
                                        Description = (string)dr["fmdescript"],
                                        UnitOfMeasure = (string)dr["fmeasure"],
                                        BackOrderQuantity = Convert.ToInt32(dr["fbkordqty"]),
                                        ShippedQuantity = Convert.ToInt32(dr["fshipqty"]),
                                        BoxNumber = (string)dr["BoxNumber"]
                                    });
                                }
                            }

                        }
                    }
                }
            }

            return shipper;
        }

        public void AddTotalNumberOfBoxes(int totalBoxes, int invoice)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_AddTotalBoxes", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (totalBoxes > 0)
                    {
                        cmd.Parameters.Add("@totalBoxes", SqlDbType.Int).Value = totalBoxes;
                    }
                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();

                }
            }
        }

        public int GetTotalNoOfBoxes(int invoice, string lineItems)
        {

            int totalBoxes = 0;
            List<PartsOrderFulfilmentLineItems> list = new List<PartsOrderFulfilmentLineItems>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_GetTotalBoxes", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    }
                    if (!string.IsNullOrEmpty(lineItems))
                    {
                        cmd.Parameters.Add("@lineItems", SqlDbType.VarChar).Value = lineItems;
                    }
                    con.Open();
                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {

                        totalBoxes = (int)(dr["BoxNumber"]);

                    }
                    con.Close();

                }
            }
            return totalBoxes;
        }

        public int GetBoxCountForLineItems(int invoice, string lineItems)
        {

            //int totalBoxes = 0; MNM: not used
            List<PartsOrderFulfilmentLineItems> list = new List<PartsOrderFulfilmentLineItems>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_GetBoxCountPerLineItems", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    if (invoice > 0)
                    {
                        cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    }
                    if (!string.IsNullOrEmpty(lineItems))
                    {
                        cmd.Parameters.Add("@lineItems", SqlDbType.VarChar).Value = lineItems;
                    }
                    con.Open();
                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var box = new PartsOrderFulfilmentLineItems
                        {
                            BoxNumber = (string)(dr["BoxNumber"])
                        };


                        list.Add(box);
                    }
                    con.Close();

                }
            }
            return list.Count;
        }
        public List<PartsSupportingDocuments> GetPartsSupportingDocuments()
        {
            List<PartsSupportingDocuments> list = new List<PartsSupportingDocuments>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_PartsSupportingDocuments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var box = new PartsSupportingDocuments
                        {
                            Id = (int)dr["ID"],
                            PartNumber = (string)dr["PartNumber"],
                            DocumentTitle = (string)dr["Title"],
                            IsUsed = (bool)(dr["ShipWithPartsOrder"] == DBNull.Value ? false : dr["ShipWithPartsOrder"])

                        };


                        list.Add(box);
                    }
                    con.Close();

                }
            }

            return list;
        }

        public void UpdatePartsDocumentStatus(PartsSupportingDocuments p)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_UpdatePartsSupportingDocuments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.Id;
                    cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = p.PartNumber;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = p.DocumentTitle;
                    cmd.Parameters.Add("@isActiv", SqlDbType.VarChar).Value = p.IsUsed;

                    con.Open();

                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }

        }

        public void AddPartsDocument(PartsSupportingDocuments p)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfillment_AddPartsSupportingDocuments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = p.PartNumber;
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = p.DocumentTitle;
                    cmd.Parameters.Add("@isActiv", SqlDbType.VarChar).Value = p.IsUsed;

                    con.Open();

                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
        }

        public List<PartsOrderFulfilmentLineItems> GetBoxList(int invoice, string lineItems)
        {
            var list = new List<PartsOrderFulfilmentLineItems>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_GetPartsOrderFulfilmentCartLineItem_BoxNumber", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = invoice;
                    cmd.Parameters.Add("@lineItems", SqlDbType.VarChar).Value = lineItems;

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var box = new PartsOrderFulfilmentLineItems
                        {
                            BoxNumber = (string)dr["BoxNumber"]
                        };


                        list.Add(box);
                    }
                    con.Close();

                }
            }
            return list;
        }
        public List<PartsOrderFulfillmentReporting> GetWeeklyReport(string startDate, string endDate)
        {
            var list = new List<PartsOrderFulfillmentReporting>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_LineItemsShippedbyWeek", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    cmd.Parameters.Add("@startDate", SqlDbType.VarChar).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.VarChar).Value = endDate;

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new PartsOrderFulfillmentReporting
                        {
                            ShipDate = (DateTime)dr["WeekStart"],
                            LineItemCount = (int)dr["TotalLineItemsShipped"]
                        };

                        list.Add(obj);
                    }

                    con.Close();
                }
            }

            return list;
        }

        public List<PartsOrderFulfillmentReporting> GetDailyReport(string startDate, string endDate)
        {
            var list = new List<PartsOrderFulfillmentReporting>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_LineItemsShippedPerDay", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@startDate", SqlDbType.VarChar).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.VarChar).Value = endDate;

                    con.Open();

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new PartsOrderFulfillmentReporting
                        {
                            ShipDate = (DateTime)dr["Date"],
                            LineItemCount = (int)dr["TotalLineItems"]
                        };

                        list.Add(obj);
                    }

                    con.Close();
                }
            }

            return list;
        }

        public List<PartsOrderFulfillmentReporting> GetReportByUser(string startDate, string endDate)
        {
            var list = new List<PartsOrderFulfillmentReporting>();
            var dateList = new List<PartsOrderFulfillmentReportingXDate>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_LineItemsShippedByUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@StartDate", SqlDbType.VarChar).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.VarChar).Value = endDate;

                    con.Open();

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new PartsOrderFulfillmentReporting
                        {
                            AssignedTo = (string)dr["AssignedTo"],
                            ShipDate = (DateTime)dr["ActualShipDate"],
                            LineItemCount = (int)dr["TotalLineItems"]
                        };

                        list.Add(obj);
                    }
                    con.Close();
                }
            }
            return list;
        }
        public List<PartsOrderFulfillmentReportingXDate> GetDatesXes(string startDate, string endDate)
        {
            var dateList = new List<PartsOrderFulfillmentReportingXDate>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_LineItemsShippedByUserDates", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@StartDate", SqlDbType.VarChar).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.VarChar).Value = endDate;

                    con.Open();

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new PartsOrderFulfillmentReportingXDate
                        {
                            ShipingDate = (DateTime)dr["ActualShipDate"]
                        };
                        dateList.Add(obj);
                    }

                    con.Close();
                }
            }
            return dateList;
        }

        public List<PartsOrderFulfillmentReporting> GetReportByUserTotal(string startDate, string endDate)
        {
            var list = new List<PartsOrderFulfillmentReporting>();

            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_LineItemsShippedByUserTotal", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@StartDate", SqlDbType.VarChar).Value = startDate;
                    cmd.Parameters.Add("@endDate", SqlDbType.VarChar).Value = endDate;

                    con.Open();

                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var obj = new PartsOrderFulfillmentReporting
                        {
                            ShipDate = (DateTime)dr["ActualShipDate"],
                            LineItemCount = (int)dr["TotalLineItems"]
                        };

                        list.Add(obj);
                    }
                    con.Close();
                }
            }
            return list;
        }

        public void UpdateShipPartsOrderChanges(string invoice, string terms, string shipBy, string orderNotes, double? termsDiscount, Guid cardId, bool? isPartialShipment)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PArtsOrderFulfillment_UpdateShipPartsOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = int.Parse(invoice);
                    cmd.Parameters.Add("@terms", SqlDbType.VarChar, 30).Value = terms;
                    cmd.Parameters.Add("@shipBy", SqlDbType.VarChar, 50).Value = shipBy;
                    cmd.Parameters.Add("@orderNotes", SqlDbType.VarChar, 400).Value = orderNotes;


                    if (termsDiscount.HasValue)
                    {
                        cmd.Parameters.Add("@termsDiscount", SqlDbType.Int).Value = termsDiscount;
                    }

                    if (!Guid.Empty.Equals(cardId))
                    {
                        cmd.Parameters.Add("@cardNumber", SqlDbType.UniqueIdentifier).Value = cardId;
                    }

                    if (isPartialShipment.HasValue)
                    {
                        cmd.Parameters.Add("@isPartialShipment", SqlDbType.Bit).Value = isPartialShipment;
                    }


                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public void UpdateLineItemLocation(int orderNumber, int lineItemId, int locationId)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PofUpdateLineItemLocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@invoice", SqlDbType.Int).Value = orderNumber;
                    cmd.Parameters.Add("@locationId", SqlDbType.Int).Value = locationId;
                    cmd.Parameters.Add("@recoNo", SqlDbType.Int).Value = lineItemId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public CartsPartLocationLookup GetPartLocations(string partNumebr, int lineItemId)
        {
            //var partLocations = new List<LocationInventory>();
            var cartItem = new CartsPartLocationLookup();
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("BullfrogAPI_PartsOrderFulfilment_GetPartLocation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(partNumebr))
                    {
                        cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = partNumebr;
                    }
                    if(lineItemId > 0)
                    {
                        cmd.Parameters.Add("@lineItemId", SqlDbType.Int).Value = lineItemId;
                    }

                    con.Open();

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var locationInventory = new LocationInventory
                        {
                            Location = (string)(reader["Location"] == DBNull.Value ? string.Empty : reader["Location"]),
                            Quantity = Convert.ToInt32(reader["Qty"]),
                            Date = (DateTime)reader["Date"],
                            AreaName = (string)(reader["AreaName"] == DBNull.Value ? string.Empty : reader["AreaName"]),
                            Aisle = (string)(reader["Aisle"] == DBNull.Value ? string.Empty : reader["Aisle"]),
                            StackPostion = (string)(reader["StackPosition"] == DBNull.Value ? string.Empty : reader["StackPosition"]),
                            Stack = Convert.ToInt32(reader["Stack"]),
                            AreaID = Convert.ToInt32(reader["AreaID"]),
                            InventoryAreaId = Convert.ToInt32(reader["InventoryAreaId"]),
                            warehouseInventoryLocationId = Convert.ToInt32(reader["Id"]),
                            LocationId = Convert.ToInt32(reader["LocationId"])
                        };

                        cartItem.Li.Add(locationInventory);
                    }

                    reader.NextResult();

                    
                    while (reader.Read())
                    {
                        cartItem.Pofli.Id = Convert.ToInt32(reader["Id"]);
                        cartItem.Pofli.MultipleLocations = (string)(reader["MultipleLocations"] == DBNull.Value ? string.Empty : reader["MultipleLocations"]);
                        cartItem.Pofli.MLQtys = (string)(reader["PickedQtyPerLocation"] == DBNull.Value ? string.Empty : reader["PickedQtyPerLocation"]);
                    }

                    
                }
            }
            return cartItem;
        }
        public void StorePartsLocations(int cartId, int locationId, int lineItemId, int pickedQty)
        {
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfilment_StorePartLocations", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (cartId > 0)
                    {
                        cmd.Parameters.Add("@cartId", SqlDbType.Int).Value = cartId;
                    }
                    if (locationId > 0)
                    {
                        cmd.Parameters.Add("@locationId", SqlDbType.Int).Value = locationId;
                    }
                    if (locationId > 0)
                    {
                        cmd.Parameters.Add("@lineItemId", SqlDbType.Int).Value = lineItemId;
                    }
                    if (pickedQty > 0)
                    {
                        cmd.Parameters.Add("@pickedQty", SqlDbType.Int).Value = pickedQty;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();

                }
            }
        }

        public int GetPartStock(string partNumber)
        {
            var stock = 0;
            using (var con = new SqlConnection(ConnectionString.GetWebTablesConnectionString()))
            {
                using (var cmd = new SqlCommand("dbo.BullfrogAPI_PartsOrderFulfilmentGetStockbyPart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(partNumber))
                    {
                        cmd.Parameters.Add("@partNumber", SqlDbType.VarChar).Value = partNumber;
                    }
                   

                    con.Open();
                   var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        stock = Convert.ToInt32(reader["InStock"]);
                    }

                    reader.Close();
                    con.Close();

                }
            }
            return stock;
        }

    }
}
