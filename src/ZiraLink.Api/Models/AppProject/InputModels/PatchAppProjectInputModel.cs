﻿using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Models.AppProject.InputModels
{
    public class PatchAppProjectInputModel
    {
        public string Title { get; set; }
        public string AppProjectViewId { get; set; }
        public AppProjectType AppProjectType { get; set; }
        //public string AppUniqueName { get; set; }
        public int InternalPort { get; set; }
        public RowState State { get; set; }
    }
}