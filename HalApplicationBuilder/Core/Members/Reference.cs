﻿using System;
using System.Collections.Generic;

namespace HalApplicationBuilder.Core.Members {
    internal class Reference : AggregateMemberBase {
        public override bool IsCollection => false;

        private Aggregate _refTarget;
        private Aggregate RefTarget {
            get {
                if (_refTarget == null) {
                    var type = UnderlyingPropertyInfo.PropertyType.GetGenericArguments()[0];
                    _refTarget = Schema.FindByType(type);
                    if (_refTarget == null) throw new InvalidOperationException($"{UnderlyingPropertyInfo.Name} の型 {type.FullName} の集約が定義されていません。");
                }
                return _refTarget;
            }
        }

        public override IEnumerable<Aggregate> GetChildAggregates() {
            yield break;
        }

        internal override IEnumerable<AutoGenerateDbEntityProperty> ToDbColumnModel() {
            foreach (var foreignKey in RefTarget.ToDbTableModel().PKColumns) {
                yield return new AutoGenerateDbEntityProperty {
                    CSharpTypeName = foreignKey.CSharpTypeName,
                    PropertyName = $"{Name}_{foreignKey.PropertyName}",
                };
            }
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToInstanceModel(ViewRenderingContext context) {
            var propName = Name;
            var nestedKey = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceKey));
            var nestedText = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceName));
            var template = new ReferenceInstanceTemplate {
                AspForKey = nestedKey.AspForPath,
                AspForText = nestedText.AspForPath,
            };
            yield return new AutoGenerateMvcModelProperty {
                CSharpTypeName = typeof(Runtime.ReferenceDTO).FullName,
                PropertyName = propName,
                Initializer = "new()",
                View = template.TransformText(),
            };
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchConditionModel(ViewRenderingContext context) {
            var propName = Name;
            var nestedKey = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceKey));
            var nestedText = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceName));
            var template = new ReferenceInstanceTemplate {
                AspForKey = nestedKey.AspForPath,
                AspForText = nestedText.AspForPath,
            };
            yield return new AutoGenerateMvcModelProperty {
                CSharpTypeName = typeof(Runtime.ReferenceDTO).FullName,
                PropertyName = Name,
                Initializer = "new()",
                View = template.TransformText(),
            };
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchResultModel(ViewRenderingContext context) {
            var propName = UnderlyingPropertyInfo.Name;
            var nestedKey = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceKey));
            var nestedText = context.Nest(propName).Nest(nameof(Runtime.ReferenceDTO.InstanceName));
            yield return new AutoGenerateMvcModelProperty {
                CSharpTypeName = "string",
                PropertyName = propName,
                View = $"<span>@{nestedText.Path}<input type=\"hidden\" asp-for=\"{nestedKey.AspForPath}\"></span>",
            };
        }
    }

    partial class ReferenceInstanceTemplate {
        internal string AspForKey { get; set; }
        internal string AspForText { get; set; }
    }
}
