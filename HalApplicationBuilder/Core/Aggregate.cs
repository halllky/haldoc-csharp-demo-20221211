﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace HalApplicationBuilder.Core {
    public class Aggregate {

        public Config Config { get; init; }
        internal Type UnderlyingType { get; init; }

        public string Name => UnderlyingType.Name;


        #region リレーション
        public AggregateMemberBase Parent { get; init; }

        private List<AggregateMemberBase> _members;
        public IReadOnlyList<AggregateMemberBase> Members {
            get {
                if (_members == null) {
                    _members = new List<AggregateMemberBase>();
                    foreach (var prop in UnderlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                        if (prop.GetCustomAttribute<NotMappedAttribute>() != null) continue;

                        if (Core.Members.SchalarValue.IsPrimitive(prop.PropertyType)) {
                            _members.Add(new Core.Members.SchalarValue { Config = Config, Owner = this, UnderlyingPropertyInfo = prop });
                        } else if (prop.PropertyType.IsGenericType
                            && prop.PropertyType.GetGenericTypeDefinition() == typeof(Child<>)) {
                            _members.Add(new Core.Members.Child { Config = Config, Owner = this, UnderlyingPropertyInfo = prop });
                        }
                    }
                }
                return _members;
            }
        }

        public IEnumerable<Aggregate> GetDescendants() {
            var children = Members.SelectMany(member => member.GetChildAggregates());
            foreach (var child in children) {
                yield return child;
                foreach (var descendant in child.GetDescendants()) {
                    yield return descendant;
                }
            }
        }
        #endregion リレーション


        #region CodeGenerating
        internal AutoGenerateDbEntityClass ToDbTableModel() {
            // 集約で定義されているカラム
            var pk = Members
                .Where(member => member.IsPrimaryKey)
                .SelectMany(member => member.ToDbColumnModel())
                .ToList();
            var notPk = Members
                .Where(member => !member.IsPrimaryKey)
                .SelectMany(member => member.ToDbColumnModel())
                .ToList();
            // 連番
            if (Parent != null && Parent.IsCollection && pk.Count == 0) {
                pk.Insert(0, new AutoGenerateDbEntityProperty {
                    Virtual = false,
                    CSharpTypeName = "int",
                    PropertyName = "Index",
                    Initializer = null,
                });
            }
            // 親の主キー
            if (Parent != null) {
                pk.InsertRange(0, Parent.Owner.ToDbTableModel().PKColumns);
                // stack overflow
                //_navigationProperties = new() {
                //    new Dto.PropertyTemplate { CSharpTypeName = $"virtual {Parent.Owner.ToDbTableModel().ClassName}", PropertyName = "Parent" },
                //};
            }

            var className = UnderlyingType.GetCustomAttribute<TableAttribute>()?.Name
                ?? UnderlyingType.Name;
            var fullname = Config.EntityNamespace + "." + className;

            return new AutoGenerateDbEntityClass {
                ClassName = className,
                RuntimeFullName = fullname,
                PKColumns = pk,
                NotPKColumns = notPk,
            };
        }
        internal AutoGenerateMvcModelClass ToSearchConditionModel(ViewRenderingContext context) {
            var className = $"{UnderlyingType.Name}__SearchCondition";
            var fullname = Config.MvcModelNamespace + "." + className;
            var props = Members
                .Select(member => new { member, ModelProps = member.ToSearchConditionModel(context) })
                .ToList();
            var template = new AggregateVerticalViewTemplate {
                Members = props.Select(x => KeyValuePair.Create(x.member.Name, string.Join(Environment.NewLine, x.ModelProps.Select(p => p.View)))),
            };
            return new AutoGenerateMvcModelClass {
                ClassName = className,
                RuntimeFullName = fullname,
                Properties = props.SelectMany(p => p.ModelProps).ToArray(),
                View = template.TransformText(),
            };
        }
        internal AutoGenerateMvcModelClass ToSearchResultModel(ViewRenderingContext context) {
            var props = Members
                .SelectMany(member => member.ToSearchResultModel(context))
                .ToList();
            var className = $"{UnderlyingType.Name}__SearchResult";
            var fullname = Config.MvcModelNamespace + "." + className;
            var view = string.Join(Environment.NewLine, props.Select(p => p.View));

            return new AutoGenerateMvcModelClass {
                ClassName = className,
                RuntimeFullName = fullname,
                Properties = props,
                View = view,
            };
        }
        internal AutoGenerateMvcModelClass ToInstanceModel(ViewRenderingContext context) {
            var className = $"{UnderlyingType.Name}";
            var fullname = Config.MvcModelNamespace + "." + className;
            var props = Members
                .Select(member => new { member, ModelProps = member.ToInstanceModel(context) })
                .ToList();
            var template = new AggregateVerticalViewTemplate {
                Members = props.Select(x => KeyValuePair.Create(x.member.Name, string.Join(Environment.NewLine, x.ModelProps.Select(p => p.View)))),
            };
            return new AutoGenerateMvcModelClass {
                ClassName = className,
                RuntimeFullName = fullname,
                Properties = props.SelectMany(p => p.ModelProps).ToArray(),
                View = template.TransformText(),
            };
        }
        #endregion CodeGenerating


        #region Runtime

        #endregion Runtime
    }

    partial class AggregateVerticalViewTemplate {
        internal IEnumerable<KeyValuePair<string, string>> Members { get; set; }
    }
}