using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Models.AppProject.InputModels
{
    public class PatchAppProjectInputModel
    {
        public string Title { get; set; }
        public string ProjectViewId { get; set; }
        public AppProjectType AppProjectType { get; set; }
        public string AppUniqueName { get; set; }
        public int InternalPort { get; set; }
        public RowState State { get; set; }
    }
}
