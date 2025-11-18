namespace Invoice_Manager.Models.ViewModels
{
    public class DashboardStatsViewModel
    {
        public decimal AmountToCollect { get; set; }
        public decimal AmountCollectedThisMonth { get; set; }
        public int OverdueCount { get; set; }
    }
}
