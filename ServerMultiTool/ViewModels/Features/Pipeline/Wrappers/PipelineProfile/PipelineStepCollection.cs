using System.Collections.Generic;
using ServerMultiTool.ViewModels.Features.Pipeline.Collections;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineSteps;

namespace ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers
{
    internal class PipelineStepCollection : PipelineStepsCollection
    {
        public PipelineStepCollection(IEnumerable<PipelineStepWrapper> collection) : base(collection)
        {
        }
    }
}
