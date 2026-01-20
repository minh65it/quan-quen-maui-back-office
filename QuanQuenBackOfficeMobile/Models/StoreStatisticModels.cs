using System;
using System.Collections.Generic;

namespace QuanQuenBackOfficeMobile.Models;

public class InvoiceItemModel
{
    public string id { get; set; }
    public string storeId { get; set; }
    public string storeName { get; set; }
    public DateTime period { get; set; }
    public decimal revenueAmount { get; set; }
    public decimal discountRate { get; set; }
    public decimal currentAmount { get; set; }
    public decimal carryOverAmount { get; set; }
    public decimal totalAmount { get; set; }
    public decimal paidAmount { get; set; }
    public int status { get; set; }
}

public class PaginationModel
{
    public int totalCount { get; set; }
    public int totalPage { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int skip { get; set; }
}

public class InvoiceListDataModel
{
    public PaginationModel pagination { get; set; }
    public List<InvoiceItemModel> result { get; set; }
}

public class ApiResponseInvoiceListModel
{
    public string message { get; set; }
    public bool success { get; set; }
    public InvoiceListDataModel data { get; set; }
}

public class OrderItemModel
{
    public string id { get; set; }
    public string storeId { get; set; }
    public int totalQuantity { get; set; }
    public decimal totalPrice { get; set; }
    public int status { get; set; }
    public string phoneNumber { get; set; }
    public string email { get; set; }
    public string orderDate { get; set; }
    public string orderTime { get; set; }
    public string createdBy { get; set; }
    public DateTime createdDate { get; set; }
    public string storeType { get; set; }
}

public class OrderListDataModel
{
    public PaginationModel pagination { get; set; }
    public List<OrderItemModel> result { get; set; }
}

public class MerchantOrderSuccessModel
{
    public int numberOfOrderCreateSuccess { get; set; }
    public OrderListDataModel listOrder { get; set; }
}

public class ApiResponseMerchantOrderSuccess
{
    public string message { get; set; }
    public bool success { get; set; }
    public MerchantOrderSuccessModel data { get; set; }
}

public class CityItemModel
{
    public string id { get; set; }
    public string name { get; set; }
}

public class StoreTypeItemModel
{
    public string id { get; set; }
    public string name { get; set; }
}

public class CategoryMasterItemModel
{
    public string id { get; set; }
    public string name { get; set; }
}

// Generic order list response used by NumberOfUserInOrder
public class ApiResponseOrderList
{
    public string message { get; set; }
    public bool success { get; set; }
    public OrderListDataModel data { get; set; }
}

public class LoyalCustomerItemModel
{
    public string userId { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
}

public class LoyalCustomerDataModel
{
    public int numberOfUser { get; set; }
    public List<LoyalCustomerItemModel> listCustomerComback { get; set; }
}

public class ApiResponseLoyalCustomer
{
    public string message { get; set; }
    public bool success { get; set; }
    public LoyalCustomerDataModel data { get; set; }
}

public class AverageRevenueDataModel
{
    public int totalOrder { get; set; }
    public decimal totalPriceAllOrder { get; set; }
    public decimal averageRevenue { get; set; }
}

public class ApiResponseAverageRevenue
{
    public string message { get; set; }
    public bool success { get; set; }
    public AverageRevenueDataModel data { get; set; }
}


