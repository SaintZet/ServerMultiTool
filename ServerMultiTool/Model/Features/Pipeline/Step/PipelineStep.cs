using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.Features.Pipeline.Step
{
    public class PipelineStep(string name, string description, int order = 0)
    {
        public Guid Guid { get; } = Guid.NewGuid();

        public string Name { get; private set; } = name;

        public string Description { get; private set; } = description;

        public int Order { get; private set; } = order;

        public List<PipelineOperationBase> Operations { get; set; } = [];

        public PipelineStep AddOperation(PipelineOperationBase operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation), "Operation cannot be null.");

            Operations.Add(operation);
            return this;
        }

        public PipelineStep RemoveOperation(PipelineOperationBase operation)
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

        public PipelineStep UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));

            Name = name;
            return this;
        }

        public PipelineStep UpdateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty.", nameof(description));

            Description = description;
            return this;
        }
    }
}