namespace BookStore.Models
{
    public class DailyStatisticsViewModel
    {
        public DateTime OrderDate { get; set; }
        public int OrderCount { get; set; }
        public double Revenue { get; set; }
        public string FormattedOrderDate => OrderDate.ToString("dd/MM/yy");
    }
}
