﻿using System;
using System.Collections.Generic;

namespace HalApplicationBuilder.Core {
    public class AutoGenerateDbEntityClass {
        public string ClassName { get; init; }
        public string RuntimeFullName { get; init; }
        public IReadOnlyList<AutoGenerateDbEntityProperty> PKColumns { get; init; }
        public IReadOnlyList<AutoGenerateDbEntityProperty> NotPKColumns { get; init; }
    }
    public class AutoGenerateMvcModelClass {
        public string ClassName { get; init; }
        public string RuntimeFullName { get; init; }
        public IReadOnlyList<AutoGenerateMvcModelProperty> Properties { get; init; }
    }

    public class AutoGenerateDbEntityProperty {
        public bool Virtual { get; init; }
        public string CSharpTypeName { get; init; }
        public string PropertyName { get; init; }
        public string Initializer { get; init; }
    }
    public class AutoGenerateMvcModelProperty {
        public bool Virtual { get; init; }
        public string CSharpTypeName { get; init; }
        public string PropertyName { get; init; }
        public string Initializer { get; init; }
    }
}
