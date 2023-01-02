﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HalApplicationBuilder.AspNetMvc {
    public class MvcModels {
        internal Core.ApplicationSchema Schema { get; init; }

        internal string TransformText() {
            var rootAggregates = Schema.RootAggregates();
            var allAggregates = Schema.AllAggregates();
            var template = new MvcModelsTemplate {
                Namespace = Schema.Config.MvcModelNamespace,
                SearchConditionClasses = allAggregates.Select(a => a.SearchConditionModel),
                SearchResultClasses = rootAggregates.Select(a => a.SearchResultModel),
                InstanceClasses = allAggregates.Select(a => a.InstanceModel),
            };
            return template.TransformText();
        }
    }

    partial class MvcModelsTemplate {
        internal string Namespace { get; set; }
        internal IEnumerable<Core.UIClass> SearchConditionClasses { get; set; }
        internal IEnumerable<Core.UIClass> SearchResultClasses { get; set; }
        internal IEnumerable<Core.UIClass> InstanceClasses { get; set; }
    }
}
