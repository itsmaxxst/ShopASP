namespace ShopASP.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Url => "/photos/" + FileName;
    }
}
