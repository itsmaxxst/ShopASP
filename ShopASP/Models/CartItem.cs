namespace ShopASP.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string ProductTitle { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public Photo? ProductPhoto { get; set; }
        public Product Product { get; set; }
    }
}
