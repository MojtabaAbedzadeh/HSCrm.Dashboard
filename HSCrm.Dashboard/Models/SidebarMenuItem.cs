namespace HSCrm.Dashboard.Models
{
    public class SidebarMenuItem
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string? PermissionKey { get; set; }
        public List<SidebarMenuItem> Children { get; set; } = new();
    }
}