using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Models.AppProject.InputModels
{
    public class PatchAppProjectInputModel
    {
        public string Title { get; set; }
        public Guid? AppProjectViewId { get; set; }
        public AppProjectType AppProjectType { get; set; }
        public PortType PortType { get; set; }
        public int InternalPort { get; set; }
        public ProjectState State { get; set; }
    }
}
