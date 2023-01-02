﻿using System;
using System.IO;

namespace HalApplicationBuilder.AspNetMvc {

    internal class AggregatePartialView {
        internal Core.Aggregate Aggregate { get; init; }

        internal string FileName => $"_{Aggregate.Name}__Partial.cshtml";
        internal string AspViewPath => Path.Combine("~", Aggregate.Schema.Config.MvcViewDirectoryRelativePath, FileName);

        internal string TransformText() {
            var template = new AggregatePartialViewTemplate {
                ModelTypeFullname = Aggregate.ToInstanceModel().RuntimeFullName,
                View = Aggregate.RenderInstanceView(new Core.ViewRenderingContext("Model")),
            };
            return template.TransformText();
        }
    }

    partial class AggregatePartialViewTemplate {
        internal string ModelTypeFullname { get; set; }
        internal string View { get; set; }
    }
}
