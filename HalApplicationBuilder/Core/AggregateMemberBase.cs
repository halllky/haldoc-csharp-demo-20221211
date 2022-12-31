﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HalApplicationBuilder.Core {
    public abstract class AggregateMemberBase {

        internal ApplicationSchema Schema { get; init; }
        internal PropertyInfo UnderlyingPropertyInfo { get; init; }

        public string Name => UnderlyingPropertyInfo.Name;
        public bool IsPrimaryKey => UnderlyingPropertyInfo.GetCustomAttribute<KeyAttribute>() != null;
        /// <summary>
        /// Entity Framework エンティティ作成時に連番カラムを生成するか否かに影響
        /// </summary>
        public abstract bool IsCollection { get; }


        #region リレーション
        public Aggregate Owner { get; init; }
        public abstract IEnumerable<Aggregate> GetChildAggregates();
        #endregion リレーション


        #region CodeGenerating
        internal abstract IEnumerable<AutoGenerateDbEntityProperty> ToDbColumnModel();
        internal abstract IEnumerable<AutoGenerateMvcModelProperty> ToSearchConditionModel(ViewRenderingContext context);
        internal abstract IEnumerable<AutoGenerateMvcModelProperty> ToSearchResultModel(ViewRenderingContext context);
        internal abstract IEnumerable<AutoGenerateMvcModelProperty> ToInstanceModel(ViewRenderingContext context);
        #endregion CodeGenerating


        #region Runtime

        #endregion Runtime
    }
}
