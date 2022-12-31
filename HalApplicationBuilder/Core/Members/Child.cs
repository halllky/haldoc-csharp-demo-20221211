﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using HalApplicationBuilder.Runtime;

namespace HalApplicationBuilder.Core.Members {
    internal class Child : AggregateMemberBase {
        public override bool IsCollection => false;

        private Dictionary<int, Aggregate> _children;
        private IReadOnlyDictionary<int, Aggregate> GetChildren() {
            if (_children == null) {
                var childType = UnderlyingPropertyInfo.PropertyType.GetGenericArguments()[0];
                var variations = UnderlyingPropertyInfo.GetCustomAttributes<VariationAttribute>();

                if (!childType.IsAbstract && !variations.Any()) {
                    // complex object
                    _children = new Dictionary<int, Aggregate> {
                        { -1, new Aggregate { Config = Config, Parent = this, UnderlyingType = childType }},
                    };

                } else if (childType.IsAbstract && variations.Any()) {
                    // バリエーション型
                    var cannotAssignable = variations.Where(x => !childType.IsAssignableFrom(x.Type)).ToArray();
                    if (cannotAssignable.Any()) {
                        var typeNames = string.Join(", ", cannotAssignable.Select(x => x.Type.Name));
                        throw new InvalidOperationException($"{childType.Name} の派生型でない: {typeNames}");
                    }
                    _children = variations.ToDictionary(v => v.Key, v => new Aggregate {
                        Config = Config,
                        UnderlyingType = v.Type,
                        Parent = this,
                    });

                } else {
                    throw new InvalidOperationException($": 抽象型ならバリエーション必須、抽象型でないならバリエーション指定不可");
                }
            }
            return _children;
        }

        /// <summary>ComplexTypeかバリエーション型か</summary>
        private bool IsComplexType() {
            var children = GetChildren();
            return children.Count == 1 && children.Single().Key == -1;
        }
        /// <summary>バリエーション型なら例外</summary>
        private Aggregate ChildAggregate => IsComplexType()
            ? GetChildren().Single().Value
            : throw new InvalidOperationException($"バリエーション型なので{nameof(ChildAggregate)}プロパティ使用不可。{nameof(IsComplexType)}メソッドを使って分岐をかけてください。");

        public override IEnumerable<Aggregate> GetChildAggregates() {
            return GetChildren().Values;
        }

        internal override IEnumerable<AutoGenerateDbEntityProperty> ToDbColumnModel() {
            if (IsComplexType()) {
                yield break;
            } else {
                yield return new AutoGenerateDbEntityProperty {
                    CSharpTypeName = "int?",
                    PropertyName = Name,
                };
            }
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchConditionModel(ViewRenderingContext context) {
            if (IsComplexType()) {
                var propname = Name;
                var nested = context.Nest(propname);
                var child = ChildAggregate.ToSearchConditionModel(nested);
                yield return new AutoGenerateMvcModelProperty {
                    CSharpTypeName = child.RuntimeFullName,
                    PropertyName = propname,
                    Initializer = "new()",
                    View = child.View,
                };
            } else {
                foreach (var variation in GetChildren()) {
                    var propname = $"{Name}_{variation.Value.Name}";
                    var nested = context.Nest(propname);
                    var template = new ChildVariationSearchConditionTemplate {
                        PropertyName = propname,
                        AspFor = nested.AspForPath,
                    };
                    yield return new AutoGenerateMvcModelProperty {
                        PropertyName = propname,
                        CSharpTypeName = "bool",
                        Initializer = "true",
                        View = template.TransformText(),
                    };
                }
            }
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToSearchResultModel(ViewRenderingContext context) {
            if (IsComplexType()) {
                foreach (var childProp in ChildAggregate.ToSearchResultModel(context).Properties) {
                    yield return childProp;
                }
            } else {
                var propname = Name;
                var nested = context.Nest(propname);
                yield return new AutoGenerateMvcModelProperty {
                    CSharpTypeName = "string",
                    PropertyName = propname,
                    View = $"<span>@{nested.Path}</span>",
                };
            }
        }

        internal override IEnumerable<AutoGenerateMvcModelProperty> ToInstanceModel(ViewRenderingContext context) {
            if (IsComplexType()) {
                var propName = Name;
                var nestedContext = context.Nest(propName);
                var childModel = ChildAggregate.ToInstanceModel(nestedContext);
                yield return new AutoGenerateMvcModelProperty {
                    CSharpTypeName = childModel.RuntimeFullName,
                    PropertyName = propName,
                    View = childModel.View,
                    Initializer = "new()",
                };
            } else {
                // 区分値
                var typePropertyName = Name;
                var nested1 = context.Nest(typePropertyName);
                yield return new AutoGenerateMvcModelProperty {
                    CSharpTypeName = "int?",
                    PropertyName = typePropertyName,
                    View = "",
                };
                // 各区分の詳細値
                foreach (var child in GetChildren()) {
                    var detailPropertyName = $"{Name}__{child.Value.Name}";
                    var nested2 = context.Nest(detailPropertyName);
                    var childModel = child.Value.ToInstanceModel(nested2);
                    var template = new ChildVariationInstanceTemplate {
                        Key = child.Key,
                        Name = child.Value.Name,
                        RadioButtonAspFor = nested1.AspForPath,
                        ChildAggregateView = childModel.View,
                    };
                    yield return new AutoGenerateMvcModelProperty {
                        CSharpTypeName = childModel.RuntimeFullName,
                        PropertyName = detailPropertyName,
                        Initializer = "new()",
                        View = template.TransformText(),
                    };
                }
            }
        }
    }

    partial class ChildVariationSearchConditionTemplate {
        public string PropertyName { get; set; }
        public string AspFor { get; set; }
    }

    partial class ChildVariationInstanceTemplate {
        public string RadioButtonAspFor { get; set; }
        public int Key { get; set; }
        public string Name { get; set; }
        public string ChildAggregateView { get; set; }
    }
}
