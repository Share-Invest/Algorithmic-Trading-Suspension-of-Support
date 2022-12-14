using ShareInvest.Interface;

namespace ShareInvest.Catalog.OpenAPI
{
    public struct Order : ISendOrder
    {
        public string AccNo
        {
            get; set;
        }
        public int OrderType
        {
            get; set;
        }
        public string Code
        {
            get; set;
        }
        public int Qty
        {
            get; set;
        }
        public int Price
        {
            get; set;
        }
        public string HogaGb
        {
            get; set;
        }
        public string OrgOrderNo
        {
            get; set;
        }
    }
}