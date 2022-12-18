﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace haldoc.Core.Props {
    public interface IAggregateProperty {
        IEnumerable<Schema.EntityColumnDef> ToTableColumn();
        object CreateInstanceValue();
    }
}
