using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Pipeline
{
    public class PipelineStep(string name, string desctiption, int order = 0)
    {
        public Guid Guid { get; } = Guid.NewGuid();

        public string Name { get; } = name;

        public string Description { get; } = desctiption;

        public int Order { get; private set; } = order;

        public List<IPipelineOperation> Operations { get; set; } = [];

        public PipelineStep AddOperation(IPipelineOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation), "Operation cannot be null.");

            Operations.Add(operation);

            return this;
        }

        public PipelineStep RemoveOperation(IPipelineOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation), "Operation cannot be null.");

            Operations.Remove(operation);

            return this;
        }

        public PipelineStep UpdateOrder(int order)
        {
            if (order < 0)
                throw new ArgumentOutOfRangeException(nameof(order), "Order must be a non-negative integer.");

            Order = order;

            return this;
        }
    }
}