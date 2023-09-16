using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Models.Project.InputModels
{
    public class CreateProjectInputModel
    {
        public string Title { get; set; }
        public DomainType DomainType { get; set; }
        public string Domain { get; set; }
        public string InternalUrl { get; set; }
        public ProjectState State { get; set; }
    }
}
