namespace Aml.AdminPanel.Services
{
    public class AlertsFilterState
    {
        public HashSet<string> SelectedStatuses { get; } = new();
        public int CurrentPage { get; set; } = 1;
    }
}
