﻿using System;
using System.IO;
using HalApplicationBuilder.Runtime;

namespace HalApplicationBuilder.AspNetMvc {

    internal class InstancePartialView {
        internal InstancePartialView(Core.Aggregate aggregate, Core.Config config) {
            Aggregate = aggregate;
            _config = config;
        }

        internal Core.Aggregate Aggregate { get; }
        private readonly Core.Config _config;

        internal string FileName => $"_{Aggregate.Name}__Partial.cshtml";
        internal string AspViewPath => Path.Combine("~", _config.MvcViewDirectoryRelativePath, FileName);

        internal string TransformText(IViewModelProvider viewModelProvider) {
            var model = viewModelProvider.GetInstanceModel(Aggregate);
            var template = new InstancePartialViewTemplate {
                ModelTypeFullname = $"{typeof(Instance<>).Namespace}.{nameof(Instance<object>)}<{model.RuntimeFullName}>",
                View = model.Render(new ViewRenderingContext("Model", nameof(Instance<object>.Item))),
            };
            return template.TransformText();
        }
    }

    partial class InstancePartialViewTemplate {
        internal string ModelTypeFullname { get; set; }
        internal string View { get; set; }
    }
}